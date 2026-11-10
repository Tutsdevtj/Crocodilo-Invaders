using UnityEngine;

// Every number that changes how hard the lake is. Edit the asset in the Inspector, no recompiling.
[CreateAssetMenu(fileName = "LakeDifficulty", menuName = "Crocodilo/Lago/Dificuldade")]
public sealed class LakeDifficulty : ScriptableObject
{
    [Header("Sapeneu")]
    public float firstAttackDelay = 1;
    public float restDuration = 2.4f;
    public float pauseAfterTongue = .8f;
    public float pauseAfterTire = .9f;
    public float pauseAfterJump = 1;

    [Header("Salto")]
    public float jumpWarning = .9f;
    public float jumpDuration = 1.45f;
    [Range(0, 1), Tooltip("Fração do voo em que o pouso para de seguir a nave.")]
    public float jumpLockProgress = .55f;
    public float jumpTrackSpeed = 4.5f;
    public float jumpContactDamage = 2;

    [Header("Fúria")]
    [Range(0, 1)] public float furyHealthFraction = .5f;
    [Range(0, 1)] public float furyRestScale = .72f;
    [Range(0, 1)] public float furyPauseScale = .6f;

    [Header("Arroto")]
    public float burpWindup = .6f;
    public float burpDuration = 1.4f;
    public float burpDamageMultiplier = 2;
    public int burpDrops = 7;
    public int burpDropsFury = 9;
    public float burpDropSpeedX = 7.6f;
    public float burpDropSpeedY = 8.6f;

    [Header("Gotas do salto")]
    public float dropDamage = 1;
    public int lowDrops = 3;
    public int lowDropsFury = 4;
    public float lowSpeedX = 4.7f;
    public float lowSpeedXStep = .55f;
    public float lowSpeedY = 5.1f;
    public float lowSpeedYStep = .4f;
    public float highWaveDelay = .32f;
    public int highDrops = 2;
    public int highDropsFury = 3;
    public float highSpeedY = 10;
    public float highSpeedYStep = .3f;
    public float highSpeedLeft = 2.6f;
    public float highSpeedLeftStep = .7f;
    public float highSpeedRight = 1.5f;
    public float highSpeedRightStep = .5f;
    public float aimedDropFlight = 1.05f;

    [Header("Língua")]
    public int tongueTargets = 3;
    public int tongueTargetsFury = 4;
    public float trackingTime = .46f;
    public float lockTime = .16f;
    public float extendTime = .13f;
    public float holdTime = .04f;
    public float retractTime = .19f;
    public float lineRadius = .28f;
    public float tongueDamage = 2;
    public float huntSpeed = 14;
    public float huntBeyondShip = .9f;
    public float roamSpeed = 1.4f;
    public float roamRadius = 2.4f;

    [Header("Pneu")]
    public float tirePreparation = 1.05f;
    public float tireAimLock = .3f;
    public float tireFlightHold = .7f;
    public float tireRecovery = .4f;
    public float tireSpeed = 10.5f;
    public float tireSpeedFury = 12.5f;
    public int tireBounces = 2;
    public int tireBouncesFury = 3;
    public float tireDamage = 2;

    [Header("Saperereca")]
    public float helperFirstAttack = 1.3f;
    public float helperCooldown = 1.6f;
    public float helperComboCooldown = 2;
    public float helperReadyTime = .4f;
    public float helperAimLock = .3f;
    public float helperAimLead = .45f;
    public float helperMaxAimLead = 2.5f;
    public int helperShots = 3;
    public float helperShotInterval = .16f;
    public float helperShotFlight = .95f;
    public float helperShotSpread = .65f;
    public float helperDamage = 1;
    public float helperDamageGate = .65f;

    [Header("Hordas")]
    public int earlyStrikes = 1;
    public int lateStrikes = 2;
    public float earlyStrikeGap = 1.18f;
    public float lateStrikeGap = .85f;
    public float journeyDamageGate = .7f;
    [Tooltip("Distância mínima e máxima que o mosquito mantém à frente da nave.")]
    public float mosquitoStandoffMin = .35f;
    public float mosquitoStandoffMax = 3.1f;
    public float mosquitoSpeed = 1.4f;
    public float mosquitoReturnSpeed = 3.6f;
}
