using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(HubCharacterMotor2D), typeof(Rigidbody2D))]
public sealed class HubCharacterPresentation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private static readonly int Speed = Animator.StringToHash("Speed");
    private Rigidbody2D body;
    private HubCharacterMotor2D motor;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        motor = GetComponent<HubCharacterMotor2D>();
    }

    private void LateUpdate()
    {
        float speed = motor.isActiveAndEnabled && motor.MaxMoveSpeed > 0f
            ? Mathf.Abs(body.linearVelocity.x) / motor.MaxMoveSpeed : 0f;
        animator.SetFloat(Speed, speed);
    }

    private void OnDisable()
    {
        if (animator != null) animator.SetFloat(Speed, 0f);
    }
}
