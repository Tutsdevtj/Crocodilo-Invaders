using TMPro;
using UnityEngine;

public sealed class PlayerHUD : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private RunScore score;
    [SerializeField] private PlayerHUDCockpit cockpit;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private TMP_Text killsText;

    private Health health;
    private PlayerCombat combat;

    // Sincroniza também depois dos Awakes dos componentes de gameplay.
    private void Start() => ResetDisplay();

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

        ResetDisplay();
    }

    private void ResetDisplay()
    {
        if (cockpit != null)
            cockpit.ResetDisplay(health != null ? health.Fraction : 0f,
                combat != null ? (float)combat.Charge / Mathf.Max(1, combat.SpecialChargeRequired) : 0f);
    }

    private void RefreshHealth(int current, int maximum)
    {
        if (cockpit != null) cockpit.SetHealth(health != null ? health.Fraction : (float)current / Mathf.Max(1, maximum));
    }

    private void RefreshSpecial(int current, int maximum)
    {
        if (cockpit != null) cockpit.SetCharge((float)current / Mathf.Max(1, maximum));
    }

    // Apenas apresenta valores fornecidos pelo gameplay; o HUD não conta pontos ou kills.
    public void SetScore(int points, int kills)
    {
        if (pointsText != null) pointsText.text = $"Pontos: {points}";
        if (killsText != null) killsText.text = $"Inimigos: {kills}";
    }

    private void Unsubscribe()
    {
        if (health != null) health.HealthChanged -= RefreshHealth;
        if (combat != null) combat.ChargeChanged -= RefreshSpecial;
    }
}
