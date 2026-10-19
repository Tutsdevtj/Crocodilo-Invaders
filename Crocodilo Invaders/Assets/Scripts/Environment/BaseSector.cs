using UnityEngine;

// Estado visual/físico de um setor da base. Não decide quando abrir: a progressão futura chama
// SetState. Fora de Open, barricadas e bloqueios ficam ativos; em Open, o que só existe aberto.
// Scenic = fechado por ambientação (não é uma etapa de progresso); Locked = espera progresso.
public sealed class BaseSector : MonoBehaviour
{
    public enum State { Open, Scenic, Locked }

    [SerializeField] private string sectorId;
    [SerializeField] private State state = State.Open;
    [SerializeField, Tooltip("Ativos enquanto o setor não está Open (barricadas, colisores de bloqueio).")]
    private GameObject[] closedOnly = new GameObject[0];
    [SerializeField, Tooltip("Ativos somente com o setor Open (acessos, futuros pontos de interação).")]
    private GameObject[] openOnly = new GameObject[0];

    public string SectorId => sectorId;
    public State Current => state;

    private void Awake() => Apply();

    public void SetState(State next)
    {
        state = next;
        Apply();
    }

    private void Apply()
    {
        bool open = state == State.Open;
        foreach (GameObject item in closedOnly) if (item != null) item.SetActive(!open);
        foreach (GameObject item in openOnly) if (item != null) item.SetActive(open);
    }
}
