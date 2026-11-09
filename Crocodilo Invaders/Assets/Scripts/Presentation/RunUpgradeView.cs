using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Read-only scoreboard: never changes time, sound, player input or EventSystem focus.
public sealed class RunUpgradeView : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private RectTransform panel;
    [SerializeField] private TMP_Text count;
    [SerializeField] private TMP_Text[] values;
    [SerializeField] private UnityEngine.UI.Image[] segments;
    private PlayerRunUpgrades upgrades;
    private Health health;
    private PlayerInputReader input;
    private PauseController pause;
    private CardRewardController rewards;
    private DialogueRunner dialogue;
    private bool waitingForRelease;
    private RectTransform canvasRect;
    public bool IsVisible => content.activeSelf;
    public bool CanShow => isActiveAndEnabled && upgrades != null && health != null && !health.IsDead && input != null && input.enabled
        && Time.timeScale > 0f && (pause == null || !pause.IsPaused) && (rewards == null || rewards.State == CardRewardController.RewardState.Closed) && (dialogue == null || !dialogue.IsPlaying);
    private void Awake() { content.SetActive(false); canvasRect=(RectTransform)transform; }
    public void Bind(PlayerRunUpgrades target)
    {
        if(upgrades != null) upgrades.Changed-=Refresh;
        upgrades=target;
        health=target!=null?target.GetComponent<Health>():null;
        input=target!=null?target.GetComponent<PlayerInputReader>():null;
        pause=FindAnyObjectByType<PauseController>();rewards=FindAnyObjectByType<CardRewardController>();dialogue=FindAnyObjectByType<DialogueRunner>();
        if(upgrades != null) upgrades.Changed+=Refresh;
        Hide();Refresh();
    }
    private void LateUpdate()
    {
        bool held=Keyboard.current!=null&&Keyboard.current.tabKey.isPressed || Gamepad.current!=null&&Gamepad.current.selectButton.isPressed;
        // Tab also skips dialogue: the held key must not open this panel afterwards.
        if(!held)waitingForRelease=false;
        else if(dialogue!=null&&dialogue.IsPlaying)waitingForRelease=true;
        bool visible=held&&!waitingForRelease&&CanShow;
        if(content.activeSelf!=visible){if(visible)Refresh();content.SetActive(visible);}
        panel.localScale=Vector3.one*Mathf.Min(1f,Mathf.Max(.1f,(canvasRect.rect.width-48)/900f));
    }
    public void Hide() => content.SetActive(false);
    private void Refresh()
    {
        if(upgrades==null)return;
        count.text=upgrades.AcquiredCount==1?"1 MELHORIA":$"{upgrades.AcquiredCount} MELHORIAS";
        int highestBonus=0;
        for(int i=0;i<5;i++)highestBonus=Mathf.Max(highestBonus,upgrades.Bonus((UpgradeKind)i));
        // Shared visual scale expands for large stacks; the exact values have no cap.
        float scale=100+Mathf.Max(200,Mathf.CeilToInt(highestBonus/100f)*100);
        var green=new Color(.54f,.91f,.35f,.98f);var gained=new Color(.95f,.8f,.32f,.98f);var empty=new Color(.68f,.75f,.7f,.14f);
        for(int row=0;row<5;row++)
        {
            var kind=(UpgradeKind)row;int bonus=upgrades.Bonus(kind);
            values[row].text=$"{UpgradeText.Value(kind,upgrades.Value(kind,-bonus))} → {UpgradeText.Value(kind,upgrades.Value(kind))}  <color=#F2CF61>+{bonus}%</color>";
            float filled=(100+bonus)/scale*20,initial=100/scale*20;
            for(int pip=0;pip<20;pip++)
            {
                float amount=Mathf.Clamp01(filled-pip);
                segments[row*20+pip].color=Color.Lerp(empty,pip<initial?green:gained,amount);
            }
        }
    }
    private void OnDisable()=>Hide();
    private void OnDestroy(){if(upgrades!=null)upgrades.Changed-=Refresh;}
}

public static class UpgradeText
{
    private static readonly CultureInfo Portuguese=CultureInfo.GetCultureInfo("pt-BR");
    public static string Name(UpgradeKind kind)=>kind switch
    {
        UpgradeKind.AttackDamage=>"Dano",UpgradeKind.AttackSpeed=>"Cadência",UpgradeKind.MaximumHealth=>"Vida máxima",
        UpgradeKind.MoveSpeed=>"Movimento",_=>"Especial"
    };
    public static string Value(UpgradeKind kind,float value)=>value.ToString(kind==UpgradeKind.MaximumHealth?"0":"0.##",Portuguese)+(kind==UpgradeKind.AttackSpeed?"/s":"");
}
