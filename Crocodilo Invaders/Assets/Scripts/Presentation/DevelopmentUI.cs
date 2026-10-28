using TMPro;
using UnityEngine;

// Small runtime uGUI panels: production scenes and reward prefabs stay unchanged.
public static class DevelopmentUI
{
    public static readonly Color Ink = new Color(.94f, .91f, .79f);
    public static readonly Color Muted = new Color(.66f, .72f, .64f);

    public static GameObject Canvas(string name)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.Canvas),
            typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        root.GetComponent<UnityEngine.Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        root.GetComponent<UnityEngine.Canvas>().sortingOrder = 3000;
        var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1200f, 720f);
        scaler.matchWidthOrHeight = .5f;
        return root;
    }

    public static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 anchor, Vector2 position)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return rect;
    }

    public static RectTransform Panel(string name, Transform parent, Vector2 size, Vector2 anchor, Vector2 position)
    {
        RectTransform panel = Rect(name, parent, size, anchor, position);
        panel.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.045f, .065f, .055f, .98f);
        var layout = panel.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 20, 20);
        layout.spacing = 8f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        return panel;
    }

    public static TMP_Text Text(string name, Transform parent, TMP_FontAsset font, string value, float size, float height)
    {
        RectTransform rect = Rect(name, parent, Vector2.zero, new Vector2(.5f, .5f), Vector2.zero);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font != null ? font : TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = size;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Min(size, 12f);
        text.fontSizeMax = size;
        text.color = Ink;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.raycastTarget = false;
        var element = rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
        element.minHeight = element.preferredHeight = height;
        return text;
    }

    public static UnityEngine.UI.Button Button(string name, Transform parent, TMP_FontAsset font, string value,
        UnityEngine.Events.UnityAction action, float height = 44f)
    {
        RectTransform rect = Rect(name, parent, new Vector2(280f, height), new Vector2(.5f, .5f), Vector2.zero);
        var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(.15f, .20f, .14f);
        var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.highlightedColor = new Color(1f, .88f, .58f);
        colors.pressedColor = new Color(.64f, .75f, .49f);
        button.colors = colors;
        button.onClick.AddListener(action);
        var element = rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
        element.minHeight = element.preferredHeight = height;
        TMP_Text label = Text("Label", rect, font, value, 16f, height);
        var labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(12f, 2f);
        labelRect.offsetMax = new Vector2(-12f, -2f);
        label.alignment = TextAlignmentOptions.Center;
        return button;
    }

    public static void Label(UnityEngine.UI.Button button, string text) => button.GetComponentInChildren<TMP_Text>().text = text;
    public static void Corner(UnityEngine.UI.Button button, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var rect = (RectTransform)button.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}
