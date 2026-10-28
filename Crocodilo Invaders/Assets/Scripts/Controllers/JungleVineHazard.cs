using System.Collections.Generic;
using UnityEngine;

// Warning and damage use the same line segment. Visual width never defines collision.
[RequireComponent(typeof(LineRenderer))]
public sealed class JungleVineHazard : MonoBehaviour
{
    [SerializeField, Min(.01f)] private float damageRadius = .18f;
    [SerializeField] private Sprite vineSprite;
    private readonly HashSet<Health> hit = new HashSet<Health>();
    private LineRenderer line;
    private LineRenderer painted;
    private MacaquiniVineRibbon ribbon;
    private Health target;
    private Vector2 start, end;
    private float fireAt, finishAt;
    public bool Dangerous { get; private set; }
    public Sprite VineSprite=>vineSprite;
    public Vector2 From=>start;
    public Vector2 To=>end;
    private void Awake()
    {
        line = GetComponent<LineRenderer>();painted = Extra("PaintedCrossingVine", damageRadius*2, line.sortingOrder);
        painted.useWorldSpace=true;painted.positionCount=2;ribbon=MacaquiniVineRibbon.Attach(painted,damageRadius*2);
    }
    private LineRenderer Extra(string name, float width, int order)
    {
        var node = new GameObject(name); node.transform.SetParent(transform, false); var extra = node.AddComponent<LineRenderer>();
        extra.sharedMaterial = line.sharedMaterial; extra.startWidth = extra.endWidth = width; extra.sortingOrder = order; extra.enabled = false; return extra;
    }
    public void Configure(Vector2 from, Vector2 to, Health player, float warning, float active)
    {
        line = GetComponent<LineRenderer>(); target = player; start = from; end = to;
        line.useWorldSpace=true;line.enabled=true;line.positionCount = 2; line.SetPosition(0, from); line.SetPosition(1, to);
        painted.SetPosition(0,from);painted.SetPosition(1,to);painted.enabled=false;Dangerous=false;hit.Clear();
        fireAt = Time.time + Mathf.Max(.1f, warning); finishAt = fireAt + Mathf.Max(.1f, active);
        line.startWidth = line.endWidth = .04f;
        line.startColor = line.endColor = new Color(1f, .78f, .24f, .7f);
    }
    private void Update()
    {
        if (target == null || target.IsDead || Time.time >= finishAt) { gameObject.SetActive(false); Destroy(gameObject); return; }
        if (Time.timeScale <= 0) return;
        if(Time.time<fireAt){line.startColor=line.endColor=new Color(1,.78f,.24f,.55f+.15f*Mathf.Sin(Time.time*8));return;}
        Dangerous = true;
        line.enabled=false;painted.enabled=true;ribbon.Tint=Color.white;
        Vector2 direction=end-start;
        foreach (Collider2D collider in Physics2D.OverlapCapsuleAll((start + end) * .5f,
            new Vector2(direction.magnitude + damageRadius * 2, damageRadius * 2), CapsuleDirection2D.Horizontal, Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg, 1 << target.gameObject.layer))
        {
            Health health = collider.GetComponentInParent<Health>();
            if (health == target && hit.Add(health)) health.TakeDamage(2f);
        }
    }
    private void OnDisable(){Dangerous=false;if(line!=null)line.enabled=false;if(painted!=null)painted.enabled=false;}
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((start + end) * .5f, new Vector3(Vector2.Distance(start, end), damageRadius * 2, 0));
    }
}
