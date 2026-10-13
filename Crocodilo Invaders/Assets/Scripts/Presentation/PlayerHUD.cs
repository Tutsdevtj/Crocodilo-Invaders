using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public sealed class PlayerHUD : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private RunScore score;
    [FormerlySerializedAs("healthFill"), SerializeField] private Image healthBar;
    [FormerlySerializedAs("specialFill"), SerializeField] private Image specialBar;
    [Tooltip("Sprites da vida cheia até vazia, na ordem do HUD original.")]
    [SerializeField] private Sprite[] healthSprites;
    [Tooltip("Sprites do especial cheio até vazio, na ordem do HUD original.")]
    [SerializeField] private Sprite[] specialSprites;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private TMP_Text killsText;

    private Health health;
    private PlayerCombat combat;

    private void OnEnable()
    {
        Bind(player);
        if (score != null)
        {
            score.Changed += SetScore;
            SetScore(score.Points, score.Kills);
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
        if (score != null) score.Changed -= SetScore;
    }

    public void Bind(Player target)
    {
        Unsubscribe();
        player = target;
        // Componentes podem existir antes de o Awake da fachada Player executar.
        health = player != null ? player.GetComponent<Health>() : null;
        combat = player != null ? player.GetComponent<PlayerCombat>() : null;

        if (isActiveAndEnabled)
        {
            if (health != null) health.HealthChanged += RefreshHealth;
            if (combat != null) combat.ChargeChanged += RefreshSpecial;
        }

        RefreshHealth(health != null ? health.CurrentHealth : 0, health != null ? health.MaxHealth : 1);
    }

    private void RefreshHealth(int current, int maximum)
    {
        if (healthSprites != null && healthSprites.Length == 9)
            healthBar.sprite = healthSprites[HealthStage(health != null ? health.Fraction : (float)current / maximum)];
        RefreshSpecial(combat != null ? combat.Charge : 0, combat != null ? combat.SpecialChargeRequired : 1);
    }

    private void RefreshSpecial(int current, int maximum)
    {
        float charge = (float)current / maximum;
        int index = charge >= 1f ? 0 : charge >= .6f ? 1 : charge >= .4f ? 2 : charge >= .2f ? 3 : 4;
        if (specialSprites != null && specialSprites.Length == 5)
            specialBar.sprite = specialSprites[index];
    }

    // Apenas apresenta valores fornecidos pelo gameplay; o HUD não conta pontos ou kills.
    public void SetScore(int points, int kills)
    {
        if (pointsText != null) pointsText.text = $"Pontos: {points}";
        if (killsText != null) killsText.text = $"Inimigos: {kills}";
    }

    private static int HealthStage(float remaining)
    {
        if (remaining <= 0f) return 8;
        if (remaining <= 1f / 6f) return 7;
        if (remaining <= .4f) return 6;
        if (remaining <= .5f) return 5;
        if (remaining <= .6f) return 4;
        if (remaining <= .7f) return 3;
        if (remaining <= .8f) return 2;
        if (remaining <= .9f) return 1;
        return 0;
    }

    private void Unsubscribe()
    {
        if (health != null) health.HealthChanged -= RefreshHealth;
        if (combat != null) combat.ChargeChanged -= RefreshSpecial;
    }
}
