using UnityEngine;

public sealed class OneShotEffect : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float lifetime = 0.34f;
    private void Start() => Destroy(gameObject, lifetime);
}
