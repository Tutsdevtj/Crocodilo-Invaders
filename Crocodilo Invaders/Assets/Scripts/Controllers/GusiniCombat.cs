using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Boss), typeof(GusiniPresentation))]
public sealed class GusiniCombat : MonoBehaviour, IBossCombatController
{
    public enum Attack { Burst, Squadron, EggParachute }
    [SerializeField] private Transform shotOrigin;
    [SerializeField] private Transform squadronOrigin;
    [SerializeField] private Transform eggDropOrigin;
    [SerializeField] private Projectile bulletPrefab;
    [SerializeField] private GusiniSquadron squadronPrefab;
    [SerializeField] private GusiniParachuteEgg eggPrefab;
    [SerializeField, Min(.1f)] private float bulletSpeed = 7.5f;
    [SerializeField, Min(0f)] private float initialDelay = .35f;
    [SerializeField, Min(0f)] private float recoveryDuration = .35f;
    [SerializeField, Min(0f)] private float burstShotInterval = .025f;
    [SerializeField, Min(1)] private int burstShots = 5;
    [SerializeField, Min(1)] private int squadronGroups = 3;
    [SerializeField, Min(.01f)] private float squadronGroupInterval = .22f;
    [SerializeField] private Vector2 squadronHeightRange = new Vector2(-1.6f, 4.3f);
    [SerializeField, Min(1)] private int eggsPerDrop = 2;
    [SerializeField] private Rect eggBurstArea = new Rect(-3f, -1.4f, 4f, 4.6f);
    [SerializeField, Min(0f)] private float eggTargetSeparation = 1.6f;

    private Boss boss;
    private GusiniPresentation presentation;
    private GusiniAudio audio;
    private Health target;
    private Collider2D ownerCollider;
    private Transform threats;
    private Coroutine routine;
    private readonly Attack[] bag = { Attack.Burst, Attack.Squadron, Attack.EggParachute };
    private int nextIndex = 3;
    private Attack? lastAttack;
    private bool releasePending;
    private bool animationPending;
    public Attack CurrentAttack { get; private set; }
    public bool HasPendingAnimation => animationPending;
    public Transform ThreatRoot => threats;
    public event Action<Attack> AttackStarted;
    public event Action<Attack, GameObject> Released;

    private void Awake()
    {
        boss = GetComponent<Boss>();
        presentation = GetComponent<GusiniPresentation>();
        audio = GetComponent<GusiniAudio>();
        ownerCollider = GetComponent<Collider2D>();
    }

    public void SetTarget(Health targetHealth)
    {
        if (target != null) target.Died -= StopCombat;
        target = targetHealth;
        if (target == null || target.IsDead || !isActiveAndEnabled) return;
        target.Died += StopCombat;
        if (audio != null) audio.BeginBattle();
        if (routine == null) routine = StartCoroutine(Fight());
    }

    private bool CanAttack => isActiveAndEnabled && boss.Ready && target != null && !target.IsDead;

    private IEnumerator Fight()
    {
        while (!boss.Ready) yield return null;
        yield return new WaitForSeconds(initialDelay);
        bool first = true;
        while (CanAttack)
        {
            CurrentAttack = first ? Attack.Burst : NextAttack();
            first = false;
            lastAttack = CurrentAttack;
            AttackStarted?.Invoke(CurrentAttack);
            int clips = CurrentAttack == Attack.Burst ? Mathf.Max(1, burstShots) : 1;
            for (int i = 0; i < clips && CanAttack; i++)
            {
                // O clip conclui pelo evento; o release nunca depende de uma espera aproximada.
                releasePending = animationPending = true;
                presentation.Play(CurrentAttack);
                while (animationPending && CanAttack) yield return null;
                // Aguarda a transição real para Idle antes de armar o próximo trigger.
                while (!presentation.IsIdle && CanAttack) yield return null;
                if (i + 1 < clips) yield return new WaitForSeconds(burstShotInterval);
            }
            yield return new WaitForSeconds(recoveryDuration);
        }
        routine = null;
    }

    private Attack NextAttack()
    {
        if (nextIndex == bag.Length)
        {
            for (int i = bag.Length - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (bag[i], bag[j]) = (bag[j], bag[i]);
            }
            if (bag[0] == lastAttack) (bag[0], bag[1]) = (bag[1], bag[0]);
            nextIndex = 0;
        }
        return bag[nextIndex++];
    }

    public bool IsPending(Attack attack) => CanAttack && animationPending && CurrentAttack == attack;

    public bool TryRelease(Attack attack)
    {
        if (!releasePending || !IsPending(attack)) return false;
        releasePending = false;
        EnsureThreatRoot();
        switch (attack)
        {
            case Attack.Burst:
                Projectile bullet = Instantiate(bulletPrefab, shotOrigin.position, Quaternion.identity, threats);
                bullet.Launch(Vector2.left, ownerCollider, bulletSpeed);
                Released?.Invoke(attack, bullet.gameObject);
                break;
            case Attack.Squadron:
                StartCoroutine(ReleaseSquadrons());
                break;
            default:
                Vector2 previous = Vector2.zero;
                for (int i = 0; i < Mathf.Max(1, eggsPerDrop); i++)
                {
                    Vector2 destination = PickEggDestination(previous, i > 0);
                    previous = destination;
                    GusiniParachuteEgg egg = Instantiate(eggPrefab, eggDropOrigin.position, Quaternion.identity, threats);
                    egg.Begin(threats, ownerCollider, destination);
                    egg.SetAudio(audio);
                    Released?.Invoke(attack, egg.gameObject);
                }
                break;
        }
        return true;
    }

    private IEnumerator ReleaseSquadrons()
    {
        int groups = Mathf.Max(1, squadronGroups);
        float minimum = Mathf.Min(squadronHeightRange.x, squadronHeightRange.y);
        float maximum = Mathf.Max(squadronHeightRange.x, squadronHeightRange.y);
        if (audio != null) audio.CueSquadron(squadronOrigin.position);
        for (int i = 0; i < groups && CanAttack; i++)
        {
            Vector3 position = squadronOrigin.position;
            // Faixas diferentes deixam a chamada cheia sem empilhar os quinze pássaros.
            float lane = (maximum - minimum) / groups;
            position.y = UnityEngine.Random.Range(minimum + i * lane, minimum + (i + 1) * lane);
            var squadron = Instantiate(squadronPrefab, position, Quaternion.identity, threats);
            Released?.Invoke(Attack.Squadron, squadron.gameObject);
            if (i + 1 < groups) yield return new WaitForSeconds(squadronGroupInterval);
        }
    }

    private Vector2 PickEggDestination(Vector2 previous, bool separate)
    {
        Vector2 point = Vector2.zero;
        for (int attempt = 0; attempt < 12; attempt++)
        {
            point = new Vector2(UnityEngine.Random.Range(eggBurstArea.xMin, eggBurstArea.xMax),
                UnityEngine.Random.Range(eggBurstArea.yMin, eggBurstArea.yMax));
            if (!separate || Vector2.Distance(point, previous) >= eggTargetSeparation) return point;
        }
        // Configurações muito pequenas ainda terminam o sorteio e usam o canto mais distante.
        Vector2 minimum = eggBurstArea.min, maximum = eggBurstArea.max;
        return new Vector2(previous.x < eggBurstArea.center.x ? maximum.x : minimum.x,
            previous.y < eggBurstArea.center.y ? maximum.y : minimum.y);
    }

    public void FinishAnimation(Attack attack)
    {
        if (!IsPending(attack)) return;
        releasePending = animationPending = false;
    }

    private void EnsureThreatRoot()
    {
        if (threats != null) return;
        var root = new GameObject("Gusini threats");
        SceneManager.MoveGameObjectToScene(root, gameObject.scene);
        threats = root.transform;
    }

    public void StopCombat()
    {
        enabled = false;
        ClearCombat();
    }

    private void OnDisable() => ClearCombat();
    private void ClearCombat()
    {
        StopAllCoroutines();
        routine = null;
        animationPending = releasePending = false;
        if (target != null) target.Died -= StopCombat;
        target = null;
        if (presentation != null) presentation.Cancel();
        // Todos os descendentes, inclusive gemas que sobrevivem ao ovo, pertencem a esta raiz.
        if (threats == null) return;
        threats.gameObject.SetActive(false);
        Destroy(threats.gameObject);
        threats = null;
    }
}
