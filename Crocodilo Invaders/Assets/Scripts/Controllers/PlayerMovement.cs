using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerMovement : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 0.6f;

    private Rigidbody2D body;
    private Vector2 moveInput;
    public float MoveSpeed => moveSpeed;
    public void SetMoveSpeed(float speed) => moveSpeed = Mathf.Max(0f, speed);

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void SetMoveInput(Vector2 direction)
    {
        moveInput = new Vector2(Mathf.Clamp(direction.x, -1f, 1f), Mathf.Clamp(direction.y, -1f, 1f));
    }

    private void FixedUpdate()
    {
        body.linearVelocity = moveInput * moveSpeed;
    }

    private void OnDisable()
    {
        moveInput = Vector2.zero;
        if (body != null) body.linearVelocity = Vector2.zero;
    }
}
