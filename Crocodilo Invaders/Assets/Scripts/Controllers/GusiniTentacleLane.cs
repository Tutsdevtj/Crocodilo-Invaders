using System.Collections.Generic;
using UnityEngine;

public sealed class GusiniTentacleLane : MonoBehaviour
{
    private GusiniPhaseTwoAssets assets;
    private SpriteRenderer visual, top, bottom, arrow;
    private BoxCollider2D hitbox;
    private readonly HashSet<Health> contacted = new HashSet<Health>();
    private bool warning, cracked;
    private float age, pixelX, pixelY;
    private GusiniAudio audio;
    private bool playTipSound;
    // Coordenadas originais do JSON; espinhos dorsais ficam fora do retângulo.
    private static readonly int[] HitboxFrom = { 454, 272, 103, 11, 18, 163, 342, 512 };
    private static readonly float[] Ends = { .05f, .1f, .16f, .24f, .32f, .39f, .46f, .53f };
    public static GusiniTentacleLane Create(GusiniPhaseTwoAssets assets, Transform parent, Rect arena, float y, bool warning, GusiniAudio audio = null, bool playTipSound = true)
    {
        var root = new GameObject(warning ? "Tentacle lane warning" : "Tentacle lash"); root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(arena.xMax, y, 0);
        float px = arena.width / 600f, py = arena.height / 360f;
        root.transform.localScale = new Vector3(px * 100f, py * 100f, 1f);
        var visual = root.AddComponent<SpriteRenderer>(); visual.sortingOrder = warning ? 8 : 26;
        var lane = root.AddComponent<GusiniTentacleLane>(); lane.assets = assets; lane.visual = visual;
        lane.warning = warning; lane.pixelX = px; lane.pixelY = py;
        lane.audio = audio; lane.playTipSound = playTipSound;
        if (warning)
        {
            visual.sprite = assets.Get("lane_warning_fill_0"); visual.drawMode = SpriteDrawMode.Tiled;
            // O fill é centrado, o tentáculo é ancorado à direita.
            visual.size = new Vector2(6f, .84f);
            var fill = new GameObject("Warning fill"); fill.transform.SetParent(root.transform, false); fill.transform.localPosition = new Vector3(-3f, 0, 0);
            var fillVisual = fill.AddComponent<SpriteRenderer>(); fillVisual.sortingOrder = 8; fillVisual.sprite = visual.sprite;
            fillVisual.drawMode = SpriteDrawMode.Tiled; fillVisual.size = visual.size;
            UnityEngine.Object.Destroy(visual); lane.visual = fillVisual;
            lane.top = lane.Edge(root.transform, .42f); lane.bottom = lane.Edge(root.transform, -.42f);
            var arrow = new GameObject("Warning arrow"); arrow.transform.SetParent(root.transform, false); arrow.transform.localPosition = new Vector3(-.22f, 0, 0);
            lane.arrow = arrow.AddComponent<SpriteRenderer>(); lane.arrow.sortingOrder = 10; lane.arrow.sprite = assets.Get("lane_warning_arrow_0");
        }
        else
        {
            visual.sprite = assets.Get("lane_tentacle_0");
            var hitbox = root.AddComponent<BoxCollider2D>(); hitbox.isTrigger = true; lane.hitbox = hitbox;
            var body = root.AddComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic; body.useFullKinematicContacts = true;
            lane.SetFrame(0);
        }
        return lane;
    }
    private SpriteRenderer Edge(Transform parent, float y)
    {
        var root = new GameObject("Warning edge"); root.transform.SetParent(parent, false); root.transform.localPosition = new Vector3(-3f, y, 0);
        var image = root.AddComponent<SpriteRenderer>(); image.sortingOrder = 9; image.sprite = assets.Get("lane_warning_edge_0");
        image.drawMode = SpriteDrawMode.Tiled; image.size = new Vector2(6f, .06f); return image;
    }
    private void SetFrame(int frame)
    {
        visual.sprite = assets.Get("lane_tentacle_" + frame);
        hitbox.size = new Vector2((600 - HitboxFrom[frame]) / 100f, .66f);
        hitbox.offset = new Vector2((HitboxFrom[frame] - 600) / 200f, .01f);
        if (frame == 3 && !cracked)
        {
            cracked = true;
            if (audio != null && playTipSound) audio.Play(GusiniSound.P2TipCrack, transform.position, transform.parent);
            GusiniPhaseTwoEffect.Create(assets.Frames("fx_tentacle_crack_", 4), new[] { .033f, .033f, .033f, .033f },
                transform.parent, transform.position + Vector3.left * (593f * pixelX), pixelY);
        }
    }
    private void Update()
    {
        age += Time.deltaTime;
        if (warning)
        {
            if (age >= .9f) { Destroy(gameObject); return; }
            int frame = (int)(age / .12f) % 2;
            visual.sprite = assets.Get("lane_warning_fill_" + frame);
            top.sprite = bottom.sprite = assets.Get("lane_warning_edge_" + frame);
            arrow.sprite = assets.Get("lane_warning_arrow_" + frame);
        }
        else
        {
            if (age >= .53f) { hitbox.enabled = false; Destroy(gameObject); return; }
            int frame = 0; while (frame < 7 && age >= Ends[frame]) frame++;
            SetFrame(frame);
        }
    }
    private void OnTriggerEnter2D(Collider2D other) => Contact(other);
    private void OnTriggerStay2D(Collider2D other) => Contact(other);
    private void Contact(Collider2D other)
    {
        if (warning || other.gameObject.layer != 8) return;
        var target = other.GetComponentInParent<Health>();
        // Uma esquiva consome este golpe mesmo que o player continue na mesma faixa.
        if (target == null || !contacted.Add(target)) return;
        target.TakeDamage(4);
    }
}
