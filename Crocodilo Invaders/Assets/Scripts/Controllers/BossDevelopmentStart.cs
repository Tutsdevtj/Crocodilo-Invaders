#if UNITY_EDITOR
using UnityEngine;

// Somente a cena de desenvolvimento usa este componente; não é um Retry Boss.
[DefaultExecutionOrder(100)]
public sealed class BossDevelopmentStart : MonoBehaviour
{
    [SerializeField] private WaveController waves;
    [Header("B. Crocodilo — somente teste do Gusini")]
    [SerializeField, Min(30)] private int testHealth = 240;
    [SerializeField, Min(1f)] private float attackSpeedMultiplier = 2.5f;
    [SerializeField, Min(1f)] private float normalDamageMultiplier = 4f;
    [SerializeField, Min(1f)] private float specialDamageMultiplier = 3f;
    [SerializeField, Min(1f)] private float movementMultiplier = 1.4f;
    [SerializeField, Min(1)] private int shotsToChargeSpecial = 5;

    private void Start()
    {
        waves.StartBossForDevelopment();
        if (waves.Phase != WaveController.RunPhase.Boss
            || gameObject.scene.path != "Assets/Scenes/Tests/GusiniBossTest.unity") return;

        // StartRun zera os upgrades: o kit de teste entra depois desse reset.
        foreach (Player player in FindObjectsByType<Player>(FindObjectsSortMode.None))
        {
            if (player.gameObject.scene != gameObject.scene) continue;
            player.Health.SetMaximumHealth(testHealth, true);
            player.Health.Heal(testHealth);
            player.Combat.SetUpgradeMultipliers(attackSpeedMultiplier, normalDamageMultiplier, specialDamageMultiplier);
            player.Combat.ConfigureBossTestSpecial(shotsToChargeSpecial);
            player.Movement.SetMoveSpeed(player.Movement.MoveSpeed * movementMultiplier);
            player.Dash.SetUpgradeMultipliers(1.3f, .4f, 1.4f);
            break;
        }
    }
}
#endif
