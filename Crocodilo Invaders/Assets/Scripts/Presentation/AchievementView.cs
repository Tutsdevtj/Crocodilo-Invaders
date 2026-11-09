using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class AchievementView : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] AchievementRow[] rows;
    [SerializeField] Sprite[] icons;
    [SerializeField] UnityEngine.UI.Button closeButton, previousButton, nextButton;
    [SerializeField] UnityEngine.UI.Button[] filterButtons;
    [SerializeField] UnityEngine.UI.Image completionFill;
    [SerializeField] TMP_Text summary, pagination, empty, saveStatus;
    [Header("Notification")]
    [SerializeField] RectTransform toast;
    [SerializeField] CanvasGroup toastGroup;
    [SerializeField] UnityEngine.UI.Image toastIcon;
    [SerializeField] TMP_Text toastTitle, toastDescription;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip unlockSound;
    readonly Queue<AchievementDefinition> pending = new Queue<AchievementDefinition>();
    readonly List<AchievementDefinition> visible = new List<AchievementDefinition>();
    AchievementDefinition active;
    Action closed;
    Vector2 toastHome;
    int page, filter;
    float clock, gap;
    public bool IsVisible => panel.activeSelf;
    public int PendingCount => pending.Count;
    public int Page => page;
    public int PageCount => Mathf.Max(1, (visible.Count + rows.Length - 1) / rows.Length);
    public string CurrentToastId => active?.Id;
    public float ToastClock => clock;
    public int SoundPlayCount { get; private set; }
    public bool ToastVisible => toastGroup.alpha > 0f;

    void Awake()
    {
        panel.SetActive(false); toastHome = toast.anchoredPosition;
        toastGroup.alpha = 0; toastGroup.interactable = toastGroup.blocksRaycasts = false;
        GameAudioSettings.Route(audioSource, AudioChannel.Effects);
        audioSource.playOnAwake = false; audioSource.spatialBlend = 0; audioSource.volume = .42f;
        closeButton.onClick.AddListener(Close);
        previousButton.onClick.AddListener(() => SetPage(page - 1));
        nextButton.onClick.AddListener(() => SetPage(page + 1));
        for (int i = 0; i < filterButtons.Length; i++) { int index = i; filterButtons[i].onClick.AddListener(() => SetFilter(index)); }
    }
    void OnEnable() { Achievements.Unlocked += Enqueue; Achievements.Changed += RefreshIfOpen; }
    void OnDisable() { Achievements.Unlocked -= Enqueue; Achievements.Changed -= RefreshIfOpen; }
    public void Open(Action onClosed)
    {
        if (IsVisible) return;
        closed = onClosed; panel.SetActive(true); filter = page = 0; Refresh();
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(closeButton.gameObject);
    }
    public void Close()
    {
        if (!IsVisible) return;
        panel.SetActive(false); var callback = closed; closed = null; callback?.Invoke();
    }
    public void HideImmediately() { panel.SetActive(false); closed = null; }
    public void SetPage(int value) { page = Mathf.Clamp(value, 0, PageCount - 1); Refresh(); }
    public void SetFilter(int value) { filter = Mathf.Clamp(value, 0, 2); page = 0; Refresh(); }
    void RefreshIfOpen() { if (IsVisible) Refresh(); }
    void Refresh()
    {
        visible.Clear();
        foreach (var definition in AchievementCatalog.Released)
        {
            bool unlocked = Achievements.Store.Get(definition.Id).Unlocked;
            if (filter == 0 || (filter == 1 && unlocked) || (filter == 2 && !unlocked)) visible.Add(definition);
        }
        page = Mathf.Clamp(page, 0, PageCount - 1);
        for (int i = 0; i < rows.Length; i++)
        {
            int index = page * rows.Length + i;
            rows[i].gameObject.SetActive(index < visible.Count);
            if (index < visible.Count)
            { var d = visible[index]; rows[i].Bind(d, Achievements.Store.Get(d.Id), Icon(d)); }
        }
        int count = Achievements.Store.UnlockedCount;
        summary.text = count + " / " + AchievementCatalog.Released.Count + " CONQUISTAS DESBLOQUEADAS";
        completionFill.fillAmount = count / (float)AchievementCatalog.Released.Count;
        pagination.text = "PÁGINA " + (page + 1) + " / " + PageCount;
        previousButton.interactable = page > 0; nextButton.interactable = page + 1 < PageCount;
        empty.gameObject.SetActive(visible.Count == 0);
        empty.text = filter == 1 ? "SUA PRIMEIRA MEDALHA ESTÁ ESPERANDO.\nJogue para desbloquear conquistas." : "TODAS AS MEDALHAS CONQUISTADAS!";
        saveStatus.text = Achievements.Store.Error ?? "PROGRESSO SALVO NESTE COMPUTADOR";
        for (int i = 0; i < filterButtons.Length; i++)
        {
            var colors = filterButtons[i].colors;
            colors.normalColor = i == filter ? new Color(.55f, .64f, .50f) : Color.white;
            filterButtons[i].colors = colors;
        }
        if (EventSystem.current != null)
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if ((selected == previousButton.gameObject && !previousButton.interactable) ||
                (selected == nextButton.gameObject && !nextButton.interactable)) EventSystem.current.SetSelectedGameObject(closeButton.gameObject);
        }
    }
    Sprite Icon(AchievementDefinition d)
    { for (int i = 0; i < AchievementCatalog.All.Count; i++) if (AchievementCatalog.All[i].Id == d.Id) return i < icons.Length ? icons[i] : null; return null; }
    void Enqueue(AchievementDefinition definition) { if (definition != null) pending.Enqueue(definition); }
    void Update()
    {
        bool held = AchievementRuntime.Instance != null && AchievementRuntime.Instance.PresentationHeld;
        if (held) { toastGroup.alpha = 0; if (audioSource.isPlaying) audioSource.Pause(); return; }
        if (active != null && !audioSource.isPlaying && clock < .7f) audioSource.UnPause();
        float dt = Mathf.Min(Time.unscaledDeltaTime, .1f);
        if (active == null)
        {
            toastGroup.alpha = 0;
            if (gap > 0) { gap -= dt; return; }
            if (pending.Count == 0) return;
            active = pending.Dequeue(); clock = 0;
            toastTitle.text = active.Name; toastDescription.text = active.Description;
            toastIcon.sprite = Icon(active);
            if (unlockSound != null) { audioSource.PlayOneShot(unlockSound); SoundPlayCount++; }
        }
        clock += dt;
        const float enter = .22f, hold = 3f, exit = .2f;
        float alpha = clock < enter ? 1f - Mathf.Pow(1f - clock / enter, 3f) : clock > enter + hold ? 1f - (clock - enter - hold) / exit : 1f;
        alpha = Mathf.Clamp01(alpha);
        toastGroup.alpha = alpha;
        toast.anchoredPosition = toastHome + Vector2.right * ((toast.rect.width + 24) * (1f - alpha));
        if (clock >= enter + hold + exit) { active = null; toastGroup.alpha = 0; gap = .18f; }
    }
}
