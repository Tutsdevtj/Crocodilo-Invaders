using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class CardRewardView : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private CanvasGroup window;
    [SerializeField] private PauseBackdrop backdrop;
    [SerializeField] private CardOptionView[] options;
    [SerializeField] private Button reroll;
    [SerializeField] private TMP_Text rerollLabel;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text totals;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip revealSound;
    [SerializeField] private AudioClip chooseSound;
    [SerializeField, Min(.01f)] private float dealDuration = .38f;
    [SerializeField, Min(.01f)] private float flipDuration = .32f;
    [SerializeField, Min(0f)] private float stagger = .1f;
    private bool paused;
    private RectTransform layoutRoot;
    private float layoutWidth = -1f;
    public event Action<int> ChoiceRequested;
    public event Action RerollRequested;
    public CardOptionView[] Options => options;
    public Button RerollButton => reroll;
    public bool IsVisible => content.activeSelf;
    private float Delta => paused ? 0f : Time.unscaledDeltaTime;

    private void Awake()
    {
        ApplyCardSpacing();
        audioSource.ignoreListenerPause = true;
        for (int i = 0; i < options.Length; i++)
        {
            int index = i;
            options[i].Button.onClick.AddListener(() => ChoiceRequested?.Invoke(index));
        }
        reroll.onClick.AddListener(() => RerollRequested?.Invoke());
        content.SetActive(false);
    }

    public void ApplyCardSpacing()
    {
        if (layoutRoot == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null) layoutRoot = (RectTransform)canvas.rootCanvas.transform;
        }
        float width = layoutRoot != null && layoutRoot.rect.width > 0f ? layoutRoot.rect.width : 1200f;
        if (Mathf.Abs(width - layoutWidth) < .1f) return;
        layoutWidth = width;
        float fit = Mathf.Min(1f, width / 1140f);
        // Referência 1200x720: vão de 100 px entre faces de 250 px (antes 50).
        for (int i = 0; i < options.Length; i++)
        {
            RectTransform slot = (RectTransform)options[i].transform;
            Vector2 position = slot.anchoredPosition;
            position.x = (i - (options.Length - 1) * .5f) * 350f * fit;
            slot.anchoredPosition = position;
            slot.localScale = Vector3.one * fit;
        }
    }
    private void LateUpdate() => ApplyCardSpacing();

    public void SetPaused(bool value)
    {
        paused = value;
        foreach (CardOptionView option in options) option.SetPaused(value);
    }
    public void SetInteraction(bool ready, int remaining)
    {
        bool unlimited = DevelopmentCardSession.IsActive;
        bool available = unlimited || remaining > 0;
        window.interactable = ready;
        foreach (CardOptionView option in options) option.SetInteractive(ready);
        reroll.interactable = ready && available;
        foreach (CardOptionView option in options)
        {
            Navigation navigation = option.Button.navigation;
            navigation.selectOnDown = reroll.interactable ? reroll : null;
            option.Button.navigation = navigation;
        }
        rerollLabel.text = unlimited ? "REROLL INFINITO  ·  DEV  [R]"
            : remaining > 0 ? "REROLL GRATUITO  ·  1/1  [R]" : "REROLL USADO  ·  0/1";
        rerollLabel.color = available ? new Color(1f, .85f, .55f) : new Color(.6f, .6f, .65f);
        if (ready && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(options[1].Button.gameObject);
    }

    public IEnumerator Open(int wave, UpgradeCard[] offers, UpgradeCardCatalog catalog, PlayerRunUpgrades upgrades)
    {
        SetPaused(false);
        backdrop.Capture();
        content.SetActive(true);
        title.text = $"ONDA {wave} CONCLUÍDA";
        Bind(offers, catalog, upgrades);
        for (int i = 0; i < options.Length; i++) options[i].SetPose(-620f, .86f, (i - 1) * 9f, 0f);
        SetInteraction(false, 1);
        window.alpha = 0f;
        for (float time = 0f; time < .16f; time += Delta)
        {
            window.alpha = Mathf.Clamp01(time / .16f);
            yield return null;
        }
        window.alpha = 1f;
        yield return DealAndReveal(offers, true);
    }

    public IEnumerator Reroll(UpgradeCard[] offers, UpgradeCardCatalog catalog, PlayerRunUpgrades upgrades)
    {
        for (float time = 0f; time < .18f; time += Delta)
        {
            float t = Mathf.Clamp01(time / .18f);
            foreach (CardOptionView option in options)
            {
                option.SetFlip(Mathf.Abs(Mathf.Cos(t * Mathf.PI)));
                if (t >= .5f) option.ShowFront(false);
            }
            yield return null;
        }
        Bind(offers, catalog, upgrades);
        yield return DealAndReveal(offers, false);
    }

    private void Bind(UpgradeCard[] offers, UpgradeCardCatalog catalog, PlayerRunUpgrades upgrades)
    {
        for (int i = 0; i < options.Length; i++)
        {
            UpgradeCard card = offers[i];
            string name = card.kind == UpgradeKind.AttackDamage ? "Dano" : card.kind == UpgradeKind.AttackSpeed ? "Tiros/s"
                : card.kind == UpgradeKind.MaximumHealth ? "HP máx." : card.kind == UpgradeKind.MoveSpeed ? "Velocidade" : "Especial";
            string format = card.kind == UpgradeKind.MaximumHealth ? "0" : "0.##";
            string comparison = $"{name}: {upgrades.Value(card.kind).ToString(format)} → {upgrades.Value(card.kind, card.percent).ToString(format)}\nTotal: +{upgrades.Bonus(card.kind) + card.percent}%";
            options[i].Bind(card, catalog.Artwork(card), comparison);
            options[i].SetPose(0f, 1f, 0f, 1f);
        }
        RefreshTotals(upgrades);
    }

    private IEnumerator DealAndReveal(UpgradeCard[] offers, bool entering)
    {
        bool[] sounded = new bool[options.Length];
        float entry = entering ? dealDuration : .08f;
        float flipStart = entry + .08f;
        var starts = new float[options.Length];
        var durations = new float[options.Length];
        float finish = 0f;
        for (int i = 0; i < options.Length; i++)
        {
            // A mais rara fecha a revelação; a posição das escolhas permanece igual.
            int order = 0;
            for (int j = 0; j < offers.Length; j++)
                if (offers[j].rarity < offers[i].rarity || (offers[j].rarity == offers[i].rarity && j < i)) order++;
            starts[i] = flipStart + order * Mathf.Max(stagger, .2f);
            durations[i] = flipDuration + (int)offers[i].rarity * .055f;
            finish = Mathf.Max(finish, starts[i] + durations[i]);
        }
        for (float time = 0f; time < finish; time += Delta)
        {
            for (int i = 0; i < options.Length; i++)
            {
                float local = time - i * stagger;
                float arriving = EaseOut(local / entry);
                options[i].SetPose(entering ? Mathf.Lerp(-620f, 0f, arriving) : 0f,
                    Mathf.Lerp(.86f, 1f, arriving), entering ? Mathf.Lerp((i - 1) * 9f, 0f, arriving) : 0f, arriving);
                float turning = Mathf.Clamp01((time - starts[i]) / durations[i]);
                options[i].SetFlip(Mathf.Abs(Mathf.Cos(turning * Mathf.PI)));
                if (turning >= .5f)
                {
                    options[i].ShowFront(true);
                    if (!sounded[i])
                    {
                        sounded[i] = true;
                        if (offers[i].rarity < UpgradeRarity.Rare || !options[i].HasRarityFeedback)
                            Play(revealSound, .94f + i * .06f + (int)offers[i].rarity * .035f);
                    }
                }
            }
            yield return null;
        }
        foreach (CardOptionView option in options) { option.ShowFront(true); option.SetFlip(1f); option.SetPose(0f, 1f, 0f, 1f); }
    }

    public IEnumerator Choose(int index, PlayerRunUpgrades upgrades)
    {
        Play(chooseSound, 1f);
        options[index].CelebrateChoice();
        RefreshTotals(upgrades);
        float duration = .32f + (int)options[index].Rarity * .06f;
        for (float time = 0f; time < duration; time += Delta)
        {
            float t = EaseOut(time / duration);
            for (int i = 0; i < options.Length; i++) options[i].SetPose(i == index ? t * 28f : -t * 12f,
                i == index ? 1f + t * .06f : 1f - t * .03f, 0f, i == index ? 1f : 1f - t);
            yield return null;
        }
        for (float time = 0f; time < .18f; time += Delta) { window.alpha = 1f - Mathf.Clamp01(time / .18f); yield return null; }
        HideImmediately();
    }
    private void RefreshTotals(PlayerRunUpgrades upgrades)
    {
        totals.text = $"DANO +{upgrades.Bonus(UpgradeKind.AttackDamage)}%     CADÊNCIA +{upgrades.Bonus(UpgradeKind.AttackSpeed)}%     HP +{upgrades.Bonus(UpgradeKind.MaximumHealth)}%     MOVIMENTO +{upgrades.Bonus(UpgradeKind.MoveSpeed)}%     ESPECIAL +{upgrades.Bonus(UpgradeKind.SpecialDamage)}%";
    }
    private void Play(AudioClip clip, float pitch) { if (clip == null) return; audioSource.pitch = pitch; audioSource.PlayOneShot(clip, .55f); }
    public void HideImmediately() { content.SetActive(false); backdrop.Release(); }
    private static float EaseOut(float progress) { float t = 1f - Mathf.Clamp01(progress); return 1f - t * t * t; }
}
