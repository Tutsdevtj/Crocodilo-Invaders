using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D), typeof(HubPlayerInput))]
public sealed class HubCharacterMotor2D : MonoBehaviour
{
    [SerializeField, Min(0f)] private float maxMoveSpeed = 3f;
    [SerializeField, Min(.01f)] private float acceleration = 30f;
    [SerializeField, Min(.01f)] private float deceleration = 40f;
    [SerializeField, Min(.01f)] private float turnAcceleration = 42f;
    [SerializeField] private Transform visualRoot;
    private Rigidbody2D body;
    private HubPlayerInput input;
    private float visualWidth;
    public float MaxMoveSpeed => maxMoveSpeed;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        input = GetComponent<HubPlayerInput>();
        visualWidth = visualRoot == null ? 1f : Mathf.Abs(visualRoot.localScale.x);
    }

    private void FixedUpdate()
    {
        float horizontal = input.isActiveAndEnabled ? input.Horizontal : 0f;
        Vector2 velocity = body.linearVelocity;
        float target = horizontal * maxMoveSpeed;
        float rate = Mathf.Abs(horizontal) < .001f ? deceleration
            : velocity.x * target < 0f ? turnAcceleration : acceleration;
        body.linearVelocity = new Vector2(Mathf.MoveTowards(velocity.x, target, rate * Time.fixedDeltaTime), velocity.y);
        if (visualRoot != null && Mathf.Abs(horizontal) > .001f)
        {
            Vector3 scale = visualRoot.localScale;
            scale.x = Mathf.Sign(horizontal) * visualWidth;
            visualRoot.localScale = scale;
        }
    }
}
