using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Hangar / wardrobe MVP: browse ship skins, preview the idle animation, equip unlocked ones.
// Built at runtime (like the developer menu), so the menu prefab only gains a button.
public sealed class WardrobeView : MonoBehaviour
{
    static readonly Color Shade = new Color(.02f, .045f, .06f, .9f);
    static readonly Color PanelColor = new Color(.043f, .094f, .125f, .99f);
    static readonly Color Card = new Color(.075f, .14f, .18f, 1f);
    static readonly Color Gold = new Color(.86f, .71f, .38f);
    static readonly Color Cream = new Color(.98f, .94f, .80f);
    static readonly Color Muted = new Color(.58f, .68f, .69f);
    static readonly Color Silhouette = new Color(.02f, .05f, .07f, .92f);

    GameObject root;
    TMP_FontAsset font;
    Image preview, previewCard;
    TMP_Text skinName, description, status, counter, saveStatus, lockMark;
    Button equipButton, closeButton, previousButton, nextButton;
    TMP_Text equipLabel;
    Action closed;
    SkinCatalog catalog;
    SkinSlot slot = SkinSlot.Ship;
    int index, frame, openedFrame = -1;
    float frameClock;

    public bool IsVisible => root != null && root.activeSelf;
    public SkinDefinition Selected => catalog != null ? catalog.At(slot, index) : null;
    public int Index => index;
    public Sprite PreviewSprite => preview != null ? preview.sprite : null;
    public bool EquipInteractable => equipButton != null && equipButton.interactable;

    public static WardrobeView Create(TMP_FontAsset font)
    {
        var view = new GameObject("Hangar").AddComponent<WardrobeView>();
        view.font = font != null ? font : TMP_Settings.defaultFontAsset;
        view.Build();
        return view;
    }

    void Build()
    {
        root = new GameObject("HangarCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(transform, false);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 2500;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1200f, 720f); scaler.matchWidthOrHeight = .5f;

        var shade = Rect("Shade", root.transform, Vector2.zero, new Vector2(.5f, .5f), Vector2.zero);
        shade.anchorMin = Vector2.zero; shade.anchorMax = Vector2.one; shade.offsetMin = shade.offsetMax = Vector2.zero;
        shade.gameObject.AddComponent<Image>().color = Shade;

        var panel = Rect("Panel", shade, new Vector2(920f, 600f), new Vector2(.5f, .5f), Vector2.zero);
        panel.gameObject.AddComponent<Image>().color = PanelColor;
        var border = panel.gameObject.AddComponent<Outline>(); border.effectColor = Gold; border.effectDistance = new Vector2(2f, -2f);

        Label("Caption", panel, "GUARDA-ROUPA DO PILOTO", 12f, Muted, new Vector2(36f, -28f), new Vector2(500f, 20f));
        Label("Title", panel, "HANGAR", 30f, Cream, new Vector2(36f, -52f), new Vector2(500f, 40f));
        closeButton = MakeButton("Close", panel, "VOLTAR", Close, new Vector2(-36f, -30f), new Vector2(160f, 44f), new Vector2(1f, 1f));

        // Slot tabs: only the airplane is live; the pilot tab shows where this is going.
        var shipTab = MakeButton("ShipTab", panel, "NAVE", () => { }, new Vector2(36f, -112f), new Vector2(170f, 36f), new Vector2(0f, 1f));
        shipTab.GetComponent<Image>().color = new Color(.16f, .22f, .16f);
        var pilotTab = MakeButton("PilotTab", panel, "PILOTO · EM BREVE", () => { }, new Vector2(222f, -112f), new Vector2(230f, 36f), new Vector2(0f, 1f));
        pilotTab.interactable = false;

        previewCard = Rect("PreviewCard", panel, new Vector2(460f, 340f), new Vector2(0f, 1f), new Vector2(36f, -168f)).gameObject.AddComponent<Image>();
        previewCard.color = Card;
        preview = Rect("Preview", previewCard.transform, new Vector2(400f, 300f), new Vector2(.5f, .5f), Vector2.zero).gameObject.AddComponent<Image>();
        preview.preserveAspect = true; preview.raycastTarget = false;
        lockMark = Label("Lock", previewCard.transform, "?", 64f, Gold, Vector2.zero, new Vector2(120f, 100f), new Vector2(.5f, .5f));
        lockMark.alignment = TextAlignmentOptions.Center;
        previousButton = MakeButton("Previous", previewCard.transform, "<", () => Step(-1), new Vector2(12f, 0f), new Vector2(48f, 64f), new Vector2(0f, .5f));
        nextButton = MakeButton("Next", previewCard.transform, ">", () => Step(1), new Vector2(-12f, 0f), new Vector2(48f, 64f), new Vector2(1f, .5f));
        counter = Label("Counter", previewCard.transform, "", 13f, Muted, new Vector2(0f, 14f), new Vector2(200f, 22f), new Vector2(.5f, 0f));
        counter.alignment = TextAlignmentOptions.Center;

        skinName = Label("Name", panel, "", 24f, Cream, new Vector2(530f, -172f), new Vector2(354f, 64f));
        skinName.textWrappingMode = TextWrappingModes.Normal;
        description = Label("Description", panel, "", 15f, Muted, new Vector2(530f, -244f), new Vector2(354f, 110f));
        description.textWrappingMode = TextWrappingModes.Normal; description.alignment = TextAlignmentOptions.TopLeft;
        status = Label("Status", panel, "", 14f, Gold, new Vector2(530f, -362f), new Vector2(354f, 44f));
        status.textWrappingMode = TextWrappingModes.Normal;
        equipButton = MakeButton("Equip", panel, "EQUIPAR", () => EquipSelected(), new Vector2(530f, -442f), new Vector2(354f, 56f), new Vector2(0f, 1f));
        equipLabel = equipButton.GetComponentInChildren<TMP_Text>();

        var hint = Label("Hint", panel, "<  >  TROCAR   ·   ENTER  EQUIPAR   ·   ESC  VOLTAR", 12f, Muted, new Vector2(0f, 46f), new Vector2(860f, 20f), new Vector2(.5f, 0f));
        hint.alignment = TextAlignmentOptions.Center;
        saveStatus = Label("SaveStatus", panel, "", 11f, Muted, new Vector2(0f, 22f), new Vector2(860f, 18f), new Vector2(.5f, 0f));
        saveStatus.alignment = TextAlignmentOptions.Center;
        root.SetActive(false);
    }

    public void Open(Action onClosed)
    {
        if (IsVisible) return;
        catalog = SkinCatalog.Load();
        if (catalog == null || catalog.Count(slot) == 0) { onClosed?.Invoke(); return; }
        closed = onClosed; root.SetActive(true); openedFrame = Time.frameCount;
        var equipped = Wardrobe.Equipped(slot);
        index = 0;
        for (int i = 0; i < catalog.Count(slot); i++) if (catalog.At(slot, i) == equipped) index = i;
        Refresh();
        // Keys are read directly, so the EventSystem must not also submit the focused button.
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    public void Close()
    {
        if (!IsVisible) return;
        root.SetActive(false);
        var callback = closed; closed = null; callback?.Invoke();
    }

    public void Step(int delta)
    {
        int count = catalog != null ? catalog.Count(slot) : 0;
        if (count == 0) return;
        index = (index + delta % count + count) % count;
        Refresh();
    }

    public bool EquipSelected()
    {
        var skin = Selected;
        if (!IsVisible || skin == null || Wardrobe.Equipped(slot) == skin || !Wardrobe.Equip(skin)) { Refresh(); return false; }
        Refresh();
        return true;
    }

    void Refresh()
    {
        var skin = Selected;
        if (skin == null) return;
        bool unlocked = Wardrobe.IsUnlocked(skin), equipped = Wardrobe.Equipped(slot) == skin;
        skinName.text = unlocked || !skin.comingSoon ? skin.displayName : "???";
        description.text = skin.description;
        if (equipped) status.text = "EQUIPADA";
        else if (skin.comingSoon) status.text = "EM BREVE";
        else if (!unlocked)
        {
            var achievement = AchievementCatalog.Get(skin.unlockAchievement);
            status.text = "BLOQUEADA · conquista \"" + (achievement != null && !achievement.Hidden ? achievement.Name : "secreta") + "\"";
        }
        else status.text = "DISPONÍVEL";
        status.color = equipped ? Gold : unlocked ? Cream : Muted;
        equipButton.interactable = unlocked && !equipped;
        equipLabel.text = equipped ? "EQUIPADA" : unlocked ? "EQUIPAR  [ENTER]" : "BLOQUEADA";
        counter.text = (index + 1) + " / " + catalog.Count(slot);
        preview.color = unlocked ? Color.white : Silhouette;
        lockMark.gameObject.SetActive(!unlocked);
        saveStatus.text = Wardrobe.Store.Error ?? "ESCOLHA SALVA NESTE COMPUTADOR";
        frame = 0; frameClock = 0f;
        preview.sprite = Frames(skin).Length > 0 ? Frames(skin)[0] : null;
        preview.enabled = preview.sprite != null;
    }

    // Locked teasers without art borrow the default airplane silhouette.
    SkinDefinition PreviewSource(SkinDefinition skin) =>
        skin.previewFrames != null && skin.previewFrames.Length > 0 ? skin : catalog.Default(slot) ?? skin;
    Sprite[] Frames(SkinDefinition skin) => PreviewSource(skin).previewFrames ?? new Sprite[0];

    void Update()
    {
        if (!IsVisible) return;
        var skin = Selected;
        if (skin != null)
        {
            var source = PreviewSource(skin); var frames = Frames(skin);
            if (frames.Length > 1)
            {
                frameClock += Mathf.Min(Time.unscaledDeltaTime, .1f);
                float duration = source.previewDurations != null && frame < source.previewDurations.Length ? Mathf.Max(.02f, source.previewDurations[frame]) : .1f;
                if (frameClock >= duration) { frameClock -= duration; frame = (frame + 1) % frames.Length; preview.sprite = frames[frame]; }
            }
        }
        if (Time.frameCount == openedFrame) return;
        var keyboard = Keyboard.current; var pad = Gamepad.current;
        if ((keyboard != null && (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)) ||
            (pad != null && (pad.dpad.left.wasPressedThisFrame || pad.leftShoulder.wasPressedThisFrame))) Step(-1);
        if ((keyboard != null && (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)) ||
            (pad != null && (pad.dpad.right.wasPressedThisFrame || pad.rightShoulder.wasPressedThisFrame))) Step(1);
        if ((keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)) ||
            (pad != null && pad.buttonSouth.wasPressedThisFrame)) EquipSelected();
    }

    static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 anchor, Vector2 position)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
        rect.sizeDelta = size; rect.anchoredPosition = position;
        return rect;
    }

    TMP_Text Label(string name, Transform parent, string value, float size, Color color, Vector2 position, Vector2 box, Vector2? anchor = null)
    {
        var text = Rect(name, parent, box, anchor ?? new Vector2(0f, 1f), position).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font; text.text = value; text.fontSize = size; text.color = color;
        text.textWrappingMode = TextWrappingModes.NoWrap; text.raycastTarget = false;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        return text;
    }

    Button MakeButton(string name, Transform parent, string value, Action action, Vector2 position, Vector2 size, Vector2 anchor)
    {
        var rect = Rect(name, parent, size, anchor, position);
        var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(.035f, .07f, .09f);
        var outline = rect.gameObject.AddComponent<Outline>(); outline.effectColor = Gold; outline.effectDistance = new Vector2(1f, -1f);
        var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        var colors = button.colors;
        colors.highlightedColor = new Color(1f, .9f, .62f); colors.pressedColor = new Color(.75f, .65f, .42f);
        colors.disabledColor = new Color(.55f, .55f, .55f, .55f);
        button.colors = colors;
        var navigation = button.navigation; navigation.mode = Navigation.Mode.None; button.navigation = navigation;
        button.onClick.AddListener(() => action());
        var label = Label("Label", rect, value, 15f, Cream, Vector2.zero, size, new Vector2(.5f, .5f));
        label.alignment = TextAlignmentOptions.Center;
        return button;
    }
}
