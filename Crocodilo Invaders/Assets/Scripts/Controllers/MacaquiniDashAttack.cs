using UnityEngine;

// Damage travels with the physical boss. The announced path never deals damage by itself.
public sealed class MacaquiniDashAttack : MonoBehaviour
{
    private Boss boss;
    private Health target;
    private SpriteRenderer[] marks, wind;
    private Vector2 from, to, previous;
    private bool launched, finished, hit;
    private float born, ended;
    public bool Dangerous => launched && !finished && boss != null && boss.Movement.IsDashing;
    public bool HitPlayer { get; private set; }
    public Vector2 From => from;
    public Vector2 Destination => to;
    public void Configure(Boss owner, Health player, Vector2 destination)
    {
        boss = owner; target = player; from = previous = boss.transform.position; to = destination; born = Time.time;
        int count = Mathf.Clamp(Mathf.CeilToInt(Vector2.Distance(from, to) / .65f), 4, 24); marks = new SpriteRenderer[count];
        for (int i = 0; i < count; i++) { marks[i] = MacaquiniCombatPixels.Mark(transform, "DashWarningPixel", false, 25); marks[i].transform.position = Vector2.Lerp(from, to, (i + 1f) / count); marks[i].transform.localScale = Vector3.one * .65f; }
        wind = new SpriteRenderer[5]; for (int i = 0; i < wind.Length; i++) { wind[i] = MacaquiniCombatPixels.Mark(transform, "DashWindPixel", true, 31); wind[i].enabled = false; }
    }
    public void Launch() { launched = true; previous = boss.transform.position; foreach (var mark in marks) mark.enabled = false; }
    public void Complete()
    {
        if (finished) return;
        if (launched && !hit && target != null && MacaquiniPhaseOneHelpers.HitPlayer(target, previous, boss.transform.position, .72f)) { hit = true; HitPlayer = true; }
        finished = true; ended = Time.time;
    }
    private void FixedUpdate()
    {
        if (Time.timeScale <= 0 || !Dangerous) return;
        Vector2 current = boss.transform.position;
        if (!hit && MacaquiniPhaseOneHelpers.HitPlayer(target, previous, current, .72f)) { hit = true; HitPlayer = true; }
        previous = current;
    }
    private void LateUpdate()
    {
        if (Time.timeScale <= 0) return;
        if (boss == null || !boss.BattleActive || target == null || target.IsDead) { Destroy(gameObject); return; }
        if (!launched)
        {
            float pulse = .5f + .15f * Mathf.Sin((Time.time - born) * 8);
            foreach (var mark in marks) mark.color = new Color(1, .84f, .34f, pulse);
            return;
        }
        float fade = finished ? Mathf.Clamp01(1 - (Time.time - ended) / .18f) : 1;
        Vector2 direction = (to - from).normalized, side = Vector2.Perpendicular(direction);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        for (int i = 0; i < wind.Length; i++)
        {
            wind[i].enabled = fade > 0; wind[i].transform.position = (Vector2)boss.transform.position - direction * (.7f + i * .32f) + side * Mathf.Sin(i * 2.1f) * .35f;
            wind[i].transform.rotation = Quaternion.Euler(0, 0, angle);
            wind[i].transform.localScale = new Vector3(1.4f - i * .15f, .4f, 1);
            wind[i].color = new Color(.88f, 1, .72f, (.45f - i * .06f) * fade);
        }
        if (finished && fade <= 0) Destroy(gameObject);
    }
}
