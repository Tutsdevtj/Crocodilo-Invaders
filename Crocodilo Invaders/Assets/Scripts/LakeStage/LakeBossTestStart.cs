using UnityEngine;

[DefaultExecutionOrder(-200)]
public sealed class LakeBossTestStart : MonoBehaviour
{
    private void Awake() { LeaderboardSession.Cancel(); DevelopmentBossSession.Begin(1); }
}
