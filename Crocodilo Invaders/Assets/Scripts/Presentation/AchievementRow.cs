using System;
using TMPro;
using UnityEngine;

public sealed class AchievementRow : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Image icon, accent, progressFill;
    [SerializeField] TMP_Text title, description, progress, status;
    [SerializeField] GameObject secretMark, progressRoot;
    public string DisplayedTitle => title.text;
    public string DisplayedDescription => description.text;
    public bool HidesDetails { get; private set; }
    public void Bind(AchievementDefinition definition, AchievementStore.Entry entry, Sprite artwork)
    {
        HidesDetails = definition.Hidden && !entry.Unlocked;
        var gold = new Color(.89f, .75f, .40f);
        var muted = new Color(.48f, .57f, .62f);
        title.text = HidesDetails ? "CONQUISTA OCULTA" : definition.Name;
        description.text = HidesDetails ? "Os detalhes serão revelados ao desbloquear." : definition.Description;
        icon.sprite = artwork; icon.enabled = !HidesDetails && artwork != null;
        icon.color = entry.Unlocked ? Color.white : new Color(.46f, .49f, .48f);
        secretMark.SetActive(HidesDetails);
        accent.color = entry.Unlocked ? gold : muted;
        title.color = entry.Unlocked ? new Color(1f, .91f, .67f) : new Color(.79f, .84f, .82f);
        progressRoot.SetActive(!HidesDetails);
        progressFill.fillAmount = entry.progress / (float)definition.Goal;
        progressFill.color = entry.Unlocked ? gold : new Color(.40f, .67f, .69f);
        progress.text = HidesDetails ? "" : entry.Unlocked ? "CONCLUÍDA" : entry.progress + " / " + definition.Goal;
        status.text = HidesDetails ? "OCULTA" : entry.Unlocked ? "DESBLOQUEADA · " +
            DateTimeOffset.FromUnixTimeSeconds(entry.unlockedUtc).ToLocalTime().ToString("dd/MM/yyyy") : "BLOQUEADA";
    }
}
