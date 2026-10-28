using System.Collections.Generic;
using UnityEngine;

// Warning and damage use the same line segment. Visual width never defines collision.
[RequireComponent(typeof(LineRenderer))]
public sealed class JungleVineHazard : MonoBehaviour
{
    [SerializeField, Min(.01f)] private float damageRadius = .18f;
    [SerializeField] private Sprite vineSprite;
    private readonly List<Transform> segments = new List<Transform>();
    private readonly HashSet<Health> hit = new HashSet<Health>();
    private LineRenderer line;
    private LineRenderer outline, core;
    private Health target;
    private Vector2 start, end;
    private float fireAt, finishAt;
    public bool Dangerous { get; private set; }
    public Sprite VineSprite=>vineSprite;
    private void Awake()
    {
        line = GetComponent<LineRenderer>(); outline = Extra("BarkOutline", .64f, line.sortingOrder - 1); core = Extra("VineVein", .08f, line.sortingOrder + 1);
    }
    private LineRenderer Extra(string name, float width, int order)
    {
        var node = new GameObject(name); node.transform.SetParent(transform, false); var extra = node.AddComponent<LineRenderer>();
        extra.sharedMaterial = line.sharedMaterial; extra.startWidth = extra.endWidth = width; extra.sortingOrder = order; extra.enabled = false; return extra;
    }
    public void Configure(Vector2 from, Vector2 to, Health player, float warning, float active)
    {
        line = GetComponent<LineRenderer>(); target = player; start = from; end = to;
        line.positionCount = 2; line.SetPosition(0, from); line.SetPosition(1, to);
        fireAt = Time.time + Mathf.Max(.1f, warning); finishAt = fireAt + Mathf.Max(.1f, active);
        line.startWidth = line.endWidth = damageRadius * 2;
        line.startColor = line.endColor = new Color(1f, .75f, .15f, .75f);
        if (vineSprite != null)
        {
            float step = vineSprite.bounds.size.y * .85f - .13f;
            int count = Mathf.CeilToInt(Vector2.Distance(start, end) / step) + 1;
            for (int i = 0; i < count; i++)
            {
                var node = new GameObject("VineSegment_" + i); node.transform.SetParent(transform, false);
                var view = node.AddComponent<SpriteRenderer>(); view.sprite = vineSprite; view.sortingOrder = line.sortingOrder;
                view.color = new Color(.85f, 1f, .7f); node.transform.localScale = Vector3.one * .85f; node.SetActive(false); segments.Add(node.transform);
            }
        }
    }
    private void Update()
    {
        if (target == null || target.IsDead || Time.time >= finishAt) { gameObject.SetActive(false); Destroy(gameObject); return; }
        if (Time.timeScale <= 0 || Time.time < fireAt) return;
        Dangerous = true;
        if (segments.Count > 0)
        {
            line.enabled = outline.enabled = core.enabled = false;
            float step = vineSprite.bounds.size.y * .85f - .13f;
            Vector2 direction = (end - start).normalized;
            for (int i = 0; i < segments.Count; i++)
            {
                segments[i].gameObject.SetActive(true);
                Vector3 point = start + direction * (step * i); point.y += Mathf.Sin(Time.time * 20 + i * 1.2f) * .045f;
                segments[i].position = point;
                segments[i].rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90 + Mathf.Sin(Time.time * 18 + i) * 1.8f);
            }
        }
        else
        {
        line.startWidth = line.endWidth = .5f;
        line.startColor = line.endColor = new Color(.2f, .62f, .12f);
        outline.enabled = core.enabled = true;
        outline.startColor = outline.endColor = new Color(.06f, .12f, .03f);
        core.startColor = core.endColor = new Color(.65f, .94f, .22f);
        line.positionCount = outline.positionCount = core.positionCount = 33;
        for (int i = 0; i < 33; i++)
        {
            float t = i / 32f; Vector3 point = Vector2.Lerp(start, end, t);
            point.y += Mathf.Sin(i * .8f - Time.time * 24) * .045f * Mathf.Sin(t * Mathf.PI);
            line.SetPosition(i, point); outline.SetPosition(i, point); core.SetPosition(i, point);
        }
        }
        foreach (Collider2D collider in Physics2D.OverlapCapsuleAll((start + end) * .5f,
            new Vector2(Vector2.Distance(start, end) + damageRadius * 2, damageRadius * 2), CapsuleDirection2D.Horizontal, 0f, 1 << target.gameObject.layer))
        {
            Health health = collider.GetComponentInParent<Health>();
            if (health == target && hit.Add(health)) health.TakeDamage(2f);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((start + end) * .5f, new Vector3(Vector2.Distance(start, end), damageRadius * 2, 0));
    }
}
