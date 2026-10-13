using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class HealthPickup : MonoBehaviour
{
    [SerializeField, Min(1)] private int healAmount = 6;
    private bool collected;
    public int HealAmount => healAmount;
    public event Action<Player> Collected;
    public event Action<HealthPickup, bool> Removed;

    public bool TryCollect(Player player)
    {
        if (collected || !isActiveAndEnabled || Time.timeScale <= 0f || player == null
            || !player.Health.Heal(healAmount)) return false;
        collected = true;
        GetComponent<Collider2D>().enabled = false;
        Collected?.Invoke(player);
        Destroy(gameObject);
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other) => TryCollect(other.GetComponentInParent<Player>());
    // Permite coletar após tomar dano se o player já estava sobre a caixa com HP cheio.
    private void OnTriggerStay2D(Collider2D other) => TryCollect(other.GetComponentInParent<Player>());
    private void OnDestroy() => Removed?.Invoke(this, collected);
}
