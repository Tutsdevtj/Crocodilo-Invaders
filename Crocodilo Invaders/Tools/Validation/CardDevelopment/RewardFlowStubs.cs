// Only collaborators for testing the production controller's reroll gate.
// Coroutine execution, UI and rendering are deliberately not simulated.
using System;
using System.Collections;
public class WaveController
{
    public event Action<int,int> WaveCleared;
    public event Action<bool> RunEnded;
    public int CurrentWave => 1;
    public int CombatWaveCount => 9;
    public int WaveCount => 10;
    public bool RequestWaveReward() => true;
    public void CompleteWaveReward() { }
}
public class Health { public bool IsDead; }
public class Player : UnityEngine.MonoBehaviour { public Health Health = new Health(); }
public class PlayerInputReader : UnityEngine.MonoBehaviour { }
public class PlayerRunUpgrades { public bool Apply(UpgradeCard card) => true; }
public class PauseController
{
    public bool IsPaused;
    public event Action<bool> PauseChanged;
    public event Action MenuReturnRequested;
    public void AllowPauseWhileFrozen(bool allow) { }
    public void ResumeGame() { IsPaused = false; }
}
public class CardRewardView
{
    public event Action<int> ChoiceRequested;
    public event Action RerollRequested;
    public void SetPaused(bool paused) { }
    public void SetInteraction(bool ready, int remaining) { }
    public void HideImmediately() { }
    public IEnumerator Open(int wave, UpgradeCard[] offers, UpgradeCardCatalog catalog, PlayerRunUpgrades upgrades) { yield break; }
    public IEnumerator Reroll(UpgradeCard[] offers, UpgradeCardCatalog catalog, PlayerRunUpgrades upgrades) { yield break; }
    public IEnumerator Choose(int index, PlayerRunUpgrades upgrades) { yield break; }
}
namespace UnityEngine.InputSystem
{
    public class KeyControl { public bool wasPressedThisFrame; }
    public class Keyboard
    {
        public static Keyboard current;
        public KeyControl rKey, digit1Key, digit2Key, digit3Key;
    }
}
