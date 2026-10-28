#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;

// Only saved in the Editor test scene, which is excluded from builds.
public sealed class MacaquiniDebugControls : MonoBehaviour
{
    private void Update()
    {
        var keys = Keyboard.current; if (keys == null) return;
        var boss = FindAnyObjectByType<MacaquiniCombat>(); if (boss == null) return;
        if (keys.f1Key.wasPressedThisFrame) boss.ForceAttack(MacaquiniCombat.Attack.BananaVolley);
        if (keys.f2Key.wasPressedThisFrame) boss.ForceAttack(MacaquiniCombat.Attack.BananaFan);
        if (keys.f3Key.wasPressedThisFrame) boss.ForceAttack(MacaquiniCombat.Attack.Vines);
        if (keys.hKey.wasPressedThisFrame) FindAnyObjectByType<Player>()?.Health.Heal(240);
    }
}
#endif
