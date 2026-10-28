using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class CardOptionView : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button button;
    [SerializeField] private RectTransform motion;
    [SerializeField] private RectTransform hover;
    [SerializeField] private RectTransform flip;
    [SerializeField] private CanvasGroup visibility;
    [SerializeField] private Image face;
    [SerializeField] private Outline outline;
    [SerializeField] private Sprite back;
    [SerializeField] private TMP_Text preview;
    [SerializeField] private CardRarityFeedback rarityFeedback;
    private Sprite front;
    private Color rarityColor;
    private bool selected, pointed, paused;
    private float focus;
    private float revealFlash;
    private UpgradeRarity rarity;
    public Button Button => button;
    public bool FrontShowing { get; private set; }
    public bool HasRarityFeedback => rarityFeedback != null;
    public UpgradeRarity Rarity => rarity;

    public void Bind(UpgradeCard card, Sprite artwork, string comparison)
    {
        front = artwork;
        rarity = card.rarity;
        rarityColor = ColorFor(card.rarity);
        if (rarityFeedback != null)
        {
            rarityFeedback.Configure(face.rectTransform.rect.size);
            rarityFeedback.Bind(card.rarity);
        }
        preview.text = comparison;
        selected = pointed = false;
        focus = 0f;
        revealFlash = 0f;
        hover.localScale = Vector3.one;
        hover.localRotation = Quaternion.identity;
        hover.anchoredPosition = Vector2.zero;
        ShowFront(false);
        SetFlip(1f);
    }
    public void ShowFront(bool show)
    {
        if (show && !FrontShowing)
        {
            revealFlash = 1f;
            if (rarityFeedback != null) rarityFeedback.Reveal();
        }
        else if (!show && rarityFeedback != null) rarityFeedback.Hide();
        FrontShowing = show;
        face.sprite = show ? front : back;
        preview.gameObject.SetActive(show);
        outline.enabled = show;
    }
    public void SetPose(float y, float scale, float angle, float alpha)
    {
        motion.anchoredPosition = new Vector2(0f, y);
        motion.localScale = Vector3.one * scale;
        motion.localRotation = Quaternion.Euler(0f, 0f, angle);
        visibility.alpha = alpha;
    }
    public void SetFlip(float width) => flip.localScale = new Vector3(Mathf.Max(.01f, width), 1f, 1f);
    public void CelebrateChoice() { if (rarityFeedback != null) rarityFeedback.Celebrate(); }
    public void SetPaused(bool value)
    {
        paused = value;
        if (rarityFeedback != null) rarityFeedback.SetPaused(value);
    }
    public void SetInteractive(bool value)
    {
        button.interactable = value;
        if (!value) selected = pointed = false;
    }
    private void LateUpdate()
    {
        if (paused) return;
        revealFlash = Mathf.MoveTowards(revealFlash, 0f, Time.unscaledDeltaTime * 3.5f);
        focus = Mathf.MoveTowards(focus, (selected || pointed) && button.interactable ? 1f : 0f, Time.unscaledDeltaTime * 9f);
        float clock = rarityFeedback != null ? rarityFeedback.Elapsed : 0f;
        float impact = rarityFeedback != null ? rarityFeedback.Impact : 0f;
        float pulse = FrontShowing && rarity >= UpgradeRarity.Legendary ? Mathf.Sin(clock * 3f) * .006f : 0f;
        float shake = rarity >= UpgradeRarity.Legendary ? Mathf.Sin(clock * 58f) * impact : 0f;
        hover.localScale = Vector3.one * (1f + focus * .045f + pulse + impact * .06f);
        hover.anchoredPosition = new Vector2(shake * 2f, focus * 9f + impact * 6f);
        hover.localRotation = Quaternion.Euler(0f, 0f, shake * .8f);
        if (rarityFeedback != null) rarityFeedback.SetFocus(focus);
        outline.effectColor = new Color(rarityColor.r, rarityColor.g, rarityColor.b,
            Mathf.Clamp01(.2f + focus * .55f + revealFlash * ((int)rarity + 1) * .09f));
    }
    public void OnSelect(BaseEventData data) => selected = true;
    public void OnDeselect(BaseEventData data) => selected = false;
    public void OnPointerEnter(PointerEventData data)
    {
        pointed = true;
        if (button.interactable && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
    public void OnPointerExit(PointerEventData data) => pointed = false;
    public static Color ColorFor(UpgradeRarity value) => value == UpgradeRarity.Common ? new Color(.75f,.8f,.85f)
        : value == UpgradeRarity.Rare ? new Color(.25f,.65f,1f) : value == UpgradeRarity.Epic ? new Color(.73f,.36f,1f)
        : value == UpgradeRarity.Legendary ? new Color(1f,.72f,.2f) : new Color(1f,.26f,.18f);
}
