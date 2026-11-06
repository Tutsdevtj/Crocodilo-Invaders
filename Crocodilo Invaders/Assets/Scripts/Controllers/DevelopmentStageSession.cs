using UnityEngine;

// Full-stage preview from DEV; keeps normal wave flow without permanent rewards.
public static class DevelopmentStageSession
{
    public static bool IsActive { get; private set; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void End() => IsActive=false;
    public static void Begin() => IsActive=true;
}
