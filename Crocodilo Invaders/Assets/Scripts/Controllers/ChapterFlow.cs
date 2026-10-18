using UnityEngine;

// Conclusão do capítulo: registra os marcos narrativos uma vez e pede a cena seguinte do ChapterDefinition.
public sealed class ChapterFlow : MonoBehaviour
{
    [SerializeField] private WaveController waves;
    [SerializeField] private ChapterDefinition chapter;
    [SerializeField] private SceneTransition transition;
    public bool Completed { get; private set; }
    public ChapterDefinition Chapter => chapter;
    public SceneTransition Transition => transition;

    private void OnEnable() { if (waves != null) waves.ChapterCompleted += OnChapterCompleted; }
    private void OnDisable() { if (waves != null) waves.ChapterCompleted -= OnChapterCompleted; }

    private void OnChapterCompleted()
    {
        if (Completed || chapter == null || transition == null) return;
        Completed = true;
        foreach (string flag in chapter.CompletionFlags) CampaignProgress.Record(flag);
        transition.Go(chapter.NextScene, chapter.UnavailableMessage);
    }
}
