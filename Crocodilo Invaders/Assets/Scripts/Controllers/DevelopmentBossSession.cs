using System.Collections;
using UnityEngine;

// Uses campaign arenas and real phase transitions, with no ranked run prepared.
public sealed class DevelopmentBossSession : MonoBehaviour
{
    public static bool IsActive { get; private set; }
    public static int Phase { get; private set; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void End() { IsActive = false; Phase = 1; }
    public static void Begin(int phase) { IsActive = true; Phase = Mathf.Clamp(phase, 1, 2); }

    public void Initialize(WaveController waves)
    {
        waves.StartBossForDevelopment();
        if (Phase == 2) StartCoroutine(EnterSecondPhase(waves));
    }

    private IEnumerator EnterSecondPhase(WaveController waves)
    {
        while (waves != null && (waves.ActiveBoss == null || !waves.ActiveBoss.Ready)) yield return null;
        if (waves == null) yield break;
        var boss = waves.ActiveBoss;
        boss.Health.TakeDamage(boss.Health.CurrentHealth);
    }
}
