using UnityEngine;

[RequireComponent(typeof(Projectile))]
public sealed class ProjectileImpact : MonoBehaviour
{
    [SerializeField] private GameObject impactPrefab;
    private Projectile projectile;

    private void Awake() => projectile = GetComponent<Projectile>();
    private void OnEnable() => projectile.Hit += ShowImpact;
    private void OnDisable() => projectile.Hit -= ShowImpact;

    private void ShowImpact(Vector2 position)
    {
        if (impactPrefab != null)
            Instantiate(impactPrefab, position, Quaternion.identity);
    }
}
