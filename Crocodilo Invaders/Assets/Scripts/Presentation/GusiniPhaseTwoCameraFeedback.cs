using UnityEngine;

// Offset aditivo e curto; nunca escreve uma nova posição-base da câmera.
public sealed class GusiniPhaseTwoCameraFeedback : MonoBehaviour
{
    private float age, duration, amplitude;
    private Vector3 offset;
    public static void Pulse(Camera camera, float amplitude, float duration)
    {
        if (camera == null) return;
        var shake = camera.GetComponent<GusiniPhaseTwoCameraFeedback>();
        if (shake == null) shake = camera.gameObject.AddComponent<GusiniPhaseTwoCameraFeedback>();
        shake.age = 0f; shake.amplitude = amplitude; shake.duration = duration;
    }
    private void LateUpdate()
    {
        transform.position -= offset;
        age += Time.deltaTime;
        float strength = amplitude * Mathf.Clamp01(1f - age / duration);
        offset = new Vector3(Mathf.Sin(age * 157f), Mathf.Cos(age * 193f), 0) * strength;
        transform.position += offset;
    }
    private void OnDisable() { transform.position -= offset; offset = Vector3.zero; }
}
