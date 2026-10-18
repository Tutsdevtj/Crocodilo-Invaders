using UnityEngine;

// Único lugar do destino pós-capítulo e dos marcos narrativos que ele registra.
[CreateAssetMenu(menuName = "Crocodilo/Chapter Definition")]
public sealed class ChapterDefinition : ScriptableObject
{
    [SerializeField] private string chapterId = "chapter1";
    [SerializeField, Tooltip("Nome ou caminho da cena carregada ao concluir. Precisa estar em Build Settings.")]
    private string nextScene = "BaseImprovisada";
    [SerializeField, Tooltip("Registrados uma única vez em CampaignProgress ao concluir.")]
    private string[] completionFlags = { "gusini_rescued", "chapter1_completed" };
    [SerializeField, TextArea(1, 3), Tooltip("Somente desenvolvimento: aparece enquanto a cena de destino não existe.")]
    private string unavailableMessage = "FIM DO CAPÍTULO 1 — BASE IMPROVISADA EM DESENVOLVIMENTO";

    public string ChapterId => chapterId;
    public string NextScene => nextScene;
    public string[] CompletionFlags => completionFlags;
    public string UnavailableMessage => unavailableMessage;
}
