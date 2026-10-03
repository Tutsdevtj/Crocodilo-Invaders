using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Executado por Tools/verificar_regressoes.ps1 numa cópia temporária do projeto.
[InitializeOnLoad]
public static class CrocodiloRegressionChecks
{
    private const string SessionKey = "CrocodiloRegressionChecks.Active";
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private static int assertions;
    private static Vector2 authoredPlayerStart;

    static CrocodiloRegressionChecks()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    public static void Run()
    {
        CrocodiloProjectSetup.PrepareScene();
        var view = UnityEngine.Object.FindObjectOfType<CrocodiloSceneView>();
        Require(view != null && view.player != null && view.player.Renderer.sprite != null,
            "Jogador/sprite não existem na cena antes de Play.");
        Require(view.menuButtons.Length == 2 && view.menuButtons[0].onClick.GetPersistentEventCount() == 1,
            "Botões e eventos não estão salvos na cena.");
        Require(view.player.hitbox != null && view.enemies[0].hitbox != null,
            "Colliders não existem na cena antes de Play.");
        Require(view.enemyBulletPrefab != null && view.effectPrefabs.Length >= 6,
            "Referências de prefabs não estão salvas na cena.");
        // Simula mover o jogador no editor: Awake deve usar essa posição, sem fixá-la no código.
        view.player.GamePosition += new Vector2(25, -39);
        authoredPlayerStart = view.player.GamePosition;
        SessionState.SetFloat(SessionKey + ".PlayerX", authoredPlayerStart.x);
        SessionState.SetFloat(SessionKey + ".PlayerY", authoredPlayerStart.y);
        SessionState.SetInt(SessionKey + ".Assertions", assertions);
        SessionState.SetBool(SessionKey, true);
        EditorApplication.isPlaying = true;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(SessionKey, false)) return;
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            SessionState.SetBool(SessionKey, false);
            EditorApplication.Exit(0);
            return;
        }
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        try
        {
            authoredPlayerStart = new Vector2(SessionState.GetFloat(SessionKey + ".PlayerX", 0), SessionState.GetFloat(SessionKey + ".PlayerY", 0));
            assertions = SessionState.GetInt(SessionKey + ".Assertions", 0);
            var games = UnityEngine.Object.FindObjectsOfType<CrocodiloGame>();
            Require(games.Length == 1, "A cena deve iniciar exatamente um jogo.");
            CrocodiloGame game = games[0];
            Require((new Vector2(Get<float>(game, "playerX"), Get<float>(game, "playerY")) - authoredPlayerStart).sqrMagnitude < 0.01f,
                "Play ignorou a posição configurada na cena.");
            CheckInspectorSettings(game);
            CheckSpeed(game);
            CheckCollisions(game);
            CheckEnemyBullets(game);
            CheckViewport();
            CheckBossLasers(game);
            CheckMenu(game);
            CheckPause(game);
            Debug.Log("REGRESSION_ALL_OK: " + assertions + " verificacoes em Play Mode.");
            EditorApplication.isPlaying = false;
        }
        catch (Exception error)
        {
            SessionState.SetBool(SessionKey, false);
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
    }

    private static void CheckSpeed(CrocodiloGame game)
    {
        foreach (int fps in new[] { 10, 30, 60, 144 })
        {
            Reset(game);
            Set(game, "movementInput", Vector2.right);
            for (int frame = 0; frame < fps; ++frame) Call(game, "AdvanceSimulation", 1f / fps);
            Require(Mathf.Abs(Get<float>(game, "playerX") - authoredPlayerStart.x - 333f) < 0.01f,
                "Velocidade do jogador depende do FPS: " + fps);
            Require(Mathf.Abs(Get<float>(game, "groundX1") + 333f) < 0.01f,
                "Velocidade do mapa depende do FPS: " + fps);
        }
        Reset(game);
        Set(game, "normalShot", true);
        Set(game, "shotX", 100f);
        Set(game, "shotY", -100f); // Fora dos inimigos para medir apenas o movimento.
        Call(game, "AdvanceSimulation", 0.09f);
        Require(Mathf.Abs(Get<float>(game, "shotX") - 250f) < 0.01f,
            "Projétil deve percorrer 150 pixels em 30 passos de 3 ms.");
        Debug.Log("REGRESSION_SPEED_OK: jogador/mapa a 10, 30, 60 e 144 FPS; velocidade do tiro.");
    }

    private static void CheckCollisions(CrocodiloGame game)
    {
        for (int index = 0; index < 2; ++index)
        {
            Reset(game);
            PlaceEnemy(game, index, 600f, 300f);
            Set(game, "normalShot", true);
            Set(game, "shotX", 620f);
            Set(game, "shotY", 275f); // Antiga colisão fantasma acima da nave.
            Call(game, "UpdateEnemies");
            Require(EnemyHealth(game, index) == 4 && Get<bool>(game, "normalShot"),
                "Tiro acima do sprite atingiu inimigo " + index);

            Set(game, "shotY", 330f);
            Call(game, "UpdateEnemies");
            Require(EnemyHealth(game, index) == 3 && !Get<bool>(game, "normalShot"),
                "Tiro dentro do sprite não atingiu inimigo " + index);

            Reset(game);
            PlaceEnemy(game, index, 600f, 300f);
            Set(game, "specialShot", true);
            Set(game, "specialX", 490f);
            Set(game, "specialY", 310f); // A ponta visível alcança a nave antes da origem do efeito.
            Call(game, "UpdateEnemies");
            Require(Get<int>(game, "kills") == 1 && !Get<bool>(game, "specialShot"),
                "A ponta visível do especial não atingiu inimigo " + index);
        }
        Debug.Log("REGRESSION_COLLISION_OK: ambos os inimigos; tiro acima/dentro e ponta do especial.");
    }

    private static void CheckEnemyBullets(CrocodiloGame game)
    {
        Reset(game);
        PlaceEnemy(game, 0, 899f, 225f);
        Call(game, "UpdateEnemies");
        IList bullets = Get<IList>(game, "enemyBullets");
        Require(bullets.Count == 1, "Inimigo pronto deve disparar uma bala.");
        object bullet = bullets[0];
        for (int i = 0; i < 10; ++i) Call(game, "UpdateEnemies");
        Require(bullets.Count == 1, "Cada passo criou outra bala do mesmo inimigo.");
        Call(game, "HitEnemy", 0, 4, false, 899f, 225f);
        Require(bullets.Count == 1 && ReferenceEquals(bullets[0], bullet),
            "Morte do inimigo apagou sua bala.");
        float oldX = Public<float>(bullet, "X");
        float oldY = Public<float>(bullet, "Y");
        Call(game, "UpdateEnemyBullets");
        Require(Public<float>(bullet, "X") == oldX - 2f && Public<float>(bullet, "Y") == oldY,
            "Respawn teletransportou ou parou a bala anterior.");
        PlaceEnemy(game, 0, 899f, 225f);
        Call(game, "UpdateEnemies");
        Require(bullets.Count == 2, "Novo inimigo não disparou independentemente da bala antiga.");

        Call(game, "StartBoss");
        oldX = Public<float>(bullet, "X");
        Call(game, "Tick");
        Require(Public<float>(bullet, "X") == oldX - 2f,
            "Transição ao chefe congelou a bala em voo.");
        Set(game, "playerX", Public<float>(bullet, "X") - 50f);
        Set(game, "playerY", oldY);
        Call(game, "UpdateEnemyBullets");
        Require(Get<int>(game, "playerHealth") < 30 && !Public<bool>(bullet, "Active"),
            "Bala não causa dano após a morte do inimigo.");

        Reset(game);
        PlaceEnemy(game, 0, 899f, 225f);
        Call(game, "UpdateEnemies");
        bullets = Get<IList>(game, "enemyBullets");
        bullets[0].GetType().GetField("X").SetValue(bullets[0], -25f);
        Call(game, "UpdateEnemyBullets");
        Require(bullets.Count == 0, "Bala fora da tela não foi removida.");
        Call(game, "UpdateEnemies");
        Require(bullets.Count == 1, "Inimigo não volta a atirar após a bala sair da tela.");
        Call(game, "ResetRun");
        Require(bullets.Count == 0, "Nova partida manteve balas da anterior.");
        Debug.Log("REGRESSION_BULLET_OK: morte, respawn, chefe, dano, saída da tela e reinício.");
    }

    private static void CheckViewport()
    {
        var method = typeof(CrocodiloGame).GetMethod("VisibleGameRect", BindingFlags.Static | BindingFlags.NonPublic);
        foreach (Vector2 size in new[] { new Vector2(1200, 720), new Vector2(1920, 1080),
            new Vector2(2560, 1440), new Vector2(1366, 768), new Vector2(720, 1280) })
        {
            Rect visible = (Rect)method.Invoke(null, new object[] { (int)size.x, (int)size.y });
            float scale = Mathf.Min(size.x / 1200f, size.y / 720f);
            Vector2 offset = (size - new Vector2(1200, 720) * scale) * 0.5f;
            Require((visible.min * scale + offset).sqrMagnitude < 0.01f,
                "Fundo não cobre a borda superior/esquerda em " + size);
            Require((visible.max * scale + offset - size).sqrMagnitude < 0.01f,
                "Fundo não cobre a borda inferior/direita em " + size);
        }
        Debug.Log("REGRESSION_VIEWPORT_OK: 1200x720, Full HD, QHD, 1366x768 e retrato.");
    }

    private static void CheckBossLasers(CrocodiloGame game)
    {
        Reset(game);
        Array lasers = Get<Array>(game, "lasers");
        object laser = lasers.GetValue(0);
        SetPublic(laser, "Position", 300f);
        SetPublic(laser, "Vertical", false);
        SetPublic(laser, "RemainingSeconds", 1.5f);
        SetPublic(laser, "Phase", Enum.Parse(laser.GetType().GetField("Phase").FieldType, "Warning"));
        int warningSteps = 0;
        while (Public<object>(laser, "Phase").ToString() == "Warning" && warningSteps < 600)
        {
            Call(game, "UpdateLaser", laser);
            ++warningSteps;
        }
        Require(warningSteps >= 500 && warningSteps <= 501,
            "Aviso do laser deve durar 1,5 segundo antes de causar dano.");
        Require(Get<int>(game, "playerHealth") == 25,
            "Laser não causou dano ao disparar na faixa do jogador.");
        int firingSteps = 0;
        while (Public<object>(laser, "Phase").ToString() == "Firing" && firingSteps < 500)
        {
            Call(game, "UpdateLaser", laser);
            ++firingSteps;
        }
        Require(firingSteps >= 400 && firingSteps <= 401,
            "Feixe deve permanecer visível por 1,2 segundo.");
        Require(Get<int>(game, "playerHealth") == 25,
            "Um único feixe causou dano repetido em cada passo.");

        var viewportMethod = typeof(CrocodiloGame).GetMethod("VisibleGameRect", BindingFlags.Static | BindingFlags.NonPublic);
        var laserRectMethod = typeof(CrocodiloGame).GetMethod("LaserRect", BindingFlags.Static | BindingFlags.NonPublic);
        foreach (Vector2 size in new[] { new Vector2(1200, 720), new Vector2(1920, 1080), new Vector2(720, 1280) })
        {
            Rect visible = (Rect)viewportMethod.Invoke(null, new object[] { (int)size.x, (int)size.y });
            foreach (bool vertical in new[] { false, true })
            {
                SetPublic(laser, "Vertical", vertical);
                Rect beam = (Rect)laserRectMethod.Invoke(null, new object[] { laser, visible, 220f });
                Rect warning = (Rect)laserRectMethod.Invoke(null, new object[] { laser, visible, 7f });
                Rect damage = (Rect)laserRectMethod.Invoke(null, new object[] { laser, visible, 30f });
                Require(vertical ? beam.yMin == visible.yMin && beam.yMax == visible.yMax
                    : beam.xMin == visible.xMin && beam.xMax == visible.xMax,
                    "Feixe cortado antes da borda da tela em " + size);
                Require(beam.center == warning.center && warning.center == damage.center,
                    "Aviso, desenho e colisão do laser estão desalinhados.");
            }
        }

        foreach (int fps in new[] { 30, 60, 144 })
        {
            Reset(game);
            Set(game, "bossActive", true);
            Set(game, "bossX", 750f);
            Set(game, "missileTimer", int.MaxValue);
            for (int frame = 0; frame < fps * 2; ++frame) Call(game, "AdvanceSimulation", 1f / fps);
            Require(Public<object>(lasers.GetValue(0), "Phase").ToString() == "Off",
                "Chefe disparou antes da espera inicial de 2,5 segundos a " + fps + " FPS.");
            for (int frame = 0; frame < fps; ++frame) Call(game, "AdvanceSimulation", 1f / fps);
            Require(Public<object>(lasers.GetValue(0), "Phase").ToString() == "Warning",
                "Tempo do aviso do chefe depende de FPS: " + fps);
        }
        Debug.Log("REGRESSION_LASER_OK: aviso 1,5s, disparo 1,2s, dano único, alinhamento e cobertura horizontal/vertical.");
    }

    private static void CheckMenu(CrocodiloGame game)
    {
        Call(game, "ReturnToMenu");
        Call(game, "MoveMenuSelection", -1);
        Require(Get<int>(game, "menuChoice") == 1, "Navegar para cima deve selecionar Sobre.");
        Call(game, "MoveMenuSelection", 1);
        Require(Get<int>(game, "menuChoice") == 0, "Menu ainda contém uma terceira opção.");
        Require(game.sceneView.menuButtons.Length == 2, "Canvas contém uma terceira opção.");
        game.sceneView.menuButtons[1].onClick.Invoke();
        Require(Get<object>(game, "screen").ToString() == "About", "Sobre deixou de abrir.");
        Call(game, "ReturnToMenu");
        game.sceneView.menuButtons[0].onClick.Invoke();
        Require(Get<object>(game, "screen").ToString() == "Playing", "Jogar deixou de iniciar uma partida.");
        Debug.Log("REGRESSION_MENU_OK: duas opções; teclado, clique, Sobre e nova partida.");
    }

    private static void CheckInspectorSettings(CrocodiloGame game)
    {
        var player = game.sceneView.player;
        float speed = player.moveSpeed;
        int health = player.maxHealth;
        player.moveSpeed = 500; player.maxHealth = 42;
        Reset(game);
        Require(Get<int>(game, "playerHealth") == 42, "Vida do Inspector foi ignorada.");
        Set(game, "movementInput", Vector2.right);
        Call(game, "AdvanceSimulation", 0.09f);
        Require(Mathf.Abs(Get<float>(game, "playerX") - authoredPlayerStart.x - 45) < 0.01f,
            "Velocidade do Inspector foi ignorada.");
        player.moveSpeed = speed; player.maxHealth = health;
        Reset(game);
        var enemy = game.sceneView.enemies[0];
        Vector2 offset = enemy.hitbox.offset;
        enemy.hitbox.offset += new Vector2(0, 0.37f);
        PlaceEnemy(game, 0, 600, 300);
        Set(game, "normalShot", true); Set(game, "shotX", 620f); Set(game, "shotY", 275f);
        Call(game, "UpdateEnemies");
        Require(EnemyHealth(game, 0) == 3, "Editar BoxCollider2D não alterou a colisão usada pelo jogo.");
        enemy.hitbox.offset = offset;
        Require(typeof(CrocodiloGame).GetMethod("OnGUI", PrivateInstance) == null,
            "Renderer OnGUI ainda substitui os objetos editáveis.");
        Debug.Log("REGRESSION_AUTHORING_OK: objetos antes de Play; posição, vida, velocidade, colliders e eventos nativos.");
    }

    private static void CheckPause(CrocodiloGame game)
    {
        Reset(game);
        PlaceEnemy(game, 0, 899f, 225f);
        Call(game, "UpdateEnemies");
        Call(game, "StartBoss");
        Set(game, "normalShot", true); Set(game, "shotX", 200f); Set(game, "shotY", 150f);
        Set(game, "points", 1234); Set(game, "playerHealth", 21);
        Call(game, "SpawnEffect", "coin", 300f, 200f, 1.65f);
        object laser = Get<Array>(game, "lasers").GetValue(0);
        SetPublic(laser, "Phase", Enum.Parse(laser.GetType().GetField("Phase").FieldType, "Warning"));
        SetPublic(laser, "RemainingSeconds", 1.5f);
        object missile = Get<Array>(game, "missiles").GetValue(0);
        SetPublic(missile, "Active", true); SetPublic(missile, "X", 800f); SetPublic(missile, "Y", 400f);
        object bullet = Get<IList>(game, "enemyBullets")[0];
        float bulletX = Public<float>(bullet, "X"), time = Get<float>(game, "simTime");
        float playerX = Get<float>(game, "playerX"), ground = Get<float>(game, "groundX1");
        double accumulated = Get<double>(game, "accumulated");
        game.PauseGame();
        Require(Get<object>(game, "screen").ToString() == "Paused", "Pausa não abriu.");
        Require(game.sceneView.pausePanel.activeSelf && game.sceneView.worldRoot.activeSelf && game.sceneView.hudPanel.activeSelf,
            "Pausa esconde a partida/HUD ou não mostra seus botões.");
        for (int i = 0; i < 600; ++i) Call(game, "AdvanceSimulation", 1f / 60);
        Call(game, "SyncVisuals");
        Require(Get<float>(game, "simTime") == time && Get<double>(game, "accumulated") == accumulated,
            "Pausa continua contando tempo ou acumula passos para depois.");
        Require(Get<float>(game, "playerX") == playerX && Get<float>(game, "groundX1") == ground && Get<float>(game, "shotX") == 200f,
            "Jogador, cenário ou tiro se moveram na pausa.");
        Require(Public<float>(bullet, "X") == bulletX && Public<float>(missile, "X") == 800f && Public<float>(laser, "RemainingSeconds") == 1.5f,
            "Bala, míssil ou laser avançaram na pausa.");
        Require(Get<IList>(game, "effects").Count == 1 && Get<int>(game, "points") == 1234 && Get<int>(game, "playerHealth") == 21,
            "Pausa apagou efeitos ou alterou pontuação/vida.");
        game.sceneView.continueButton.onClick.Invoke();
        Require(Get<object>(game, "screen").ToString() == "Playing" && !game.sceneView.pausePanel.activeSelf,
            "Botão Continuar não retoma a mesma partida.");
        Call(game, "AdvanceSimulation", 0.003f);
        Require(Get<float>(game, "shotX") == 205f && Public<float>(bullet, "X") == bulletX - 2f,
            "Retomar gerou avanço acumulado ou não manteve projéteis.");
        game.PauseGame();
        game.sceneView.pauseMenuButton.onClick.Invoke();
        Require(Get<object>(game, "screen").ToString() == "Menu" && !game.sceneView.pausePanel.activeSelf,
            "Botão Voltar ao menu não fecha a pausa.");
        game.sceneView.menuButtons[0].onClick.Invoke();
        Require(Get<int>(game, "points") == 0 && Get<int>(game, "playerHealth") == 30 && Get<IList>(game, "enemyBullets").Count == 0,
            "Nova partida após a pausa não reiniciou corretamente.");
        Debug.Log("REGRESSION_PAUSE_OK: tempo, tiros, mísseis, lasers, efeitos, HUD, continuar e voltar ao menu.");
    }

    private static void Reset(CrocodiloGame game) { Call(game, "SelectMenu"); }
    private static object Enemy(CrocodiloGame game, int index) { return Get<Array>(game, "enemies").GetValue(index); }
    private static int EnemyHealth(CrocodiloGame game, int index) { return Public<int>(Enemy(game, index), "Health"); }
    private static void PlaceEnemy(CrocodiloGame game, int index, float x, float y)
    {
        object enemy = Enemy(game, index);
        enemy.GetType().GetField("X").SetValue(enemy, x);
        enemy.GetType().GetField("Y").SetValue(enemy, y);
    }
    private static T Public<T>(object value, string name) { return (T)value.GetType().GetField(name).GetValue(value); }
    private static void SetPublic(object value, string name, object fieldValue) { value.GetType().GetField(name).SetValue(value, fieldValue); }
    private static T Get<T>(CrocodiloGame game, string name) { return (T)typeof(CrocodiloGame).GetField(name, PrivateInstance).GetValue(game); }
    private static void Set(CrocodiloGame game, string name, object value) { typeof(CrocodiloGame).GetField(name, PrivateInstance).SetValue(game, value); }
    private static void Call(CrocodiloGame game, string name, params object[] args) { typeof(CrocodiloGame).GetMethod(name, PrivateInstance).Invoke(game, args); }
    private static void Require(bool condition, string message)
    {
        ++assertions;
        if (!condition) throw new Exception(message);
    }
}
