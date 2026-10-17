using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public sealed class PlayerInputReader : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference fireAction;
    [SerializeField] private InputActionReference specialAction;
    [SerializeField] private InputActionReference dashAction;

    private Player player;
    private InputAction localMoveAction;
    private InputAction localFireAction;
    private InputAction localSpecialAction;
    private InputAction localDashAction;

    private void Awake()
    {
        player = GetComponent<Player>();

        if (moveAction == null || moveAction.action == null)
        {
            Debug.LogError("Configurar a ação Move no PlayerInputReader", this);
            enabled = false;
            return;
        }

        localMoveAction = moveAction.action.Clone();
        localFireAction = fireAction?.action?.Clone();
        localSpecialAction = specialAction?.action?.Clone();
        localDashAction = dashAction?.action?.Clone();
    }

    private void OnEnable()
    {
        localMoveAction?.Enable();
        localFireAction?.Enable();
        localSpecialAction?.Enable();
        localDashAction?.Enable();
    }

    private void Update()
    {
        player.SetMoveInput(localMoveAction.ReadValue<Vector2>());
        if (localDashAction != null && localDashAction.WasPressedThisFrame()) player.TryDash();
        // Segurar Espaço pede disparos; o combate decide quando pode executar cada um.
        // If both buttons arrive together, give a charged special its windup first.
        bool specialStarted = localSpecialAction != null && localSpecialAction.WasPressedThisFrame()
            && player.TrySpecialAttack();
        if (!specialStarted && localFireAction != null && localFireAction.IsPressed()) player.TryShoot();
    }

    private void OnDisable()
    {
        if (localMoveAction == null) return;

        localMoveAction.Disable();
        localFireAction?.Disable();
        localSpecialAction?.Disable();
        localDashAction?.Disable();
        player.SetMoveInput(Vector2.zero);
    }

    private void OnDestroy()
    {
        localMoveAction?.Dispose();
        localFireAction?.Dispose();
        localSpecialAction?.Dispose();
        localDashAction?.Dispose();
    }
}
