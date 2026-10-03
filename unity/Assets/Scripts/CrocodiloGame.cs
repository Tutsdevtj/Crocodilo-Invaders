using System;
using System.Collections.Generic;
using UnityEngine;

// Simulação do jogo. A cena, os sprites, os colliders e a UI são assets editáveis.
// As posições internas usam pixels; CrocodiloVisual converte para unidades da Unity.
public sealed class CrocodiloGame : MonoBehaviour
{
    private const float Width = 1200f;
    private const float Height = 720f;
    private const float StepSeconds = 0.003f;
    private const float MaxFrameSeconds = 0.1f;
    private const int MenuOptionCount = 2;

    [Header("Objetos salvos na cena")]
    public CrocodiloSceneView sceneView;
    [Header("Partida")]
    public bool startInGameplay;
    [Min(0.1f)] public float simulationSpeed = 1f;
    [Min(1)] public int killsToBoss = 15;
    public Rect playerMovementLimits = new Rect(1, 4, 648, 445);
    [Min(1)] public int powerUpHealing = 6;
    public Vector2 normalShotOffset = new Vector2(0, 45), specialShotOffset = Vector2.zero;
    [Min(0)] public float specialBossDamageMultiplier = 1.25f;
    [Min(0.01f)] public float missileIntervalSeconds = 0.54f;
    [Min(0)] public float bossVictoryDelay = 2.1f;
    [Header("Laser do chefe (segundos)")]
    [Min(0.1f)] public float laserWarningSeconds = 1.5f;
    [Min(0.1f)] public float laserFiringSeconds = 1.2f;
    [Min(0)] public int laserDamage = 5;
    [Min(0)] public float firstLaserDelay = 2.5f;
    [Min(0.1f)] public float horizontalLaserCooldown = 4.5f, verticalLaserCooldown = 6f;
    public Vector2 firstHorizontalLaserRange = new Vector2(80, 300), secondHorizontalLaserRange = new Vector2(350, 600);
    public Vector2 firstVerticalLaserRange = new Vector2(75, 500), secondVerticalLaserRange = new Vector2(400, 550);

    private enum ScreenMode { Menu, About, Playing, GameOver, Victory, Paused }
    private enum LaserPhase { Off, Warning, Firing }

    private sealed class Enemy
    {
        public float X, Y;
        public int Health;
        public EnemyBullet Bullet;
        public CrocodiloVisual View;
    }

    private sealed class EnemyBullet
    {
        public float X, Y;
        public bool Active = true;
        public CrocodiloVisual View;
    }

    private sealed class Laser
    {
        public bool Vertical;
        public float Position;
        public float RemainingSeconds, PhaseStarted;
        public LaserPhase Phase;
        public bool DamagedPlayer;
        public CrocodiloLaserView View;
    }

    private sealed class Missile
    {
        public float X, Y;
        public bool Active;
        public CrocodiloVisual View;
    }

    private sealed class Effect
    {
        public string Animation;
        public float X, Y, Started, Duration;
        public CrocodiloVisual View;
    }

    private readonly Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();
    private readonly List<Effect> effects = new List<Effect>();
    private readonly List<EnemyBullet> enemyBullets = new List<EnemyBullet>();
    private readonly Laser[] lasers = { new Laser(), new Laser(), new Laser(), new Laser() };
    private readonly Missile[] missiles = { new Missile(), new Missile(), new Missile(), new Missile(), new Missile() };
    private readonly Enemy[] enemies = { new Enemy(), new Enemy() };

    private AudioSource music;
    private AudioSource effectsAudio;
    private ScreenMode screen = ScreenMode.Menu;
    private int menuChoice;
    private double accumulated;
    private float simTime;
    private Vector2 movementInput;
    private bool normalFireInput, specialFireInput;
    private float playerX, playerY;
    private int playerHealth, charge, kills, points;
    private bool normalShot, specialShot;
    private float shotX, shotY, specialX, specialY;
    private int powerCount;
    private float powerX, powerY, groundX1, cloudX1, cloudX2, cloudY1, cloudY2;
    private float bossX, bossY, bossWinDelay;
    private int bossHealth, missileTimer;
    private float nextLaserTimer;
    private bool bossActive, bossDying, nextLaserVertical, borderAlert;
    private float borderAlertUntil;
    private Vector2 playerStart, bossSpawn, bossTarget, cloudStart1, cloudStart2;
    private readonly Vector2[] enemySpawns = new Vector2[2], enemyTargets = new Vector2[2];
    private float groundStartX;

    private void Awake()
    {
        if (sceneView == null)
        {
            Debug.LogError("Abra Assets/Scenes/JogoEditavel.unity: CrocodiloGame precisa das referências da cena.", this);
            enabled = false;
            return;
        }
        Application.targetFrameRate = 60;
        music = sceneView.musicSource;
        effectsAudio = sceneView.effectsSource;
        playerStart = sceneView.player.SpawnPosition;
        bossSpawn = sceneView.boss.SpawnPosition;
        bossTarget = sceneView.boss.GamePosition;
        cloudStart1 = sceneView.clouds[0].GamePosition; cloudStart2 = sceneView.clouds[1].GamePosition;
        groundStartX = sceneView.ground.GamePosition.x;
        for (int i = 0; i < enemies.Length; ++i)
        {
            enemies[i].View = sceneView.enemies[i];
            enemySpawns[i] = enemies[i].View.SpawnPosition;
            enemyTargets[i] = enemies[i].View.GamePosition;
        }
        for (int i = 0; i < missiles.Length; ++i) missiles[i].View = sceneView.missiles[i];
        for (int i = 0; i < lasers.Length; ++i) lasers[i].View = sceneView.lasers[i];
        ResetRun();
        screen = startInGameplay ? ScreenMode.Playing : ScreenMode.Menu;
        if (!startInGameplay) PlayMusic("menu_music");
        SyncVisuals();
    }

    private void Update()
    {
        if (screen == ScreenMode.Menu)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) { MoveMenuSelection(-1); PlaySound("selection"); }
            if (Input.GetKeyDown(KeyCode.DownArrow)) { MoveMenuSelection(1); PlaySound("selection"); }
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) SelectMenu();
            return;
        }

        if (screen == ScreenMode.About)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return)) ReturnToMenu();
            return;
        }

        if (screen == ScreenMode.GameOver || screen == ScreenMode.Victory)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) ReturnToMenu();
            return;
        }

        if (screen == ScreenMode.Paused)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) ResumeGame();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape)) { PauseGame(); return; }
        movementInput = new Vector2(
            (Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.LeftArrow) ? 1 : 0),
            (Input.GetKey(KeyCode.DownArrow) ? 1 : 0) - (Input.GetKey(KeyCode.UpArrow) ? 1 : 0));
        normalFireInput = Input.GetKey(KeyCode.Space);
        specialFireInput = Input.GetKey(KeyCode.C);
        AdvanceSimulation(Time.deltaTime);
    }

    private void AdvanceSimulation(float frameSeconds)
    {
        if (screen != ScreenMode.Playing) return;
        // O limite protege contra pausas longas, sem reduzir a velocidade a 30/60/144 FPS.
        accumulated += Mathf.Clamp(frameSeconds, 0f, MaxFrameSeconds) * simulationSpeed;
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
    }

    public void PlayFromMenu() { menuChoice = 0; SelectMenu(); }
    public void AboutFromMenu() { menuChoice = 1; SelectMenu(); }
    public void BackToMenu() { ReturnToMenu(); }
    public void PauseGame()
    {
        if (screen != ScreenMode.Playing) return;
        screen = ScreenMode.Paused;
        ClearInput();
        if (music != null) music.Pause();
        if (effectsAudio != null) effectsAudio.Pause();
        SyncVisuals();
        sceneView.SelectContinueButton();
    }

    public void ResumeGame()
    {
        if (screen != ScreenMode.Paused) return;
        screen = ScreenMode.Playing;
        ClearInput();
        if (music != null) music.UnPause();
        if (effectsAudio != null) effectsAudio.UnPause();
        SyncVisuals();
    }

    private void ClearInput()
    {
        movementInput = Vector2.zero;
        normalFireInput = specialFireInput = false;
    }
    private void LateUpdate() { if (sceneView != null) SyncVisuals(); }

    private void MoveMenuSelection(int direction)
    {
        menuChoice = (menuChoice + direction + MenuOptionCount) % MenuOptionCount;
    }

    private void ReturnToMenu()
    {
        screen = ScreenMode.Menu;
        menuChoice = 0;
        ClearInput();
        if (effectsAudio != null) effectsAudio.Stop();
        PlayMusic("menu_music");
        SyncVisuals();
    }

    private void ResetRun()
    {
        simTime = 0f;
        accumulated = 0;
        ClearInput();
        playerX = playerStart.x; playerY = playerStart.y; playerHealth = sceneView.player.maxHealth;
        charge = kills = points = powerCount = 0;
        normalShot = specialShot = false;
        shotX = specialX = -1000; shotY = specialY = -1000;
        powerX = -300; powerY = 0;
        groundX1 = groundStartX;
        cloudX1 = cloudStart1.x; cloudX2 = cloudStart2.x; cloudY1 = cloudStart1.y; cloudY2 = cloudStart2.y;
        for (int i = 0; i < enemies.Length; ++i)
        {
            enemies[i].X = enemySpawns[i].x; enemies[i].Y = enemySpawns[i].y;
            enemies[i].Bullet = null; enemies[i].Health = enemies[i].View.maxHealth;
        }
        foreach (EnemyBullet bullet in enemyBullets) if (bullet.View != null) Destroy(bullet.View.gameObject);
        enemyBullets.Clear();
        bossX = bossSpawn.x; bossY = bossSpawn.y; bossHealth = sceneView.boss.maxHealth;
        bossActive = bossDying = nextLaserVertical = false;
        bossWinDelay = 0; missileTimer = Mathf.CeilToInt(missileIntervalSeconds / StepSeconds); nextLaserTimer = firstLaserDelay;
        borderAlert = false; borderAlertUntil = 0;
        foreach (Laser laser in lasers)
        {
            laser.Phase = LaserPhase.Off;
            laser.RemainingSeconds = laser.PhaseStarted = 0;
            laser.DamagedPlayer = false;
        }
        foreach (Missile missile in missiles) missile.Active = false;
        foreach (Effect effect in effects) if (effect.View != null) Destroy(effect.View.gameObject);
        effects.Clear();
    }

    private void Tick()
    {
        simTime += StepSeconds;
        ScrollMap();
        MovePlayer();
        UpdateShots();
        if (!bossActive) UpdateEnemies();
        UpdateEnemyBullets();
        UpdatePowerUp();
        if (kills >= killsToBoss && !bossActive) StartBoss();
        if (bossActive) UpdateBoss();
        for (int i = effects.Count - 1; i >= 0; --i)
            if (simTime - effects[i].Started >= effects[i].Duration)
            {
                if (effects[i].View != null) Destroy(effects[i].View.gameObject);
                effects.RemoveAt(i);
            }
        if (playerHealth <= 0 && screen == ScreenMode.Playing)
        {
            screen = ScreenMode.GameOver;
            PlaySound("gameover");
            if (music != null) music.Stop();
        }
    }

    private void ScrollMap()
    {
        groundX1 -= sceneView.ground.moveSpeed * StepSeconds;
        if (groundX1 <= -Width) groundX1 += Width;
        cloudX1 -= sceneView.clouds[0].moveSpeed * StepSeconds; cloudX2 -= sceneView.clouds[1].moveSpeed * StepSeconds;
        if (cloudX1 <= -200) { cloudX1 = 2000; cloudY1 = cloudY1 + 45 > 200 ? 33 : cloudY1 + 45; }
        if (cloudX2 <= -200) { cloudX2 = 2000; cloudY2 = cloudY2 + 75 > 300 ? 78 : cloudY2 + 75; }
    }

    private void MovePlayer()
    {
        playerX += movementInput.x * sceneView.player.moveSpeed * StepSeconds;
        playerY += movementInput.y * sceneView.player.moveSpeed * StepSeconds;
        playerX = Mathf.Clamp(playerX, playerMovementLimits.xMin, playerMovementLimits.xMax);
        playerY = Mathf.Clamp(playerY, playerMovementLimits.yMin, playerMovementLimits.yMax);
        if (playerX >= playerMovementLimits.xMax && movementInput.x > 0)
        {
            borderAlert = true;
            borderAlertUntil = simTime + 0.9f;
        }
        if (simTime >= borderAlertUntil) borderAlert = false;
    }

    private void UpdateShots()
    {
        if (normalFireInput && !normalShot)
        {
            normalShot = true;
            shotX = playerX + normalShotOffset.x; shotY = playerY + normalShotOffset.y;
            charge = Mathf.Min(10, charge + 1);
            PlaySound("mainC_projectile_sound");
        }
        if (normalShot) { shotX += sceneView.normalShot.moveSpeed * StepSeconds; if (shotX > 1225) normalShot = false; }
        if (specialFireInput && !specialShot && charge >= 10)
        {
            specialShot = true;
            specialX = playerX + specialShotOffset.x; specialY = playerY + specialShotOffset.y;
            charge = 0;
            PlaySound("sp_sound");
        }
        if (specialShot) { specialX += sceneView.specialShot.moveSpeed * StepSeconds; if (specialX > 1320) specialShot = false; }
    }

    private void UpdateEnemies()
    {
        Enemy first = enemies[0];
        Enemy second = enemies[1];
        if (first.X > enemyTargets[0].x) first.X = Mathf.Max(enemyTargets[0].x, first.X - first.View.moveSpeed * StepSeconds);
        if (first.X <= enemyTargets[0].x && first.Y > enemyTargets[0].y) first.Y = Mathf.Max(enemyTargets[0].y, first.Y - first.View.moveSpeed * StepSeconds);
        if (second.X > enemyTargets[1].x) second.X = Mathf.Max(enemyTargets[1].x, second.X - second.View.moveSpeed * StepSeconds);
        if (first.X <= enemyTargets[0].x && first.Y <= enemyTargets[0].y) FireEnemyBullet(first);
        if (second.X <= enemyTargets[1].x) FireEnemyBullet(second);

        for (int i = 0; i < enemies.Length; ++i)
        {
            Enemy enemy = enemies[i];
            if (enemy.X >= 1200) continue;
            Rect hitbox = EnemyRect(enemy);
            if (normalShot && Overlaps(NormalShotRect(), hitbox))
            {
                HitEnemy(i, sceneView.normalShot.damage, false, shotX, shotY);
                normalShot = false;
            }
            if (specialShot && Overlaps(SpecialShotRect(), hitbox))
            {
                HitEnemy(i, sceneView.specialShot.damage, true, specialX, specialY);
                specialShot = false;
            }
        }
    }

    private void FireEnemyBullet(Enemy enemy)
    {
        if (enemy.Bullet != null && enemy.Bullet.Active) return;
        enemy.Bullet = new EnemyBullet { X = enemy.X, Y = enemy.Y + 10, View = sceneView.CreateBullet() };
        enemyBullets.Add(enemy.Bullet);
        PlaySound("enemy_projectile_sound");
    }

    private void UpdateEnemyBullets()
    {
        for (int i = enemyBullets.Count - 1; i >= 0; --i)
        {
            EnemyBullet bullet = enemyBullets[i];
            bullet.X -= bullet.View.moveSpeed * StepSeconds;
            if (Overlaps(bullet.View.CollisionRectAt(bullet.X, bullet.Y), PlayerRect()))
            {
                playerHealth -= bullet.View.damage;
                bullet.Active = false;
                PlaySound("mainC_dmg");
            }
            if (bullet.X + 25 < 0) bullet.Active = false;
            if (!bullet.Active) { Destroy(bullet.View.gameObject); enemyBullets.RemoveAt(i); }
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
        enemy.Health = enemy.View.maxHealth;
        enemy.X = enemySpawns[index].x;
        enemy.Y = UnityEngine.Random.Range(enemy.View.respawnY.x, enemy.View.respawnY.y);
        // O novo inimigo pode atirar; a bala anterior continua na lista independente.
        enemy.Bullet = null;
        if (powerCount >= 13 && powerX < -200)
        {
            powerX = 1300;
            powerY = UnityEngine.Random.Range(50, 501);
        }
    }

    private void UpdatePowerUp()
    {
        if (powerX < -200) return;
        powerX -= sceneView.powerUp.moveSpeed * StepSeconds;
        if (Overlaps(PlayerRect(), sceneView.powerUp.CollisionRectAt(powerX, powerY)))
        {
            playerHealth = Mathf.Min(sceneView.player.maxHealth, playerHealth + powerUpHealing);
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

        if (Mathf.Abs(bossX - bossTarget.x) > 0.001f || Mathf.Abs(bossY - bossTarget.y) > 0.001f)
        {
            bossX = Mathf.MoveTowards(bossX, bossTarget.x, sceneView.boss.moveSpeed * StepSeconds);
            bossY = Mathf.MoveTowards(bossY, bossTarget.y, sceneView.boss.moveSpeed * StepSeconds);
            return;
        }
        if (normalShot && HitsBoss(NormalShotRect()))
        {
            normalShot = false;
            DamageBoss(sceneView.normalShot.damage, shotX, shotY, false);
        }
        if (specialShot && !bossDying && HitsBoss(SpecialShotRect()))
        {
            specialShot = false;
            DamageBoss(Mathf.CeilToInt(sceneView.specialShot.damage * specialBossDamageMultiplier), specialX, specialY, true);
        }
        if (bossDying) return;

        bool allLasersOff = true;
        foreach (Laser laser in lasers) if (laser.Phase != LaserPhase.Off) allLasersOff = false;
        if (allLasersOff) nextLaserTimer -= StepSeconds;
        if (allLasersOff && nextLaserTimer <= 0)
        {
            for (int i = 0; i < 2; ++i)
            {
                Laser laser = lasers[nextLaserVertical ? i + 2 : i];
                laser.Vertical = nextLaserVertical;
                Vector2 range = nextLaserVertical ? (i == 0 ? firstVerticalLaserRange : secondVerticalLaserRange)
                    : (i == 0 ? firstHorizontalLaserRange : secondHorizontalLaserRange);
                laser.Position = UnityEngine.Random.Range(range.x, range.y);
                laser.RemainingSeconds = laserWarningSeconds;
                laser.PhaseStarted = simTime;
                laser.Phase = LaserPhase.Warning;
                laser.DamagedPlayer = false;
            }
            nextLaserVertical = !nextLaserVertical;
            nextLaserTimer = nextLaserVertical ? horizontalLaserCooldown : verticalLaserCooldown;
        }
        foreach (Laser laser in lasers) UpdateLaser(laser);

        if (--missileTimer <= 0)
        {
            missileTimer = Mathf.CeilToInt(missileIntervalSeconds / StepSeconds);
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
            missile.X -= missile.View.moveSpeed * StepSeconds;
            if (Overlaps(missile.View.CollisionRectAt(missile.X, missile.Y), PlayerRect()))
            {
                playerHealth -= missile.View.damage;
                missile.Active = false;
                PlaySound("mainC_dmg");
            }
            if (missile.X < -120) missile.Active = false;
        }
    }

    private void UpdateLaser(Laser laser)
    {
        if (laser.Phase == LaserPhase.Off) return;
        laser.RemainingSeconds -= StepSeconds;
        if (laser.RemainingSeconds <= 0)
        {
            if (laser.Phase == LaserPhase.Warning)
            {
                laser.Phase = LaserPhase.Firing;
                laser.RemainingSeconds = laserFiringSeconds;
                laser.PhaseStarted = simTime;
                PlaySound("laser_beam_sound");
            }
            else laser.Phase = LaserPhase.Off;
        }
        if (laser.Phase != LaserPhase.Firing || laser.DamagedPlayer) return;
        Rect beam = LaserRect(laser, sceneView.VisibleRect, laser.View.damageThickness);
        if (Overlaps(PlayerRect(), beam))
        {
            playerHealth -= laserDamage;
            laser.DamagedPlayer = true; // um disparo causa dano uma vez por faixa
            PlaySound("mainC_dmg");
        }
    }

    private static Rect LaserRect(Laser laser, Rect visible, float thickness)
    {
        float center = laser.Position + laser.View.damageThickness / 2f;
        return laser.Vertical
            ? new Rect(center - thickness / 2f, visible.yMin, thickness, visible.height)
            : new Rect(visible.xMin, center - thickness / 2f, visible.width, thickness);
    }

    private void DamageBoss(int damage, float x, float y, bool special)
    {
        bossHealth -= damage;
        SpawnEffect(special ? "sp_hit_effect" : "hit_effect", x, y, 0.6f);
        PlaySound("enemy_when_dmg");
        if (bossHealth > 0) return;
        bossHealth = 0;
        bossDying = true;
        bossWinDelay = bossVictoryDelay;
        points += 40000;
        SpawnEffect("coin", playerX + 60, playerY - 20, 1.65f);
        kills += 10;
        SpawnEffect("boss_death_effect", bossX, bossY, 3f);
        PlaySound("boss_death_sound");
        foreach (Laser laser in lasers) laser.Phase = LaserPhase.Off;
        foreach (Missile missile in missiles) missile.Active = false;
    }

    private bool HitsBoss(Rect projectile)
    {
        if (Overlaps(projectile, sceneView.boss.CollisionRectAt(bossX, bossY))) return true;
        foreach (BoxCollider2D wing in sceneView.bossWingHitboxes)
            if (Overlaps(projectile, CrocodiloVisual.ColliderRectAt(wing, sceneView.boss.transform, bossX, bossY))) return true;
        return false;
    }

    private Rect PlayerRect() { return sceneView.player.CollisionRectAt(playerX, playerY); }
    private Rect NormalShotRect() { return sceneView.normalShot.CollisionRectAt(shotX, shotY); }
    private Rect SpecialShotRect() { return sceneView.specialShot.CollisionRectAt(specialX, specialY); }
    private static Rect EnemyRect(Enemy enemy)
    {
        return enemy.View.CollisionRectAt(enemy.X, enemy.Y);
    }
    private static bool Overlaps(Rect a, Rect b) { return a.Overlaps(b); }

    private void SpawnEffect(string animation, float x, float y, float duration)
    {
        effects.Add(new Effect { Animation = animation, X = x, Y = y, Started = simTime, Duration = duration, View = sceneView.CreateEffect(animation) });
    }

    private void PlaySound(string name)
    {
        if (!sounds.TryGetValue(name, out AudioClip clip))
        {
            clip = sceneView.SoundClip(name);
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
            clip = sceneView.SoundClip(name);
            sounds[name] = clip;
        }
        music.clip = clip;
        if (clip != null) music.Play();
    }

    private void SyncVisuals()
    {
        sceneView.FitCamera();
        sceneView.ShowScreen((int)screen, bossActive);
        if (screen == ScreenMode.Menu) sceneView.HighlightMenu(menuChoice);
        sceneView.ScrollGround(groundX1);
        sceneView.sun.Show(true, simTime);
        sceneView.clouds[0].GamePosition = new Vector2(cloudX1, cloudY1);
        sceneView.clouds[1].GamePosition = new Vector2(cloudX2, cloudY2);
        sceneView.player.GamePosition = new Vector2(playerX, playerY);
        sceneView.player.Show(true, simTime);
        sceneView.normalShot.GamePosition = new Vector2(shotX, shotY);
        sceneView.normalShot.Show(normalShot, simTime);
        sceneView.specialShot.GamePosition = new Vector2(specialX, specialY);
        sceneView.specialShot.Show(specialShot, simTime);
        foreach (Enemy enemy in enemies)
        {
            enemy.View.GamePosition = new Vector2(enemy.X, enemy.Y);
            enemy.View.Show(!bossActive, simTime);
        }
        sceneView.boss.GamePosition = new Vector2(bossX, bossY);
        sceneView.boss.Show(bossActive, simTime);
        foreach (EnemyBullet bullet in enemyBullets)
        {
            bullet.View.GamePosition = new Vector2(bullet.X, bullet.Y);
            bullet.View.Show(true, simTime);
        }
        foreach (Missile missile in missiles)
        {
            missile.View.GamePosition = new Vector2(missile.X, missile.Y);
            missile.View.Show(bossActive && missile.Active, simTime);
        }
        foreach (Laser laser in lasers)
            laser.View.Show(bossActive ? (int)laser.Phase : 0, laser.Position, sceneView.VisibleRect, simTime - laser.PhaseStarted);
        sceneView.powerUp.GamePosition = new Vector2(powerX, powerY);
        sceneView.powerUp.Show(powerX >= -200, simTime);
        sceneView.borderAlert.GamePosition = new Vector2(playerX + 70, playerY - 40);
        sceneView.borderAlert.Show(borderAlert, simTime);
        foreach (Effect effect in effects)
        {
            if (effect.View == null) continue;
            float elapsed = simTime - effect.Started;
            effect.View.GamePosition = new Vector2(effect.X, effect.Animation == "coin" ? effect.Y - elapsed * 60f : effect.Y);
            effect.View.Show(true, elapsed);
        }
        sceneView.UpdateHud(playerHealth, sceneView.player.maxHealth, charge, points, kills, bossHealth, sceneView.boss.maxHealth);
    }

    private static Rect VisibleGameRect(int screenWidth, int screenHeight)
    {
        float scale = Mathf.Min(Mathf.Max(1, screenWidth) / Width, Mathf.Max(1, screenHeight) / Height);
        float visibleWidth = Mathf.Max(1, screenWidth) / scale;
        float visibleHeight = Mathf.Max(1, screenHeight) / scale;
        return new Rect((Width - visibleWidth) / 2f, (Height - visibleHeight) / 2f, visibleWidth, visibleHeight);
    }
}
