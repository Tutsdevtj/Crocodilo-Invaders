using UnityEngine;

[RequireComponent(typeof(Enemy), typeof(Animator))]
public sealed class EnemyAttackPresentation : MonoBehaviour
{
    [SerializeField] private LineRenderer aimGuide;
    [SerializeField] private SpriteRenderer charge;
    [SerializeField] private Transform shotOrigin;
    [SerializeField] private bool showDashPath;
    [SerializeField, Range(10f, 100f)] private float fanAngle = 48f;

    private Enemy enemy;
    private Animator animator;
    private static readonly int Dashing = Animator.StringToHash("Dashing");

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        animator = GetComponent<Animator>();
        HideWarning();
    }

    private void OnEnable()
    {
        enemy.Combat.TelegraphStarted += ShowWarning;
        enemy.Combat.TelegraphFinished += HideWarning;
    }

    private void OnDisable()
    {
        enemy.Combat.TelegraphStarted -= ShowWarning;
        enemy.Combat.TelegraphFinished -= HideWarning;
        HideWarning();
    }

    private void ShowWarning(Vector2 destination)
    {
        if (aimGuide != null) aimGuide.enabled = true;
        if (charge != null) charge.enabled = true;
    }

    private void HideWarning()
    {
        if (aimGuide != null) aimGuide.enabled = false;
        if (charge != null) charge.enabled = false;
    }

    private void LateUpdate()
    {
        animator.SetBool(Dashing, enemy.Movement.IsDashing);
        if (!enemy.Combat.IsTelegraphing || aimGuide == null) return;
        Vector3 origin = shotOrigin != null ? shotOrigin.position : transform.position;
        if (showDashPath)
        {
            aimGuide.positionCount = 2;
            aimGuide.SetPosition(0, origin);
            aimGuide.SetPosition(1, enemy.Combat.WarningDestination);
        }
        else
        {
            aimGuide.positionCount = 13;
            Vector2 direction = enemy.Combat.WarningDestination - (Vector2)origin;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            for (int i = 0; i < 13; i++)
            {
                float radians = (angle + Mathf.Lerp(-fanAngle / 2f, fanAngle / 2f, i / 12f)) * Mathf.Deg2Rad;
                aimGuide.SetPosition(i, origin + new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * .85f);
            }
        }
    }
}
