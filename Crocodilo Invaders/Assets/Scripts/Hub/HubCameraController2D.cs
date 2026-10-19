using UnityEngine;
using UnityEngine.U2D;

/// <summary>A persistent camera window with separately damped, confirmed directional focus.</summary>
[DefaultExecutionOrder(900), DisallowMultipleComponent, RequireComponent(typeof(Camera))]
public sealed class HubCameraController2D : MonoBehaviour
{
    [SerializeField] private Transform trackingTarget;
    [SerializeField, Range(0f, .6f)] private float deadZoneWidth = .20f;
    [SerializeField, Range(0f, .3f)] private float lookAhead = .12f;
    [SerializeField, Min(.01f)] private float horizontalDamping = .26f;
    [SerializeField, Min(.01f)] private float lookAheadDamping = .4f;
    [SerializeField, Min(0f)] private float directionConfirmation = .14f;
    [SerializeField, Min(.01f)] private float maxFollowSpeed = 12f;
    [SerializeField] private bool verticalFollow;
    [SerializeField] private float baseHeight = 2.4f;
    [SerializeField, Min(.1f)] private float subjectHeight = 1.8f;
    [SerializeField, Min(0f)] private float verticalDeadZone = .8f;
    [SerializeField, Min(.01f)] private float verticalDamping = .3f;
    [SerializeField] private Rect cameraBounds = new Rect(-13.5f, -2.1f, 120f, 9f);
    [SerializeField, Min(.1f)] private float maximumOrthoSize = 4.5f;
    [SerializeField, Min(1f), Tooltip("Maximum visible width supported by the existing parallax art.")]
    private float maximumViewWidth = 21f;
    [SerializeField] private PixelPerfectCamera pixelPerfect;
    [SerializeField] private bool drawGizmos = true;
    private Camera view;
    private Vector3 previousTarget;
    private Vector2 window, goal;
    private float xVelocity, yVelocity, lookVelocity, currentLook, desiredLook;
    private float filteredSpeed, candidateTime;
    private int candidateDirection;
    private bool initialized;
    public Transform TrackingTarget => trackingTarget;
    public Rect CameraBounds => cameraBounds;
    public Vector2 WindowCenter => window;
    public float CurrentLookAhead => currentLook;
    public float DeadZoneWidth => deadZoneWidth;

    private void Awake() => view = GetComponent<Camera>();
    private void OnEnable() => initialized = false;

    // The official component owns render snapping only. This public lens adapter does not
    // require Cinemachine: it prevents Pixel Perfect's OnPreCull from overwriting our lens.
    private void UpdateLens()
    {
        if (pixelPerfect == null || !pixelPerfect.isActiveAndEnabled) return;
        int height = Mathf.Max(1, Screen.height);
        float limit = Mathf.Min(maximumOrthoSize, maximumViewWidth / (2f * view.aspect));
        int zoom = Mathf.Max(1, Mathf.CeilToInt(height / (2f * pixelPerfect.assetsPPU * limit) - .0001f));
        float ortho = height / (2f * pixelPerfect.assetsPPU * zoom);
        view.orthographicSize = pixelPerfect.CorrectCinemachineOrthoSize(ortho);
    }

    public void SnapToTarget()
    {
        if (view == null) view = GetComponent<Camera>();
        if (trackingTarget == null) return;
        UpdateLens();
        previousTarget = trackingTarget.position;
        window = new Vector2(previousTarget.x, verticalFollow ? previousTarget.y : FramingHeight());
        goal = Confine(window);
        transform.position = new Vector3(goal.x, goal.y, transform.position.z);
        xVelocity = yVelocity = lookVelocity = currentLook = desiredLook = filteredSpeed = candidateTime = 0f;
        candidateDirection = 0;
        initialized = true;
    }

    private void LateUpdate()
    {
        if (trackingTarget == null) return;
        UpdateLens();
        if (!initialized || (trackingTarget.position - previousTarget).sqrMagnitude > 36f) SnapToTarget();
        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        Vector3 target = trackingTarget.position;
        filteredSpeed = Mathf.Lerp(filteredSpeed, (target.x - previousTarget.x) / dt, 1f - Mathf.Exp(-dt / .08f));
        previousTarget = target;
        float width = 2f * view.orthographicSize * view.aspect;
        float halfWindow = width * deadZoneWidth * .5f;
        float excess = target.x - window.x;
        bool movedWindow = Mathf.Abs(excess) > halfWindow;
        if (movedWindow) window.x = target.x - Mathf.Sign(excess) * halfWindow;

        int direction = Mathf.Abs(filteredSpeed) > .15f ? (int)Mathf.Sign(filteredSpeed) : 0;
        if (direction != candidateDirection) { candidateDirection = direction; candidateTime = 0f; }
        else candidateTime += dt;
        if (movedWindow && direction != 0 && candidateTime >= directionConfirmation)
            // Compensate the window's trailing half-width so the final framing really
            // places the moving target 12% of a viewport behind centre, rather than 2%.
            desiredLook = direction * (halfWindow + width * lookAhead);
        // On release keep the established composition. No automatic recenter or drift.
        currentLook = Mathf.SmoothDamp(currentLook, desiredLook, ref lookVelocity, lookAheadDamping, Mathf.Infinity, dt);
        if (verticalFollow)
        {
            float dy = target.y - window.y;
            if (Mathf.Abs(dy) > verticalDeadZone) window.y = target.y - Mathf.Sign(dy) * verticalDeadZone;
        }
        else window.y = FramingHeight();
        goal = Confine(new Vector2(window.x + currentLook, window.y));
        Vector2 position = transform.position;
        position.x = Mathf.SmoothDamp(position.x, goal.x, ref xVelocity, horizontalDamping, maxFollowSpeed, dt);
        position.y = Mathf.SmoothDamp(position.y, goal.y, ref yVelocity, verticalDamping, maxFollowSpeed, dt);
        // A hard framing margin catches unusually fast targets, without allowing them to
        // drag the viewport beyond the visual bounds. Normal walking never hits this rail.
        position.x = Mathf.Clamp(position.x, target.x - width * .38f, target.x + width * .38f);
        position = Confine(position);
        if (Mathf.Abs(position.x - goal.x) < .0001f && Mathf.Abs(xVelocity) < .001f) { position.x = goal.x; xVelocity = 0f; }
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    public Vector2 Confine(Vector2 position)
    {
        if (view == null) view = GetComponent<Camera>();
        // One art pixel protects the edge from the render camera's rounding.
        float guard = pixelPerfect != null && pixelPerfect.isActiveAndEnabled ? 1f / pixelPerfect.assetsPPU : 0f;
        float halfY = view.orthographicSize, halfX = halfY * view.aspect;
        position.x = ClampAxis(position.x, cameraBounds.xMin, cameraBounds.xMax, halfX + guard);
        position.y = ClampAxis(position.y, cameraBounds.yMin, cameraBounds.yMax, halfY);
        return position;
    }

    // Keep ground and the whole sprite visible when integer zoom produces a shorter
    // viewport. This depends only on window size, never on Idle or physical bobbing.
    private float FramingHeight()
    {
        float minimum = subjectHeight + .1f - view.orthographicSize;
        float maximum = view.orthographicSize - .35f;
        return minimum <= maximum ? Mathf.Clamp(baseHeight, minimum, maximum) : subjectHeight * .5f;
    }

    private static float ClampAxis(float value, float min, float max, float halfView) =>
        max - min <= 2f * halfView ? (min + max) * .5f : Mathf.Clamp(value, min + halfView, max - halfView);

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        if (view == null) view = GetComponent<Camera>();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(new Vector3(cameraBounds.center.x, cameraBounds.center.y), new Vector3(cameraBounds.width, cameraBounds.height));
        Vector2 center = Application.isPlaying ? window : (Vector2)transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, new Vector3(2f * view.orthographicSize * view.aspect * deadZoneWidth, 2f * view.orthographicSize));
        if (trackingTarget == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(trackingTarget.position, .1f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(center, center + Vector2.right * currentLook);
        Gizmos.DrawWireSphere(goal, .12f);
    }
}
