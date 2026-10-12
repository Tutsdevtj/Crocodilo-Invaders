using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public sealed class PlayerInputReader : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference fireAction;
    [SerializeField] private InputActionReference specialAction;

    private Player player;
    private InputAction localMoveAction;
    private InputAction localFireAction;
    private InputAction localSpecialAction;

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
    }

    private void OnEnable()
    {
        localMoveAction?.Enable();
        localFireAction?.Enable();
        localSpecialAction?.Enable();
    }

    private void Update()
    {
        player.SetMoveInput(localMoveAction.ReadValue<Vector2>());
        // Segurar Espaço pede disparos; o combate decide quando pode executar cada um.
        if (localFireAction != null && localFireAction.IsPressed()) player.TryShoot();
        if (localSpecialAction != null && localSpecialAction.WasPressedThisFrame()) player.TrySpecialAttack();
    }

    private void OnDisable()
    {
        if (localMoveAction == null) return;

        localMoveAction.Disable();
        localFireAction?.Disable();
        localSpecialAction?.Disable();
        player.SetMoveInput(Vector2.zero);
    }

    private void OnDestroy()
    {
        localMoveAction?.Dispose();
        localFireAction?.Dispose();
        localSpecialAction?.Dispose();
    }
}
