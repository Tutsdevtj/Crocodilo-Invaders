using UnityEngine;

// Shooting the seed cancels the burst; a boss dash cancels it through the same Health path.
public sealed class MacaquiniSporeBubble : MonoBehaviour
{
    private MacaquiniPhaseOneHelpers director;
    private Health health;
    private LineRenderer ring, cross;
    private Vector2 from, to;
    private float born, burstAt = -1;
    private bool neutralized;
    private readonly bool[] hit = new bool[4];
    private static readonly Vector2[] Directions = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
    public bool Alive => !neutralized && health != null && !health.IsDead && burstAt < 0;
    public bool Committed => Alive && Time.time - born >= 1.8f;
    public bool Bursting => burstAt >= 0;

    public void Configure(MacaquiniPhaseOneHelpers owner, Vector2 start, Vector2 finish)
    {
        director = owner; from = start; to = finish; born = Time.time; transform.position = start;
        health = GetComponent<Health>(); health.Died += Neutralize;
        var body = gameObject.AddComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic;
        var collider = gameObject.AddComponent<CircleCollider2D>(); collider.isTrigger = true; collider.radius = .3f;
        ring = MacaquiniFighter.MakeLine(transform, "SporeShell", director.Material, .075f, 34, 25);
        cross = MacaquiniFighter.MakeLine(transform, "FourWayWarning", director.Material, .025f, 28, 8);
    }
    private void Neutralize()
    {
        neutralized = true; gameObject.SetActive(false); Destroy(gameObject);
    }
    private void Update()
    {
        if (Time.timeScale <= 0) return;
        if (neutralized || director == null || !director.CanContinue) { Neutralize(); return; }
        float age = Time.time - born;
        if (burstAt < 0)
        {
            transform.position = Vector2.Lerp(from, to, Mathf.SmoothStep(0, 1, Mathf.Clamp01(age)));
            float radius = Mathf.Lerp(.16f, .34f, Mathf.Clamp01(age / 2.6f));
            for (int i = 0; i < 25; i++)
            {
                float angle = i * Mathf.PI * 2 / 24;
                ring.SetPosition(i, transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
            ring.enabled = true; ring.startColor = ring.endColor = Color.Lerp(new Color(.5f, .9f, .25f), new Color(1, .65f, .15f), Mathf.Clamp01(age / 2.6f));
            cross.enabled = age >= 1.8f;
            if (cross.enabled)
            {
                // One polyline returns to center between arms, so there are no diagonal hazards.
                for (int i = 0; i < 4; i++) { cross.SetPosition(i * 2, transform.position); cross.SetPosition(i * 2 + 1, (Vector2)transform.position + Directions[i] * 1.1f); }
                cross.startColor = cross.endColor = new Color(1, .74f, .22f, .65f);
            }
            // Never resolve a cross while the boss is dashing or purging gates.
            if (age >= 2.6f && !director.BiteCommitted && (!director.BossBusy || director.GetComponent<MacaquiniCombat>().CurrentAttack == MacaquiniCombat.Attack.JetStrafe))
            {
                burstAt = Time.time; ring.enabled = cross.enabled = false; GetComponent<Collider2D>().enabled = false;
            }
            return;
        }
        float elapsed = Time.time - burstAt;
        if (elapsed >= 1.6f) { Neutralize(); return; }
        // Four small, separated seeds, all moving straight; no tracking and no contact damage from the shell.
        for (int i = 0; i < 4; i++)
        {
            Vector2 previous = to + Directions[i] * (Mathf.Max(0, elapsed - Time.deltaTime) * 3.8f + .36f);
            Vector2 point = to + Directions[i] * (elapsed * 3.8f + .36f);
            if (!hit[i] && MacaquiniPhaseOneHelpers.HitPlayer(director.Target, previous, point, .11f)) hit[i] = true;
        }
        ring.enabled = false;
        DrawSeeds(elapsed);
    }
    private LineRenderer[] seeds;
    private void DrawSeeds(float elapsed)
    {
        if (seeds == null)
        {
            seeds = new LineRenderer[4];
            for (int i = 0; i < 4; i++) seeds[i] = MacaquiniFighter.MakeLine(transform, "SporeSeed" + i, director.Material, .13f, 33, 2);
        }
        for (int i = 0; i < 4; i++)
        {
            seeds[i].enabled = !hit[i];
            Vector2 p = to + Directions[i] * (elapsed * 3.8f + .36f);
            seeds[i].SetPosition(0, p); seeds[i].SetPosition(1, p - Directions[i] * .2f);
            seeds[i].startColor = seeds[i].endColor = new Color(.8f, 1, .28f);
        }
    }
    private void OnDestroy() { if (health != null) health.Died -= Neutralize; }
}
