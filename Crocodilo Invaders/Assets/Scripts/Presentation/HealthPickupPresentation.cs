using UnityEngine;

[RequireComponent(typeof(HealthPickup))]
public sealed class HealthPickupPresentation : MonoBehaviour
{
    [SerializeField] private GameObject healEffect;
    [SerializeField] private Vector2 effectOffset = new Vector2(.1f, .55f);
    private HealthPickup pickup;
    private void Awake() => pickup = GetComponent<HealthPickup>();
    private void OnEnable() => pickup.Collected += ShowHeal;
    private void OnDisable() => pickup.Collected -= ShowHeal;

    private void ShowHeal(Player player)
    {
        if (healEffect == null) return;
        GameObject effect = Instantiate(healEffect, player.transform);
        effect.transform.localPosition = effectOffset;
    }
}
