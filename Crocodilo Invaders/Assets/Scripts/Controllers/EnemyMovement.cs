using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class EnemyMovement : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 2.5f;
    [SerializeField, Min(0f)] private float patrolSpeed = 1.4f;
    [SerializeField, Min(0.01f)] private float brakingDistance = 0.8f;
    [SerializeField, Range(0.1f, 1f)] private float hitSpeedMultiplier = 0.6f;
    [SerializeField, Min(0f)] private float hitSlowDuration = 0.1f;
    [SerializeField, Min(0.01f)] private float hitRecoveryDuration = 0.12f;

    private Rigidbody2D body;
    private Rect patrolArea;
    private Vector2 destination;
    private bool configured;
    private bool holding;
    private float dashSpeed;
    private float hitStartedAt = float.NegativeInfinity;
    public bool HasArrived { get; private set; }
    public bool IsDashing { get; private set; }

    private void Awake() => body = GetComponent<Rigidbody2D>();

    public void ConfigurePatrol(Rect area)
    {
        patrolArea = area;
        destination = new Vector2(Random.Range(area.xMin, area.xMax),
            Mathf.Clamp(body.position.y, area.yMin, area.yMax));
        configured = true;
        holding = false;
        IsDashing = false;
        HasArrived = false;
    }

    public void HoldPosition(bool hold)
    {
        holding = hold;
        if (hold) body.linearVelocity = Vector2.zero;
    }

    public void DashTo(Vector2 point, float speed)
    {
        destination = point;
        dashSpeed = Mathf.Max(.1f, speed);
        holding = false;
        IsDashing = true;
    }

    public void ReturnToPatrol() => ConfigurePatrol(patrolArea);

    public void ApplyHitSlow()
    {
        if (!isActiveAndEnabled) return;
        // Cada acerto renova a reação; a velocidade base nunca é alterada.
        hitStartedAt = Time.time;
    }

    private float HitSpeedFactor()
    {
        float recovery = (Time.time - hitStartedAt - hitSlowDuration) / hitRecoveryDuration;
        return Mathf.SmoothStep(hitSpeedMultiplier, 1f, Mathf.Clamp01(recovery));
    }

    private void FixedUpdate()
    {
        if (!configured) return;
        if (holding)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }
        Vector2 offset = destination - body.position;
        float distance = offset.magnitude;
        float speed = IsDashing ? dashSpeed : (HasArrived ? patrolSpeed : moveSpeed);
        // Freia perto do destino, sem parar no ponto nem teleportar entre posições.
        if (!IsDashing) speed *= Mathf.Clamp(distance / brakingDistance, 0.15f, 1f);
        speed *= HitSpeedFactor();
        if (distance <= Mathf.Max(0.02f, speed * Time.fixedDeltaTime))
        {
            body.linearVelocity = Vector2.zero;
            body.MovePosition(destination);
            if (IsDashing)
            {
                IsDashing = false;
                holding = true;
                return;
            }
            HasArrived = true;
            destination = new Vector2(Random.Range(patrolArea.xMin, patrolArea.xMax),
                Random.Range(patrolArea.yMin, patrolArea.yMax));
            return;
        }

        body.linearVelocity = offset / distance * speed;
    }

    private void OnDisable()
    {
        if (body != null) body.linearVelocity = Vector2.zero;
        holding = false;
        IsDashing = false;
        hitStartedAt = float.NegativeInfinity;
    }
}
