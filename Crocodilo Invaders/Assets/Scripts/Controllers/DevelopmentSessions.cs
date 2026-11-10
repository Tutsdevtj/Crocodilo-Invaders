// Single reset point, so a new DEV mode cannot leak into another one.
public static class DevelopmentSessions
{
    public static void EndAll()
    {
        DevelopmentCardSession.End();
        DevelopmentBossSession.End();
        DevelopmentStageSession.End();
    }
}
