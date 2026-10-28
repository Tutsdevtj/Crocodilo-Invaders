using UnityEngine;

// One painted character per cel, including vine, weapon, face, legs, feet and tail.
[RequireComponent(typeof(SpriteRenderer))]
public sealed class MacaquiniFrameAnimation : MonoBehaviour
{
    public enum Motion { Swing, Attack, Tear }
    [SerializeField] private Sprite[] closedSwing, halfSwing, closedAttack, halfAttack, tearing;
    private SpriteRenderer view;
    private float elapsed;
    private int swingDirection = 1;
    public Motion CurrentMotion { get; private set; }
    public bool HalfShell { get; private set; }
    public bool Paused { get; set; }
    public bool RepeatAttack { get; set; }
    public float Speed { get; set; } = 1;
    public int Frame { get; private set; }
    public int CompletedLoops { get; private set; }
    public Sprite[] CurrentFrames => CurrentMotion == Motion.Tear ? tearing : CurrentMotion == Motion.Attack ? (HalfShell ? halfAttack : closedAttack) : HalfShell ? halfSwing : closedSwing;
    public int TotalDrawings => closedSwing.Length + halfSwing.Length + closedAttack.Length + halfAttack.Length + tearing.Length;
    public Sprite CurrentSprite => CurrentFrames[Frame];
    private static readonly float[] AttackHolds = { .16f,.16f,.20f,.12f,.13f,.13f,.18f,.24f };
    private static readonly float[] TearHolds = { .4f,.45f,.55f,.6f,.65f,.65f,.5f,.42f };
    private void Awake() { view = GetComponent<SpriteRenderer>(); Display(); }
    private void Update() { if (!Paused) Advance(Time.deltaTime); }
    public void SetForm(bool half) { HalfShell = half; Play(Motion.Swing); }
    public void Play(Motion motion)
    {
        if (motion == Motion.Tear) HalfShell = false;
        CurrentMotion = motion; Frame = 0; elapsed = 0; swingDirection = 1; Display();
    }
    public void Seek(int index) { Frame = (index % CurrentFrames.Length + CurrentFrames.Length) % CurrentFrames.Length; elapsed = 0; swingDirection = 1; Display(); }
    public static float Hold(Motion motion,int frame) => motion == Motion.Tear ? TearHolds[frame] : motion == Motion.Attack ? AttackHolds[frame] : .125f;
    public void Advance(float seconds)
    {
        elapsed += Mathf.Max(0, seconds) * Mathf.Clamp(Speed, .25f, 2);
        for (int guard = 0; guard < 100; guard++)
        {
            float hold = Hold(CurrentMotion,Frame);
            if (elapsed < hold) break;
            elapsed -= hold;
            if(CurrentMotion == Motion.Swing)
            {
                Frame += swingDirection;
                if(Frame >= CurrentFrames.Length){Frame=CurrentFrames.Length-2;swingDirection=-1;}
                else if(Frame<0){Frame=1;swingDirection=1;CompletedLoops++;}
                continue;
            }
            Frame++;
            if (Frame >= CurrentFrames.Length)
            {
                CompletedLoops++; Frame = 0;
                if (CurrentMotion == Motion.Tear) { HalfShell = true; CurrentMotion = Motion.Swing; }
                else if (CurrentMotion == Motion.Attack && !RepeatAttack) CurrentMotion = Motion.Swing;
            }
        }
        Display();
    }
    private void Display() { if (view == null) view = GetComponent<SpriteRenderer>(); if (CurrentFrames != null && CurrentFrames.Length > 0) view.sprite = CurrentSprite; }
}
