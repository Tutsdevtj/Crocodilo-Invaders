using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Scene-local subscriptions. Counters are saved only through actual gameplay events.
public sealed class AchievementTracker : MonoBehaviour
{
    Player player;
    WaveController waves;
    Boss boss;
    PlayerRunUpgrades upgrades;
    readonly List<Projectile> specials = new List<Projectile>();
    bool waveDamaged, waveObserved, dodgeCounted, productionScene;
    public bool Eligible => productionScene && Achievements.CanRecord && player != null && !player.Health.IsDead;

    public void BindScene(Scene scene)
    {
        Unbind();
        productionScene = scene.name == "PlayerMovementTest" || scene.name == "StageOne" || scene.name == "StageTwoJungle" || scene.name == "StageTwoLake";
        if (!productionScene) return;
        player = FindAnyObjectByType<Player>(); waves = FindAnyObjectByType<WaveController>();
        if (player == null || waves == null) { Unbind(); return; }
        player.Health.Damaged += OnDamage;
        player.Health.DamagePrevented += OnPrevented;
        player.Dash.DashStarted += OnDash;
        player.Combat.SpecialProjectileCreated += OnSpecial;
        upgrades = player.GetComponent<PlayerRunUpgrades>();
        if (upgrades != null) upgrades.CardApplied += OnCard;
        waves.WaveAnnounced += OnWave;
        waves.WaveCleared += OnWaveCleared;
        waves.BossStarted += OnBoss;
        if (waves.CurrentWave > 0 && waves.Phase != WaveController.RunPhase.Waiting) waveObserved = true;
        if (waves.ActiveBoss != null) OnBoss(waves.ActiveBoss);
    }
    void OnWave(int wave, int count) { waveDamaged = false; waveObserved = Eligible; }
    void OnDamage() { waveDamaged = true; }
    void OnWaveCleared(int wave, int bonus)
    {
        if (!Eligible || !waveObserved) return;
        if (wave == 1) Achievements.Award(AchievementCatalog.FirstWave);
        if (!waveDamaged) Achievements.Award(AchievementCatalog.CleanWave);
        waveObserved = false;
    }
    void OnDash(Vector2 direction) { dodgeCounted = false; }
    void OnPrevented()
    {
        if (!Eligible || dodgeCounted || !player.Dash.HasInvulnerability || !player.Dash.IsDashing) return;
        // A sustained attack or several projectiles in the same dash count once.
        dodgeCounted = true; Achievements.Add(AchievementCatalog.Dodge, 1);
    }
    void OnCard(UpgradeCard card) { if (Eligible) Achievements.Card(card); }
    void OnSpecial(Projectile projectile)
    {
        if (!Eligible) return;
        specials.RemoveAll(p => p == null);
        specials.Add(projectile); projectile.DamagedTarget += OnSpecialHit;
    }
    void OnSpecialHit(Health target)
    {
        if (Eligible && target != null && target.GetComponentInParent<Boss>() != null)
            Achievements.Award(AchievementCatalog.Special);
    }
    void OnBoss(Boss next)
    {
        if (boss != null) boss.Defeated -= OnDefeated;
        boss = next;
        if (boss != null) boss.Defeated += OnDefeated;
    }
    void OnDefeated(Boss defeated)
    {
        if (!Eligible) return;
        if (defeated.GetComponent<GusiniPhaseTwo>() != null) Achievements.Award(AchievementCatalog.Gusini);
        else if (defeated.GetComponent<MacaquiniPhaseTwo>() != null) Achievements.Award(AchievementCatalog.Macaquini);
        else if (defeated.GetComponent<SapoDoPneuCombat>() != null) Achievements.Award(AchievementCatalog.Sapeneu);
        if (player.Health.Fraction <= .100001f) Achievements.Award(AchievementCatalog.LastTooth);
    }
    void Unbind()
    {
        if (player != null)
        {
            player.Health.Damaged -= OnDamage; player.Health.DamagePrevented -= OnPrevented;
            player.Dash.DashStarted -= OnDash; player.Combat.SpecialProjectileCreated -= OnSpecial;
        }
        if (upgrades != null) upgrades.CardApplied -= OnCard;
        if (waves != null) { waves.WaveAnnounced -= OnWave; waves.WaveCleared -= OnWaveCleared; waves.BossStarted -= OnBoss; }
        if (boss != null) boss.Defeated -= OnDefeated;
        foreach (var projectile in specials) if (projectile != null) projectile.DamagedTarget -= OnSpecialHit;
        specials.Clear(); player = null; waves = null; boss = null; upgrades = null;
        waveDamaged = waveObserved = dodgeCounted = false;
    }
    void OnDestroy() => Unbind();
}
