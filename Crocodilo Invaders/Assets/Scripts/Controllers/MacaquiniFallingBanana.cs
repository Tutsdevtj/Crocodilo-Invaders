using UnityEngine;

// One visible fruit follows one fixed lane. Warning markers are harmless.
public sealed class MacaquiniFallingBanana : MonoBehaviour
{
    private Health target;
    private Boss boss;
    private SpriteRenderer fruit, trail, cue;
    private Vector2 from, overhead, previous;
    private float born, fireAt, bottom, cueY;
    private bool hit;
    public bool Falling => Time.time >= fireAt;
    public bool HitPlayer { get; private set; }
    public float LaneX => overhead.x;
    public void Configure(Boss owner, Health player, Sprite sprite, Vector2 source, Vector2 top, float warning, float bottomY)
    {
        boss = owner; target = player; from = source; overhead = top; born = Time.time; fireAt = born + warning; bottom = bottomY; cueY = player.transform.position.y;
        fruit = new GameObject("FallingBananaDrawing").AddComponent<SpriteRenderer>(); fruit.transform.SetParent(transform, false);
        fruit.sprite = sprite; fruit.sortingOrder = 33; fruit.transform.localScale = Vector3.one * .85f;
        trail = MacaquiniCombatPixels.Mark(transform, "FruitWindPixel", true, 31); trail.enabled = false;
        cue = MacaquiniCombatPixels.Mark(transform, "BananaLandingWarning", false, 26); cue.transform.position = new Vector2(top.x, player.transform.position.y); cue.transform.localScale = Vector3.one * 1.5f;
        previous = source; transform.position = source;
    }
    private void Update()
    {
        if (Time.timeScale <= 0) return;
        if (target == null || target.IsDead || boss == null || !boss.BattleActive || boss.Transitioning) { Destroy(gameObject); return; }
        Vector2 point;
        if (!Falling)
        {
            float t = Mathf.Clamp01((Time.time - born) / .75f);
            point = Vector2.Lerp(from, overhead, Mathf.SmoothStep(0, 1, t)) + Vector2.up * Mathf.Sin(t * Mathf.PI) * .8f;
            fruit.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, 95, t));
            cue.color = new Color(1, .84f, .26f, .5f + .15f * Mathf.Sin((Time.time - born) * 8));
        }
        else
        {
            point = overhead + Vector2.down * ((Time.time - fireAt) * 5.4f);
            fruit.transform.localRotation = Quaternion.Euler(0, 0, 95); cue.enabled = false;
            trail.enabled = true;
            if (!hit && MacaquiniPhaseOneHelpers.HitPlayer(target, previous, point, .22f)) { hit = true; HitPlayer = true; fruit.enabled = trail.enabled = false; Destroy(gameObject); }
            if (point.y < bottom) Destroy(gameObject);
        }
        transform.position = point; previous = point;
        if (trail.enabled) { trail.transform.position = point + Vector2.up * .35f; trail.transform.rotation = Quaternion.Euler(0, 0, 90); trail.transform.localScale = new Vector3(1.1f, .45f, 1); trail.color = new Color(.9f, 1, .52f, .5f); }
        // Cue stays at the locked lane rather than following either the fruit or player.
        if (cue.enabled) cue.transform.position = new Vector2(overhead.x, cueY);
    }
}
