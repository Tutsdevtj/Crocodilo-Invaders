using System.Collections;
using UnityEngine;

public sealed class MacaquiniJungleHelper : MonoBehaviour
{
    public enum Stage { Enter, Waiting, Warning, Strike, Recovery, Exit, Dead }
    private MacaquiniPhaseOneHelpers director;
    private Health health;
    private Rigidbody2D body;
    private SpriteRenderer view;
    private Collider2D hitbox;
    private Sprite[] poses;
    private LineRenderer warning, sporeCharge;
    private Vector2 spawn, home, from, end, previous;
    private float started, duration, delay, born;
    private bool hit, scored;
    public Stage CurrentStage { get; private set; }
    public bool Spitter { get; private set; }
    public bool Alive => health != null && !health.IsDead && CurrentStage != Stage.Dead;
    public bool Committed => Alive && !Spitter && (CurrentStage == Stage.Warning || CurrentStage == Stage.Strike);
    public Vector2 LockedAim { get; private set; }
    public int AnimationFrame { get; private set; }
    public int WingFramesShown { get; private set; }
    public int WingFrameCount => poses == null ? 0 : poses.Length;
    public Sprite CurrentDrawing => view == null ? null : view.sprite;
    private int lastAnimationFrame = -1;

    public void Configure(MacaquiniPhaseOneHelpers owner, bool spitter, Sprite[] sprites, Vector2 entry, Vector2 perch, float stagger)
    {
        director = owner; Spitter = spitter; poses = sprites; spawn = entry; home = perch; delay = stagger;
        transform.position = entry; born = Time.time;
        health = GetComponent<Health>(); health.Died += Die; health.Damaged += Flash;
        body = gameObject.AddComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic;
        var box = gameObject.AddComponent<BoxCollider2D>(); box.isTrigger = true;
        box.size = spitter ? new Vector2(.72f, .5f) : new Vector2(.9f, .55f); hitbox = box;
        var drawing = new GameObject("Drawing"); drawing.transform.SetParent(transform, false);
        view = drawing.AddComponent<SpriteRenderer>(); view.sortingOrder = 32; view.sprite = sprites[0];
        warning = MacaquiniFighter.MakeLine(transform, "LockedBitePath", director.Material, .035f, 29, 2);
        sporeCharge = MacaquiniFighter.MakeLine(transform, "MothSporeCharge", director.Material, .035f, 34, 17);
        SetStage(Stage.Enter, .8f); StartCoroutine(Life());
    }
    private void SetStage(Stage stage, float seconds) { CurrentStage = stage; started = Time.time; duration = seconds; }
    private bool Active => Alive && director != null && director.CanContinue;
    private IEnumerator Life()
    {
        yield return new WaitForSeconds(.8f + delay);
        if (!Active) yield break;
        SetStage(Stage.Waiting, 1);
        if (Spitter)
        {
            while (Active && director.BossBusy) yield return null;
            if (!Active) yield break;
            SetStage(Stage.Warning, .95f); yield return new WaitForSeconds(.95f);
            if (!Active) yield break;
            SetStage(Stage.Strike, .2f); director.Spit(body.position + new Vector2(-.22f, -.18f));
            yield return new WaitForSeconds(.2f);
        }
        else
        {
            while (Active && !director.TryReserveBite()) yield return null;
            if (!Active) yield break;
            LockedAim = director.Target.transform.position; from = body.position;
            // A short overshoot makes the commitment visible. Aim never follows the player.
            Vector2 direction = (LockedAim - from).normalized; end = LockedAim + direction * .8f;
            view.flipX = direction.x > 0;
            SetStage(Stage.Warning, .85f); yield return new WaitForSeconds(.85f);
            if (!Active) yield break;
            previous = body.position; SetStage(Stage.Strike, .42f);
            yield return new WaitForSeconds(.42f);
        }
        if (!Active) yield break;
        SetStage(Stage.Recovery, 1.05f); yield return new WaitForSeconds(1.05f);
        if (!Active) yield break;
        from = body.position; end = new Vector2(spawn.x, home.y + 1.5f);
        SetStage(Stage.Exit, .8f); yield return new WaitForSeconds(.8f);
        Destroy(gameObject);
    }
    private void FixedUpdate()
    {
        if (Time.timeScale <= 0 || !Alive) return;
        if (director == null || !director.CanContinue) { gameObject.SetActive(false); Destroy(gameObject); return; }
        float t = Mathf.Clamp01((Time.time - started) / duration);
        if (CurrentStage == Stage.Enter) body.position = Vector2.Lerp(spawn, home, Mathf.SmoothStep(0, 1, t));
        else if (CurrentStage == Stage.Strike && !Spitter)
        {
            body.position = Vector2.Lerp(from, end, Mathf.SmoothStep(0, 1, t));
            if (!hit && MacaquiniPhaseOneHelpers.HitPlayer(director.Target, previous, body.position, .24f)) hit = true;
            previous = body.position;
        }
        else if (CurrentStage == Stage.Exit) body.position = Vector2.Lerp(from, end, Mathf.SmoothStep(0, 1, t));
        else if (CurrentStage == Stage.Waiting || Spitter && (CurrentStage == Stage.Warning || CurrentStage == Stage.Recovery))
            body.position = home + new Vector2(Mathf.Sin((Time.time - born) * 2.2f) * (Spitter ? .18f : 0), Mathf.Sin((Time.time - born) * 4) * .09f);
        if (Time.time - born > 15f) Destroy(gameObject);
    }
    private void LateUpdate()
    {
        if (view == null || Time.timeScale <= 0) return;
        if (CurrentStage == Stage.Dead)
        {
            float t = Mathf.Clamp01((Time.time - started) / .22f);
            view.color = new Color(1, 1, 1, 1 - t); if (t >= 1) Destroy(gameObject); return;
        }
        // Real full-body cels keep flapping through entry, warning, attack and exit.
        AnimationFrame = Mathf.FloorToInt(((Time.time - born) + delay * .37f) * (Spitter ? 24f : 16f)) % poses.Length;
        if (AnimationFrame != lastAnimationFrame) { WingFramesShown++; lastAnimationFrame = AnimationFrame; }
        view.sprite = poses[AnimationFrame]; view.color = Color.Lerp(view.color, Color.white, Time.deltaTime * 10);
        view.transform.localPosition = Vector3.zero;
        view.transform.localRotation = Quaternion.Euler(0, 0, !Spitter && CurrentStage == Stage.Strike ? 12 : 0);
        warning.enabled = CurrentStage == Stage.Warning && !Spitter;
        if (warning.enabled)
        {
            warning.SetPosition(0, from); warning.SetPosition(1, end);
            warning.startColor = warning.endColor = new Color(1, .7f, .16f, .8f);
        }
        sporeCharge.enabled = Spitter && CurrentStage == Stage.Warning;
        if (sporeCharge.enabled)
        {
            float t = Mathf.Clamp01((Time.time - started) / duration);
            Vector2 center = body.position + new Vector2(-.22f, -.18f);
            for (int i = 0; i < 17; i++) { float angle = i * Mathf.PI * 2 / 16; sporeCharge.SetPosition(i, center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Mathf.Lerp(.08f, .24f, t)); }
            sporeCharge.startColor = sporeCharge.endColor = new Color(.65f, 1, .25f, .85f);
        }
    }
    private void Flash() { if (view != null) view.color = new Color(1, .7f, .4f); }
    private void Die()
    {
        if (scored) return; scored = true; StopAllCoroutines(); SetStage(Stage.Dead, .22f);
        hitbox.enabled = false; warning.enabled = sporeCharge.enabled = false;
        FindAnyObjectByType<RunScore>()?.RecordDefeat(Spitter ? 250 : 150);
    }
    private void OnDestroy() { if (health != null) { health.Died -= Die; health.Damaged -= Flash; } }
}
