using System;
using System.Collections.Generic;
using UnityEngine;

// Port da fase de "Crocodilo Invaders.por". Todas as posições usam o espaço 1200 x 720.
// O OnGUI é apenas o renderer 2D; a simulação roda a 60 passos por segundo.
public sealed class CrocodiloGame : MonoBehaviour
{
    private const float Width = 1200f;
    private const float Height = 720f;
    private const float StepSeconds = 1f / 60f;

    private enum ScreenMode { Menu, About, Playing, GameOver, Victory }
    private enum LaserPhase { Off, Warning, Firing }

    private sealed class Enemy
    {
        public float X, Y, BulletX;
        public int Health;
    }

    private sealed class Laser
    {
        public bool Vertical;
        public float Position;
        public int Ticks;
        public LaserPhase Phase;
        public bool DamagedPlayer;
    }

    private sealed class Missile
    {
        public float X, Y;
        public bool Active;
    }

    private sealed class Effect
    {
        public string Animation;
        public float X, Y, Started, Duration;
    }

    private static CrocodiloGame instance;
    private readonly Dictionary<string, Texture2D> images = new Dictionary<string, Texture2D>();
    private readonly Dictionary<string, Texture2D[]> animations = new Dictionary<string, Texture2D[]>();
    private readonly Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();
    private readonly List<Effect> effects = new List<Effect>();
    private readonly Laser[] lasers = { new Laser(), new Laser(), new Laser(), new Laser() };
    private readonly Missile[] missiles = { new Missile(), new Missile(), new Missile(), new Missile(), new Missile() };
    private readonly Enemy[] enemies = { new Enemy(), new Enemy() };

    private AudioSource music;
    private AudioSource effectsAudio;
    private ScreenMode screen = ScreenMode.Menu;
    private GUIStyle darkText, lightText, largeText, scoreText;
    private int menuChoice;
    private float accumulated, simTime;
    private float playerX, playerY;
    private int playerHealth, charge, kills, points;
    private bool normalShot, specialShot;
    private float shotX, shotY, specialX, specialY;
    private int powerCount;
    private float powerX, powerY, groundX1, groundX2, cloudX1, cloudX2, cloudY1, cloudY2;
    private float bossX, bossY, bossWinDelay;
    private int bossHealth, missileTimer, nextLaserTimer;
    private bool bossActive, bossDying, nextLaserVertical, borderAlert;
    private float borderAlertUntil;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateGame()
    {
        if (instance != null) return;
        GameObject root = new GameObject("Crocodilo Invaders");
        DontDestroyOnLoad(root);
        root.AddComponent<CrocodiloGame>();
    }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        Application.targetFrameRate = 60;
        music = gameObject.AddComponent<AudioSource>();
        music.loop = true;
        music.volume = 0.65f;
        effectsAudio = gameObject.AddComponent<AudioSource>();
        effectsAudio.volume = 0.8f;
        ResetRun();
        PlayMusic("menu_music");
    }

    private void Update()
    {
        if (screen == ScreenMode.Menu)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) { menuChoice = (menuChoice + 2) % 3; PlaySound("selection"); }
            if (Input.GetKeyDown(KeyCode.DownArrow)) { menuChoice = (menuChoice + 1) % 3; PlaySound("selection"); }
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) SelectMenu();
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mouse = MouseGamePosition();
                if (mouse.x >= 320 && mouse.x <= 775 && mouse.y >= 330 && mouse.y < 665)
                {
                    menuChoice = Mathf.Clamp(Mathf.FloorToInt((mouse.y - 330) / 110), 0, 2);
                    SelectMenu();
                }
            }
            return;
        }

        if (screen == ScreenMode.About)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)) screen = ScreenMode.Menu;
            return;
        }

        if (screen == ScreenMode.GameOver || screen == ScreenMode.Victory)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0)) ReturnToMenu();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape)) { ReturnToMenu(); return; }
        accumulated = Mathf.Min(accumulated + Time.deltaTime, StepSeconds * 5f);
        while (accumulated >= StepSeconds && screen == ScreenMode.Playing)
        {
            Tick();
            accumulated -= StepSeconds;
        }
    }

    private void SelectMenu()
    {
        PlaySound("select");
        if (menuChoice == 0)
        {
            ResetRun();
            screen = ScreenMode.Playing;
            if (music != null) music.Stop();
        }
        else if (menuChoice == 1) screen = ScreenMode.About;
        else Application.Quit(); // Sem efeito no navegador; o menu permanece aberto.
    }

    private void ReturnToMenu()
    {
        screen = ScreenMode.Menu;
        menuChoice = 0;
        PlayMusic("menu_music");
    }

    private void ResetRun()
    {
        simTime = accumulated = 0f;
        playerX = 100; playerY = 319; playerHealth = 30;
        charge = kills = points = powerCount = 0;
        normalShot = specialShot = false;
        shotX = specialX = -1000; shotY = specialY = -1000;
        powerX = -300; powerY = 0;
        groundX1 = 0; groundX2 = 1200;
        cloudX1 = 400; cloudX2 = 750; cloudY1 = 15; cloudY2 = 55;
        enemies[0].X = 1400; enemies[0].Y = 350; enemies[0].BulletX = 900; enemies[0].Health = 4;
        enemies[1].X = 1700; enemies[1].Y = 400; enemies[1].BulletX = 850; enemies[1].Health = 4;
        bossX = 1700; bossY = 90; bossHealth = 108;
        bossActive = bossDying = nextLaserVertical = false;
        bossWinDelay = 0; missileTimer = 180; nextLaserTimer = 200;
        borderAlert = false; borderAlertUntil = 0;
        foreach (Laser laser in lasers) { laser.Phase = LaserPhase.Off; laser.Ticks = 0; laser.DamagedPlayer = false; }
        foreach (Missile missile in missiles) missile.Active = false;
        effects.Clear();
    }

    private void Tick()
    {
        simTime += StepSeconds;
        ScrollMap();
        MovePlayer();
        UpdateShots();
        if (!bossActive) UpdateEnemies();
        UpdatePowerUp();
        if (kills >= 15 && !bossActive) StartBoss();
        if (bossActive) UpdateBoss();
        for (int i = effects.Count - 1; i >= 0; --i)
            if (simTime - effects[i].Started >= effects[i].Duration) effects.RemoveAt(i);
        if (playerHealth <= 0 && screen == ScreenMode.Playing)
        {
            screen = ScreenMode.GameOver;
            PlaySound("gameover");
            if (music != null) music.Stop();
        }
    }

    private void ScrollMap()
    {
        groundX1 -= 1; groundX2 -= 1;
        if (groundX1 <= -1200) groundX1 += 2400;
        if (groundX2 <= -1200) groundX2 += 2400;
        cloudX1 -= 2; cloudX2 -= 2;
        if (cloudX1 <= -200) { cloudX1 = 2000; cloudY1 = cloudY1 + 45 > 200 ? 33 : cloudY1 + 45; }
        if (cloudX2 <= -200) { cloudX2 = 2000; cloudY2 = cloudY2 + 75 > 300 ? 78 : cloudY2 + 75; }
    }

    private void MovePlayer()
    {
        if (Input.GetKey(KeyCode.UpArrow)) playerY -= 1;
        if (Input.GetKey(KeyCode.DownArrow)) playerY += 1;
        if (Input.GetKey(KeyCode.LeftArrow)) playerX -= 1;
        if (Input.GetKey(KeyCode.RightArrow)) playerX += 1;
        playerX = Mathf.Clamp(playerX, 1, 649);
        playerY = Mathf.Clamp(playerY, 4, 449);
        if (playerX >= 649 && Input.GetKey(KeyCode.RightArrow))
        {
            borderAlert = true;
            borderAlertUntil = simTime + 0.9f;
        }
        if (simTime >= borderAlertUntil) borderAlert = false;
    }

    private void UpdateShots()
    {
        if (Input.GetKey(KeyCode.Space) && !normalShot)
        {
            normalShot = true;
            shotX = playerX; shotY = playerY + 45;
            charge = Mathf.Min(10, charge + 1);
            PlaySound("mainC_projectile_sound");
        }
        if (normalShot) { shotX += 5; if (shotX > 1225) normalShot = false; }
        if (Input.GetKey(KeyCode.C) && !specialShot && charge >= 10)
        {
            specialShot = true;
            specialX = playerX; specialY = playerY;
            charge = 0;
            PlaySound("sp_sound");
        }
        if (specialShot) { specialX += 5; if (specialX > 1320) specialShot = false; }
    }

    private void UpdateEnemies()
    {
        Enemy first = enemies[0];
        Enemy second = enemies[1];
        if (first.X > 899) first.X -= 1;
        if (first.X <= 899 && first.Y > 225) first.Y -= 1;
        if (second.X > 850) second.X -= 1;
        if (first.X <= 899 && first.Y <= 225) MoveEnemyBullet(first, first.Y + 10);
        if (second.X <= 850) MoveEnemyBullet(second, second.Y + 10);

        for (int i = 0; i < enemies.Length; ++i)
        {
            Enemy enemy = enemies[i];
            if (enemy.X >= 1200) continue;
            Rect hitbox = new Rect(enemy.X + (i == 0 ? 6 : 0), enemy.Y - 37, 65, 58);
            if (normalShot && Overlaps(new Rect(shotX, shotY, 25, 25), hitbox))
            {
                HitEnemy(i, 1, false, shotX, shotY);
                normalShot = false;
            }
            if (specialShot && Overlaps(new Rect(specialX, specialY, 50, 25), hitbox))
            {
                HitEnemy(i, 4, true, specialX, specialY);
                specialShot = false;
            }
        }
    }

    private void MoveEnemyBullet(Enemy enemy, float y)
    {
        enemy.BulletX -= 2;
        if (enemy.BulletX <= -200) enemy.BulletX = enemy.X;
        if (Mathf.Abs(enemy.BulletX - 840) < 0.1f) PlaySound("enemy_projectile_sound");
        if (Overlaps(new Rect(enemy.BulletX, y, 25, 25), PlayerRect()))
        {
            playerHealth -= 1;
            enemy.BulletX = -200;
            PlaySound("mainC_dmg");
        }
    }

    private void HitEnemy(int index, int damage, bool special, float hitX, float hitY)
    {
        Enemy enemy = enemies[index];
        enemy.Health -= damage;
        SpawnEffect(special ? "sp_hit_effect" : "hit_effect", hitX, hitY, 0.5f);
        PlaySound("enemy_when_dmg");
        if (enemy.Health > 0) return;
        SpawnEffect("enemy_death", enemy.X, enemy.Y, 0.65f);
        PlaySound("enemy_dmg");
        points += special ? (index == 0 ? 2323 : 2967) : (index == 0 ? 800 : 876);
        SpawnEffect("coin", playerX + 60, playerY - 20, 1.65f);
        kills++;
        powerCount += special && index == 0 ? 2 : 1;
        enemy.Health = 4;
        enemy.X = 1400;
        enemy.Y = index == 0 ? UnityEngine.Random.Range(300, 501) : UnityEngine.Random.Range(250, 501);
        enemy.BulletX = enemy.X;
        if (powerCount >= 13 && powerX < -200)
        {
            powerX = 1300;
            powerY = UnityEngine.Random.Range(50, 501);
        }
    }

    private void UpdatePowerUp()
    {
        if (powerX < -200) return;
        powerX -= 1;
        if (Overlaps(PlayerRect(), new Rect(powerX, powerY, 62, 69)))
        {
            playerHealth = Mathf.Min(30, playerHealth + 6);
            powerCount = 0;
            powerX = -300;
            SpawnEffect("hp_up", playerX + 40, playerY - 30, 1.1f);
            PlaySound("powerUp_Sound");
        }
        else if (powerX < -200) { powerX = -300; powerCount = 0; }
    }

    private void StartBoss()
    {
        bossActive = true;
        PlayMusic("boss_music");
    }

    private void UpdateBoss()
    {
        if (bossDying)
        {
            bossY += 2;
            bossX -= 1;
            bossWinDelay -= StepSeconds;
            if (bossWinDelay <= 0)
            {
                screen = ScreenMode.Victory;
                if (music != null) music.Stop();
                PlaySound("win_sound");
            }
            return;
        }

        if (bossX > 750) { bossX -= 1; return; }
        Rect body = new Rect(bossX + 150, bossY + 80, 230, 250);
        Rect wing1 = new Rect(bossX + 45, bossY + 160, 50, 50);
        Rect wing2 = new Rect(bossX + 150, bossY + 160, 50, 50);
        if (normalShot && HitsBoss(new Rect(shotX, shotY, 25, 25), body, wing1, wing2))
        {
            normalShot = false;
            DamageBoss(1, shotX, shotY, false);
        }
        if (specialShot && !bossDying && HitsBoss(new Rect(specialX, specialY, 50, 25), body, wing1, wing2))
        {
            specialShot = false;
            DamageBoss(5, specialX, specialY, true);
        }
        if (bossDying) return;

        bool allLasersOff = true;
        foreach (Laser laser in lasers) if (laser.Phase != LaserPhase.Off) allLasersOff = false;
        if (allLasersOff && --nextLaserTimer <= 0)
        {
            for (int i = 0; i < 2; ++i)
            {
                Laser laser = lasers[nextLaserVertical ? i + 2 : i];
                laser.Vertical = nextLaserVertical;
                laser.Position = nextLaserVertical
                    ? UnityEngine.Random.Range(i == 0 ? 75 : 400, i == 0 ? 501 : 551)
                    : UnityEngine.Random.Range(i == 0 ? 80 : 350, i == 0 ? 301 : 601);
                laser.Ticks = 110;
                laser.Phase = LaserPhase.Warning;
                laser.DamagedPlayer = false;
            }
            nextLaserVertical = !nextLaserVertical;
            nextLaserTimer = nextLaserVertical ? 1500 : 2000;
        }
        foreach (Laser laser in lasers) UpdateLaser(laser);

        if (--missileTimer <= 0)
        {
            missileTimer = 180;
            foreach (Missile missile in missiles)
            {
                if (missile.Active) continue;
                missile.Active = true;
                missile.X = bossX + 50;
                missile.Y = UnityEngine.Random.Range(85, 571);
                PlaySound("bit");
                break;
            }
        }
        foreach (Missile missile in missiles)
        {
            if (!missile.Active) continue;
            missile.X -= 3;
            if (Overlaps(new Rect(missile.X, missile.Y, 30, 15), PlayerRect()))
            {
                playerHealth -= 1;
                missile.Active = false;
                PlaySound("mainC_dmg");
            }
            if (missile.X < -120) missile.Active = false;
        }
    }

    private void UpdateLaser(Laser laser)
    {
        if (laser.Phase == LaserPhase.Off) return;
        if (--laser.Ticks <= 0)
        {
            if (laser.Phase == LaserPhase.Warning)
            {
                laser.Phase = LaserPhase.Firing;
                laser.Ticks = 150;
                PlaySound("laser_beam_sound");
            }
            else laser.Phase = LaserPhase.Off;
        }
        if (laser.Phase != LaserPhase.Firing || laser.DamagedPlayer) return;
        Rect beam = laser.Vertical ? new Rect(laser.Position, 0, 30, 720) : new Rect(0, laser.Position, 1200, 30);
        if (Overlaps(PlayerRect(), beam))
        {
            playerHealth -= 5;
            laser.DamagedPlayer = true; // um disparo causa dano uma vez por faixa
            PlaySound("mainC_dmg");
        }
    }

    private void DamageBoss(int damage, float x, float y, bool special)
    {
        bossHealth -= damage;
        SpawnEffect(special ? "sp_hit_effect" : "hit_effect", x, y, 0.6f);
        PlaySound("enemy_when_dmg");
        if (bossHealth > 0) return;
        bossHealth = 0;
        bossDying = true;
        bossWinDelay = 700f * StepSeconds;
        points += 40000;
        SpawnEffect("coin", playerX + 60, playerY - 20, 1.65f);
        kills += 10;
        SpawnEffect("boss_death_effect", bossX, bossY, 3f);
        PlaySound("boss_death_sound");
        foreach (Laser laser in lasers) laser.Phase = LaserPhase.Off;
        foreach (Missile missile in missiles) missile.Active = false;
    }

    private static bool HitsBoss(Rect projectile, Rect body, Rect wing1, Rect wing2)
    {
        return Overlaps(projectile, body) || Overlaps(projectile, wing1) || Overlaps(projectile, wing2);
    }

    private Rect PlayerRect() { return new Rect(playerX, playerY, 100, 83); }
    private static bool Overlaps(Rect a, Rect b) { return a.Overlaps(b); }

    private void SpawnEffect(string animation, float x, float y, float duration)
    {
        effects.Add(new Effect { Animation = animation, X = x, Y = y, Started = simTime, Duration = duration });
    }

    private void PlaySound(string name)
    {
        if (!sounds.TryGetValue(name, out AudioClip clip))
        {
            clip = Resources.Load<AudioClip>("Audio/" + name);
            sounds[name] = clip;
        }
        if (clip != null && effectsAudio != null) effectsAudio.PlayOneShot(clip);
    }

    private void PlayMusic(string name)
    {
        if (music == null) return;
        music.Stop();
        if (!sounds.TryGetValue(name, out AudioClip clip))
        {
            clip = Resources.Load<AudioClip>("Audio/" + name);
            sounds[name] = clip;
        }
        music.clip = clip;
        if (clip != null) music.Play();
    }

    private Texture2D Image(string name)
    {
        if (!images.TryGetValue(name, out Texture2D texture))
        {
            texture = Resources.Load<Texture2D>("Art/" + name);
            images[name] = texture;
        }
        return texture;
    }

    private Texture2D AnimationFrame(string name, float elapsed)
    {
        if (!animations.TryGetValue(name, out Texture2D[] frames))
        {
            frames = Resources.LoadAll<Texture2D>("Art/Anim/" + name);
            Array.Sort(frames, (a, b) => string.CompareOrdinal(a.name, b.name));
            animations[name] = frames;
        }
        if (frames.Length == 0) return null;
        float frameLength = AnimationFrameSeconds(name);
        int index = Mathf.FloorToInt(Mathf.Max(0f, elapsed) / frameLength) % frames.Length;
        return frames[index];
    }

    private static float AnimationFrameSeconds(string name)
    {
        switch (name)
        {
            case "alert": return 0.09f;
            case "boss_death_effect": return 0.15f;
            case "boss_missile": return 0.12f;
            case "coin": case "hit_effect": return 0.04f;
            case "enemy1": return 0.14f;
            case "enemy_death": case "laser_beam": case "laser_beam_vertical": return 0.05f;
            case "health_box": case "hp_up": case "newboss": case "sun": return 0.08f;
            case "sp_hit_effect": return 0.16f;
            case "Sprojectile_effect": return 0.01f;
            default: return 0.1f;
        }
    }

    private void Draw(Texture2D texture, float x, float y, float width = -1, float height = -1)
    {
        if (texture == null) return;
        GUI.DrawTexture(new Rect(x, y, width > 0 ? width : texture.width, height > 0 ? height : texture.height), texture, ScaleMode.StretchToFill, true);
    }
    private void DrawImage(string name, float x, float y, float width = -1, float height = -1) { Draw(Image(name), x, y, width, height); }
    private void DrawAnimation(string name, float x, float y, float elapsed, float width = -1, float height = -1)
    {
        Draw(AnimationFrame(name, elapsed), x, y, width, height);
    }

    private void OnGUI()
    {
        float scale = Mathf.Min(Screen.width / Width, Screen.height / Height);
        float offsetX = (Screen.width - Width * scale) / 2f;
        float offsetY = (Screen.height - Height * scale) / 2f;
        Matrix4x4 previous = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3(offsetX, offsetY, 0), Quaternion.identity, new Vector3(scale, scale, 1));
        EnsureStyles();
        switch (screen)
        {
            case ScreenMode.Menu: DrawMenu(); break;
            case ScreenMode.About: DrawImage("sobre", 0, 0, Width, Height); break;
            case ScreenMode.Playing: DrawGame(); break;
            case ScreenMode.GameOver: DrawGame(); DrawGameOver(); break;
            case ScreenMode.Victory: DrawVictory(); break;
        }
        GUI.matrix = previous;
    }

    private void EnsureStyles()
    {
        if (darkText != null) return;
        darkText = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, normal = { textColor = Color.black } };
        lightText = new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = Color.white } };
        largeText = new GUIStyle(GUI.skin.label) { fontSize = 42, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
        scoreText = new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
    }

    private void DrawMenu()
    {
        DrawImage("mainMenu", 0, 0, Width, Height);
        DrawImage("seta", 780, menuChoice == 0 ? 347 : menuChoice == 1 ? 457 : 570);
        GUI.Label(new Rect(20, 652, 600, 20), "Setas direcionais para se mover", lightText);
        GUI.Label(new Rect(20, 668, 600, 20), "Pressione ESPAÇO para atacar", lightText);
        GUI.Label(new Rect(20, 684, 600, 20), "Pressione C para ataque especial", lightText);
        GUI.Label(new Rect(20, 700, 650, 20), "Pegue os power-ups para recuperar vida", lightText);
    }

    private void DrawGame()
    {
        DrawImage("background", 0, 0);
        DrawImage("ground", groundX1, 530);
        DrawImage("ground", groundX2, 530);
        DrawAnimation("sun", 510, 70, simTime);
        DrawImage("cloud", cloudX1, cloudY1);
        DrawImage("cloud", cloudX2, cloudY2);

        DrawImage("character", playerX, playerY);
        if (normalShot) DrawImage("projectile", shotX, shotY);
        if (specialShot) DrawAnimation("Sprojectile_effect", specialX, specialY, simTime);
        if (!bossActive)
        {
            foreach (Enemy enemy in enemies)
            {
                DrawAnimation("enemy1", enemy.X, enemy.Y, simTime);
                if (enemy.X <= 899 && enemy.BulletX > -100 && enemy.BulletX < 1200)
                    DrawImage("enemy_projectile", enemy.BulletX, enemy.Y + 10);
            }
        }
        else
        {
            DrawAnimation("newboss", bossX, bossY, simTime);
            DrawBossAttacks();
        }
        if (powerX >= -200) DrawAnimation("health_box", powerX, powerY, simTime);
        foreach (Effect effect in effects)
        {
            float elapsed = simTime - effect.Started;
            DrawAnimation(effect.Animation, effect.X, effect.Animation == "coin" ? effect.Y - elapsed * 60f : effect.Y, elapsed);
        }
        if (borderAlert) DrawAnimation("alert", playerX + 70, playerY - 40, simTime);
        DrawHud();
    }

    private void DrawBossAttacks()
    {
        foreach (Laser laser in lasers)
        {
            if (laser.Phase == LaserPhase.Warning)
                DrawImage(laser.Vertical ? "laser_trace_vertical" : "laser_trace", laser.Vertical ? laser.Position : 0, laser.Vertical ? 0 : laser.Position);
            else if (laser.Phase == LaserPhase.Firing)
                DrawAnimation(laser.Vertical ? "laser_beam_vertical" : "laser_beam", laser.Vertical ? laser.Position - 150 : 0,
                    laser.Vertical ? -150 : laser.Position - 125, simTime);
        }
        foreach (Missile missile in missiles)
            if (missile.Active) DrawAnimation("boss_missile", missile.X, missile.Y, simTime);
    }

    private void DrawHud()
    {
        DrawImage("portrait", 20, 600);
        string healthImage = "hp_bar";
        if (playerHealth <= 0) healthImage = "hp-8";
        else if (playerHealth <= 5) healthImage = "hp-7";
        else if (playerHealth <= 12) healthImage = "hp-6";
        else if (playerHealth <= 15) healthImage = "hp-5";
        else if (playerHealth <= 18) healthImage = "hp-4";
        else if (playerHealth <= 21) healthImage = "hp-3";
        else if (playerHealth <= 24) healthImage = "hp-2";
        else if (playerHealth <= 27) healthImage = "hp-1";
        DrawImage(healthImage, 140, 650);
        DrawImage(charge >= 10 ? "special" : charge >= 6 ? "special-1" : charge >= 4 ? "special-2" : charge >= 2 ? "special-3" : "special-4", 140, 680);
        GUI.Label(new Rect(165, 626, 230, 24), "B. Crocodilo", darkText);
        GUI.Label(new Rect(450, 655, 270, 25), "Pontos: " + points, darkText);
        GUI.Label(new Rect(450, 682, 270, 25), "Inimigos: " + kills, darkText);
        if (!bossActive) return;
        DrawImage("b_portrait", 1070, 600);
        int barIndex = Mathf.Clamp(12 - Mathf.CeilToInt(bossHealth / 9f), 0, 12);
        DrawImage(barIndex == 0 ? "hp_boss" : "hp_boss" + barIndex, 745, 650);
        GUI.Label(new Rect(967, 625, 200, 25), "Executor V-9", darkText);
    }

    private void DrawGameOver()
    {
        Color previous = GUI.color;
        GUI.color = new Color(0, 0, 0, 0.75f);
        Draw(Texture2D.whiteTexture, 260, 190, 680, 300);
        GUI.color = previous;
        GUI.Label(new Rect(260, 220, 680, 80), "FIM DE JOGO", largeText);
        GUI.Label(new Rect(440, 320, 450, 25), "Pontos: " + points + "    Inimigos: " + kills, lightText);
        GUI.Label(new Rect(405, 400, 500, 25), "ENTER ou clique para voltar ao menu", lightText);
    }

    private void DrawVictory()
    {
        DrawImage("win_menu", 0, 0, Width, Height);
        GUI.Label(new Rect(620, 275, 160, 44), points.ToString(), scoreText);
        GUI.Label(new Rect(735, 344, 60, 44), kills.ToString(), scoreText);
        GUI.Label(new Rect(668, 532, 110, 60), "SS", largeText);
        GUI.Label(new Rect(850, 692, 330, 24), "ENTER ou clique para voltar ao menu", lightText);
    }

    private static Vector2 MouseGamePosition()
    {
        float scale = Mathf.Min(Screen.width / Width, Screen.height / Height);
        return new Vector2((Input.mousePosition.x - (Screen.width - Width * scale) / 2f) / scale,
            (Screen.height - Input.mousePosition.y - (Screen.height - Height * scale) / 2f) / scale);
    }
}
