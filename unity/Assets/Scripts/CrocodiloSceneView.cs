using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Referências salvas na cena; este componente não constrói a hierarquia em Play.
[ExecuteAlways]
public sealed class CrocodiloSceneView : MonoBehaviour
{
    public enum Preview { Fase, Menu, Chefe, Sobre, Vitoria, Pausa }
    [Header("Prévia no editor (não altera o início da partida)")]
    public Preview editorPreview = Preview.Fase;
    [Header("Câmera e cenário")]
    public Camera gameCamera;
    public bool fitCameraToScreen = true, fillScreenWithBackground = true;
    public GameObject worldRoot;
    public CrocodiloVisual background, ground, sun;
    public CrocodiloVisual[] clouds;
    [Header("Personagens e tiros salvos na cena")]
    public CrocodiloVisual player, boss, normalShot, specialShot, powerUp, borderAlert;
    public CrocodiloVisual[] enemies, missiles;
    public BoxCollider2D[] bossWingHitboxes;
    public CrocodiloLaserView[] lasers;
    [Header("Prefabs para objetos que nascem durante a partida")]
    public Transform transientRoot;
    public CrocodiloVisual enemyBulletPrefab;
    public EffectPrefab[] effectPrefabs;
    [Header("Telas Canvas")]
    public GameObject menuPanel, aboutPanel, hudPanel, gameOverPanel, victoryPanel, pausePanel;
    public Button continueButton, pauseMenuButton;
    public Button[] menuButtons;
    public RectTransform menuArrow;
    public Vector2 menuArrowOffset = new Vector2(12, -17);
    public GameObject bossHud;
    public Text pointsText, killsText, gameOverScore, victoryPoints, victoryKills;
    public string pointsPrefix = "Pontos: ", killsPrefix = "Inimigos: ";
    public Image playerHealthImage, chargeImage, bossHealthImage;
    public Sprite[] playerHealthSprites, chargeSprites, bossHealthSprites;
    [Header("Áudio")]
    public AudioSource musicSource, effectsSource;
    public Sound[] audioClips;

    [Serializable] public sealed class EffectPrefab { public string animation; public CrocodiloVisual prefab; }
    [Serializable] public sealed class Sound { public string key; public AudioClip clip; }
    private Preview lastPreview = (Preview)(-1);
    private int lastScreen = -1, lastChoice = -1;

    private void Update()
    {
        FitCamera();
        if (!Application.isPlaying && worldRoot != null && player != null && editorPreview != lastPreview) ApplyEditorPreview();
    }

    public void FitCamera()
    {
        if (gameCamera == null) return;
        if (fitCameraToScreen) gameCamera.orthographicSize = Mathf.Max(3.6f, 6f / Mathf.Max(0.01f, gameCamera.aspect));
        Rect visible = VisibleRect;
        if (background != null && fillScreenWithBackground) background.FitRect(visible);
    }

    public Rect VisibleRect
    {
        get
        {
            if (gameCamera == null) return new Rect(0, 0, 1200, 720);
            float h = gameCamera.orthographicSize * 200;
            float w = h * gameCamera.aspect;
            Vector3 center = gameCamera.transform.position;
            return new Rect(center.x * 100 - w / 2, -center.y * 100 - h / 2, w, h);
        }
    }

    public void ApplyEditorPreview()
    {
        if (Application.isPlaying || worldRoot == null || player == null) return;
        lastPreview = editorPreview;
        int mode = editorPreview == Preview.Menu ? 0 : editorPreview == Preview.Sobre ? 1 : editorPreview == Preview.Vitoria ? 4 : editorPreview == Preview.Pausa ? 5 : 2;
        ShowScreen(mode, editorPreview == Preview.Chefe);
        foreach (CrocodiloVisual enemy in enemies) enemy.Show(editorPreview == Preview.Fase, 0);
        boss.Show(editorPreview == Preview.Chefe, 0);
        normalShot.Show(false, 0); specialShot.Show(false, 0); powerUp.Show(false, 0); borderAlert.Show(false, 0);
        foreach (CrocodiloVisual missile in missiles) missile.Show(false, 0);
        foreach (CrocodiloLaserView laser in lasers) laser.Show(0, laser.previewPosition, VisibleRect, 0);
        player.Show(true, 0);
    }

    public void ShowScreen(int mode, bool bossActive)
    {
        bool gameplay = mode == 2 || mode == 3 || mode == 5;
        worldRoot.SetActive(gameplay);
        hudPanel.SetActive(gameplay);
        menuPanel.SetActive(mode == 0);
        aboutPanel.SetActive(mode == 1);
        gameOverPanel.SetActive(mode == 3);
        victoryPanel.SetActive(mode == 4);
        if (pausePanel != null) pausePanel.SetActive(mode == 5);
        bossHud.SetActive(bossActive);
        if (lastScreen != mode) lastChoice = -1;
        lastScreen = mode;
    }

    public void SelectContinueButton()
    {
        if (continueButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
    }

    public void HighlightMenu(int choice)
    {
        if (choice == lastChoice || choice < 0 || choice >= menuButtons.Length) return;
        lastChoice = choice;
        RectTransform button = menuButtons[choice].GetComponent<RectTransform>();
        menuArrow.anchoredPosition = button.anchoredPosition + new Vector2(button.rect.width, 0) + menuArrowOffset;
        if (Application.isPlaying && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(menuButtons[choice].gameObject);
    }

    public void UpdateHud(int hp, int maxHp, int charge, int points, int kills, int bossHp, int maxBossHp)
    {
        pointsText.text = pointsPrefix + points;
        killsText.text = killsPrefix + kills;
        gameOverScore.text = pointsPrefix + points + "    " + killsPrefix + kills;
        victoryPoints.text = points.ToString(); victoryKills.text = kills.ToString();
        int health = Mathf.CeilToInt(hp * 30f / Mathf.Max(1, maxHp));
        int index = health <= 0 ? 8 : health <= 5 ? 7 : health <= 12 ? 6 : health <= 15 ? 5
            : health <= 18 ? 4 : health <= 21 ? 3 : health <= 24 ? 2 : health <= 27 ? 1 : 0;
        SetImage(playerHealthImage, playerHealthSprites, index);
        SetImage(chargeImage, chargeSprites, charge >= 10 ? 0 : charge >= 6 ? 1 : charge >= 4 ? 2 : charge >= 2 ? 3 : 4);
        SetImage(bossHealthImage, bossHealthSprites, Mathf.Clamp(12 - Mathf.CeilToInt(bossHp * 12f / Mathf.Max(1, maxBossHp)), 0, 12));
    }

    private static void SetImage(Image target, Sprite[] sprites, int index)
    {
        if (target != null && sprites != null && index < sprites.Length) target.sprite = sprites[index];
    }

    public CrocodiloVisual CreateBullet() { return Instantiate(enemyBulletPrefab, transientRoot); }
    public CrocodiloVisual CreateEffect(string animation)
    {
        foreach (EffectPrefab effect in effectPrefabs)
            if (effect.animation == animation) return Instantiate(effect.prefab, transientRoot);
        return null;
    }

    public AudioClip SoundClip(string key)
    {
        foreach (Sound sound in audioClips) if (sound.key == key) return sound.clip;
        return null;
    }

    public void ScrollGround(float offset)
    {
        Rect visible = VisibleRect;
        float x = offset;
        while (x > visible.xMin) x -= 1200;
        ground.GamePosition = new Vector2(x, ground.GamePosition.y);
        ground.Renderer.size = new Vector2((visible.xMax - x + 1200) / 100f,
            Mathf.Max(1.94f, (visible.yMax - ground.GamePosition.y) / 100f));
    }
}
