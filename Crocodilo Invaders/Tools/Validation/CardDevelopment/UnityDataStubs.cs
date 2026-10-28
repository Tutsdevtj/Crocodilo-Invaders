// Pure data test host. This does not emulate Unity UI, rendering or Play Mode.
namespace UnityEngine
{
    public class ScriptableObject { }
    public class Sprite { }
    public class MonoBehaviour
    {
        public bool enabled = true;
        public T GetComponent<T>() where T : class => null;
        protected void StartCoroutine(System.Collections.IEnumerator routine) { }
        protected void StopAllCoroutines() { }
    }
    public static class Time { public static float timeScale = 1f; }
    public static class Debug { public static void LogError(string message, object context) { } }
    public class SerializeField : System.Attribute { }
    public class MinAttribute : System.Attribute { public MinAttribute(float value) { } }
    public class CreateAssetMenuAttribute : System.Attribute { public string menuName; }
    public enum RuntimeInitializeLoadType { SubsystemRegistration }
    public class RuntimeInitializeOnLoadMethodAttribute : System.Attribute
    { public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType type) { } }
    public static class Mathf
    {
        public static float Clamp01(float value) => System.Math.Clamp(value, 0f, 1f);
        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);
        public static int Min(int a, int b) => System.Math.Min(a, b);
    }
    public static class Resources
    { public static T[] LoadAll<T>(string path) => throw new System.NotSupportedException("Rendering is validated in Unity."); }
}
