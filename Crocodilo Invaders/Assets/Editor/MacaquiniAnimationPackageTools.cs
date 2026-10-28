using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class MacaquiniAnimationPackageTools
{
    private static AddRequest request;
    private static double deadline;
    public static void Install()
    {
        request = Client.Add("com.unity.2d.animation");
        deadline = EditorApplication.timeSinceStartup + 180;
        EditorApplication.update += Poll;
    }
    private static void Poll()
    {
        if (!request.IsCompleted && EditorApplication.timeSinceStartup < deadline) return;
        EditorApplication.update -= Poll;
        if (request.IsCompleted && request.Status == StatusCode.Success)
        {
            Debug.Log("MACAQUINI_ANIMATION_PACKAGE " + request.Result.packageId);
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError("Animation package: " + (request.Error?.message ?? "timeout"));
            EditorApplication.Exit(1);
        }
    }
}
