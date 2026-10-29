using TMPro;
using UnityEngine;

// Reuses the existing Gusini frame, health animation and placement.
public sealed class LakeBossBar : MonoBehaviour
{
    public void SetIdentity(Sprite portrait, TMP_FontAsset font)
    {
        foreach (var image in GetComponentsInChildren<UnityEngine.UI.Image>(true))
        {
            if (image.name == "name_text")
            {
                image.enabled = false;
                var old = image.transform.parent.Find("LakeBossName");
                if (old != null) continue;
                var node = new GameObject("LakeBossName", typeof(RectTransform));
                var rect = node.GetComponent<RectTransform>(); rect.SetParent(image.transform.parent, false);
                var source = image.rectTransform;
                rect.anchorMin = source.anchorMin; rect.anchorMax = source.anchorMax; rect.pivot = source.pivot;
                rect.anchoredPosition = source.anchoredPosition; rect.sizeDelta = source.sizeDelta;
                var text = node.AddComponent<TextMeshProUGUI>(); text.text = "SAPO DO PNEU";
                text.font = font; text.fontSize = 10; text.enableAutoSizing = true;
                text.fontSizeMin = 6; text.fontSizeMax = 10; text.alignment = TextAlignmentOptions.Center;
                text.color = new Color(.91f, .95f, .75f); text.raycastTarget = false;
            }
            if (image.name == "emblem_roundel") { image.sprite = portrait; image.preserveAspect = true; }
        }
    }
}
