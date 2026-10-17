#if UNITY_EDITOR
using UnityEngine;

// Somente a cena de desenvolvimento usa este componente; não é um Retry Boss.
[DefaultExecutionOrder(100)]
public sealed class BossDevelopmentStart : MonoBehaviour
{
    [SerializeField] private WaveController waves;
    private void Start() => waves.StartBossForDevelopment();
}
#endif
