using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// First-form ecology. The boss keeps owning its patterns, transformation and shutdown.
public sealed class MacaquiniPhaseOneHelpers : MonoBehaviour
{
    private MacaquiniCombat combat;
    private Boss boss;
    private Health target;
    private Material material;
    private Sprite[] bat, moth;
    private Transform root;
    private readonly List<MacaquiniJungleHelper> helpers = new List<MacaquiniJungleHelper>();
    private readonly List<MacaquiniSporeBubble> bubbles = new List<MacaquiniSporeBubble>();
    private float nextWaveAt, nextBiteAt;
    private int patterns, waves;
    private Vector2 previousBoss;
    public int LivingHelpers => helpers.FindAll(h => h != null && h.Alive).Count;
    public bool BiteCommitted => helpers.Exists(h => h != null && h.Committed);
    public bool HasCommittedAttack => BiteCommitted || bubbles.Exists(b => b != null && (b.Committed || b.Bursting));
    public int WavesSpawned { get; private set; }
    public int FriendlyHits { get; private set; }
    public bool CanContinue => isActiveAndEnabled && combat != null && !combat.PhaseTwo &&
        boss.BattleActive && !boss.Transitioning && target != null && !target.IsDead;
    public bool BossBusy => combat.PerformingAttack || !boss.Ready;
    public Health Target => target;
    public Material Material => material;
    public Transform ThreatRoot => root;

    private void Awake()
    {
        combat = GetComponent<MacaquiniCombat>(); boss = GetComponent<Boss>();
        bat = Load("bat"); moth = Load("moth");
    }
    private static Sprite[] Load(string species)
    {
        var sprites = new Sprite[8];
        for (int i = 0; i < sprites.Length; i++) sprites[i] = Resources.Load<Sprite>("MacaquiniHelpersV24/" + species + "_" + i);
        return sprites;
    }
    public void Begin(Health player, Material fx)
    {
        Cancel(); target = player; material = fx; patterns = waves = 0;
        nextWaveAt = Time.time; nextBiteAt = Time.time; previousBoss = transform.position;
    }
    public void PatternStarted(MacaquiniCombat.Attack attack)
    {
        if (!CanContinue || material == null || bat[0] == null || moth[0] == null) return;
        patterns++;
        // Teach the first attack alone. Waves are spaced by at least eight seconds.
        if (patterns < 2 || Time.time < nextWaveAt || LivingHelpers > 0) return;
        if (attack != MacaquiniCombat.Attack.JetDash && attack != MacaquiniCombat.Attack.JetStrafe && attack != MacaquiniCombat.Attack.ExhaustGates) return;
        SpawnWave();
    }
    public void SpawnWave()
    {
        if (!CanContinue || material == null || bat[0] == null || moth[0] == null || LivingHelpers > 0) return;
        if (root == null)
        {
            root = new GameObject("MacaquiniPhaseOneEcology").transform;
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root.gameObject, gameObject.scene);
        }
        helpers.RemoveAll(h => h == null); bubbles.RemoveAll(b => b == null);
        waves++; WavesSpawned++; nextWaveAt = Time.time + 8f;
        // Two bats first; later rounds add a hovering spore moth. Never instant replacement.
        int count = waves == 1 ? 2 : 3;
        var camera = Camera.main;
        float cx = camera == null ? 0 : camera.transform.position.x;
        float cy = camera == null ? 1 : camera.transform.position.y;
        float half = camera == null ? 9 : camera.orthographicSize * camera.aspect;
        for (int i = 0; i < count; i++)
        {
            bool spitter = waves > 1 && i == count - 1;
            Vector2 home = new Vector2(cx + half * (spitter ? .55f : .25f + i * .19f), cy + (i % 2 == 0 ? 2.3f : -1.3f));
            var node = new GameObject(spitter ? "MariposaEsporadora" : "MorcegoEmboscador");
            node.transform.SetParent(root, false); node.layer = LayerMask.NameToLayer("Enemy");
            var health = node.AddComponent<Health>(); health.SetMaximumHealth(spitter ? 3 : 2, false);
            var helper = node.AddComponent<MacaquiniJungleHelper>();
            helper.Configure(this, spitter, spitter ? moth : bat, new Vector2(cx + half + 1.5f, home.y + .7f), home, i * .65f);
            helpers.Add(helper);
        }
    }
    public bool TryReserveBite()
    {
        if (!CanContinue || BossBusy || Time.time < nextBiteAt || bubbles.Exists(b => b != null && (b.Committed || b.Bursting))) return false;
        var dash = target.GetComponent<PlayerDash>();
        nextBiteAt = Time.time + .85f + .42f + Mathf.Max(1.14f, dash == null ? 1.14f : dash.Cooldown + .04f);
        return true;
    }
    public void Spit(Vector2 from)
    {
        if (!CanContinue || root == null) return;
        bubbles.RemoveAll(b => b == null);
        if (bubbles.Count >= 2) return;
        var node = new GameObject("DestroyableSporeBubble"); node.layer = LayerMask.NameToLayer("Enemy"); node.transform.SetParent(root, false);
        node.AddComponent<Health>().SetMaximumHealth(1, false);
        var bubble = node.AddComponent<MacaquiniSporeBubble>();
        bubble.Configure(this, from, Vector2.Lerp(from, target.transform.position, .65f)); bubbles.Add(bubble);
    }
    private void FixedUpdate()
    {
        if (Time.timeScale <= 0) return;
        if (!CanContinue) { if (root != null) Cancel(); return; }
        Vector2 current = transform.position;
        if (combat.CurrentAttack == MacaquiniCombat.Attack.JetDash && combat.PerformingAttack && boss.Movement.IsDashing)
        {
            foreach (var helper in helpers)
                if (helper != null && helper.Alive && SegmentNear(previousBoss, current, helper.transform.position, .95f))
                { helper.GetComponent<Health>().TakeDamage(99); FriendlyHits++; }
            foreach (var bubble in bubbles)
                if (bubble != null && bubble.Alive && SegmentNear(previousBoss, current, bubble.transform.position, .95f))
                { bubble.GetComponent<Health>().TakeDamage(99); FriendlyHits++; }
        }
        previousBoss = current;
    }
    public static bool SegmentNear(Vector2 a, Vector2 b, Vector2 p, float radius)
    {
        Vector2 d = b - a;
        float u = Mathf.Clamp01(Vector2.Dot(p - a, d) / Mathf.Max(.0001f, d.sqrMagnitude));
        return (p - (a + d * u)).sqrMagnitude <= radius * radius;
    }
    public static bool HitPlayer(Health player, Vector2 a, Vector2 b, float radius)
    {
        if (player == null || player.IsDead) return false;
        foreach (var collider in player.GetComponentsInChildren<Collider2D>())
        {
            if (!collider.enabled || !collider.gameObject.activeInHierarchy) continue;
            Vector2 d = b - a;
            float u = Mathf.Clamp01(Vector2.Dot((Vector2)collider.bounds.center - a, d) / Mathf.Max(.0001f, d.sqrMagnitude));
            Vector2 p = a + u * d;
            if (Vector2.Distance(collider.ClosestPoint(p), p) <= radius) { player.TakeDamage(1); return true; }
        }
        return false;
    }
    public void Cancel()
    {
        StopAllCoroutines(); target = null;
        if (root != null) { root.gameObject.SetActive(false); Destroy(root.gameObject); root = null; }
        helpers.Clear(); bubbles.Clear();
    }
    private void OnDisable() => Cancel();
}
