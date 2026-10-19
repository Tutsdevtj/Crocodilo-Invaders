using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Reads the HUB action map; locomotion consumes the cached value in FixedUpdate.</summary>
[DisallowMultipleComponent]
public sealed class HubPlayerInput : MonoBehaviour
{
    [SerializeField] private InputActionAsset actions;
    private InputActionAsset instance;
    private InputAction move;
    public float Horizontal { get; private set; }

    private void Awake()
    {
        if (actions == null) { Debug.LogError("HubPlayerInput needs its HUB actions.", this); enabled = false; return; }
        instance = Instantiate(actions);
        move = instance.FindAction("Hub/Move", true);
    }

    private void OnEnable() => move?.Enable();
    private void Update() => Horizontal = move == null ? 0f : move.ReadValue<float>();
    private void OnDisable() { Horizontal = 0f; move?.Disable(); }
    private void OnDestroy() { if (instance != null) Destroy(instance); }
}
