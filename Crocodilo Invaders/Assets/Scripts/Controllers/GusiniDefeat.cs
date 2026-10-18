using System;
using UnityEngine;

// Derrota definitiva da F2: o parasita estoura e Gusini volta, exausto, na nave avariada.
// Os tempos vêm dos Animation Events do pacote Gusini_Derrota_F2_F1 v2; nada aqui espera por delay fixo.
[RequireComponent(typeof(Boss), typeof(GusiniPhaseTwo))]
public sealed class GusiniDefeat : MonoBehaviour
{
    public enum Stage { None, Collapsing, Recovered }
    [SerializeField] private RuntimeAnimatorController defeatController;
    [SerializeField] private string defeatState = "P2Defeat";
    [SerializeField] private string recoveredLoop = "P1DefeatedIdle";

    private Boss boss;
    private GusiniPhaseTwo phaseTwo;
    private Animator animator;
    private GusiniAudio audio;
    public Stage Current { get; private set; }
    public bool Recovered => Current == Stage.Recovered;
    public bool ParasiteDestroyed { get; private set; }
    public event Action ParasiteBurst;
    public event Action RecoveryCompleted;

    private void Awake()
    {
        boss = GetComponent<Boss>();
        phaseTwo = GetComponent<GusiniPhaseTwo>();
        animator = GetComponent<Animator>();
        audio = GetComponent<GusiniAudio>();
    }

    private void OnEnable() { if (defeatController != null) phaseTwo.PhaseTwoDefeated += Begin; }
    private void OnDisable() => phaseTwo.PhaseTwoDefeated -= Begin;

    // Roda dentro de Health.Depleted, antes de Died: o Boss fica em cena e a vitória antiga não destrói o Gusini.
    private void Begin()
    {
        if (Current != Stage.None) return;
        Current = Stage.Collapsing;
        boss.RetainAfterDefeat();
        animator.runtimeAnimatorController = defeatController;
        animator.speed = 1f;
        if (audio != null) audio.BeginDefeat();
        animator.Play(defeatState, 0, 0f);
    }

    public void OnDefeatEvent(string value)
    {
        if (Current != Stage.Collapsing) return;
        switch (value)
        {
            case "defeat_hit":
                GusiniPhaseTwoCameraFeedback.Pulse(Camera.main, .06f, .12f);
                if (audio != null) audio.Play(GusiniSound.P2FinalHit, transform.position);
                break;
            case "stagger_start":
                if (audio != null) audio.Play(GusiniSound.P2EngineStall, transform.position);
                break;
            case "parasite_burst":
                ParasiteDestroyed = true;
                GusiniPhaseTwoCameraFeedback.Pulse(Camera.main, .04f, .15f);
                if (audio != null) audio.Play(GusiniSound.P2EggBurst, transform.position);
                ParasiteBurst?.Invoke();
                break;
            case "clip_end":
                // Último quadro do pacote: nave avariada, respingos mortos e fumaça; segue no loop exausto.
                Current = Stage.Recovered;
                animator.Play(recoveredLoop, 0, 0f);
                RecoveryCompleted?.Invoke();
                break;
        }
    }
}
