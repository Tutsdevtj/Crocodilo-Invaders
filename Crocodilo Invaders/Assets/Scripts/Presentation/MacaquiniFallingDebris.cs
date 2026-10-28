using System;
using UnityEngine;

// World-space debris has no collider or damage component. Never expire while still above the map bottom.
public sealed class MacaquiniFallingDebris : MonoBehaviour
{
    private SpriteRenderer view;
    private float bottom;
    private Action<float> reachedBottom;
    public void Initialize(Sprite sprite, Vector2 velocity, float spin, float boundary, Action<float> onBottom)
    {
        bottom=boundary;reachedBottom=onBottom;
        view=gameObject.AddComponent<SpriteRenderer>();view.sprite=sprite;view.sortingOrder=32;
        var body=gameObject.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Dynamic;
        body.gravityScale=.8f;body.linearVelocity=velocity;body.angularVelocity=spin;
        body.interpolation=RigidbodyInterpolation2D.Interpolate;
    }
    private void LateUpdate()
    {
        if(view.bounds.max.y>=bottom)return;
        reachedBottom?.Invoke(view.bounds.max.y);reachedBottom=null;Destroy(gameObject);
    }
}
