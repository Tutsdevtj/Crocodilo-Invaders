using UnityEngine;
using UnityEngine.InputSystem;

// Saved only in the isolated jetpack test scene, never in the campaign scene.
[DefaultExecutionOrder(-100)]
public sealed class MacaquiniJetpackTestControls : MonoBehaviour
{
    private Player player;
    private UnityEngine.UI.Text status;
    private bool protection;
    private void Start()
    {
        CampaignProgress.UseDirectory(System.IO.Path.Combine(Application.persistentDataPath,"JetpackTest_v10"));
        FindAnyObjectByType<WaveController>().StartBossForDevelopment();
        LeaderboardSession.Cancel();
        player=FindAnyObjectByType<Player>();player.Health.SetMaximumHealth(240,true);
        var node=new GameObject("JetpackTestHelp");var canvas=node.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=200;
        var scaler=node.AddComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);
        var text=new GameObject("Help",typeof(RectTransform));text.transform.SetParent(node.transform,false);status=text.AddComponent<UnityEngine.UI.Text>();
        status.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");status.fontSize=15;status.color=new Color(.9f,1,.8f);status.alignment=TextAnchor.LowerLeft;
        var rect=status.rectTransform;rect.anchorMin=new Vector2(0,1);rect.anchorMax=Vector2.one;rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(18,-10);rect.sizeDelta=new Vector2(-36,65);
    }
    private void Update()
    {
        var combat=FindAnyObjectByType<MacaquiniCombat>();var keys=Keyboard.current;
        if(combat==null||keys==null)return;var boss=combat.GetComponent<Boss>();
        if(keys.f1Key.wasPressedThisFrame)combat.ForceAttack(combat.PhaseTwo?MacaquiniCombat.Attack.BananaVolley:MacaquiniCombat.Attack.JetStrafe);
        if(keys.f2Key.wasPressedThisFrame)combat.ForceAttack(combat.PhaseTwo?MacaquiniCombat.Attack.BananaFan:MacaquiniCombat.Attack.JetDash);
        if(keys.f3Key.wasPressedThisFrame)combat.ForceAttack(combat.PhaseTwo?MacaquiniCombat.Attack.Vines:MacaquiniCombat.Attack.ExhaustGates);
        if(keys.f4Key.wasPressedThisFrame)combat.ForceAttack(MacaquiniCombat.Attack.Pendulum);
        if(keys.f5Key.wasPressedThisFrame)combat.ForceAttack(MacaquiniCombat.Attack.SwingKick);
        if(keys.jKey.wasPressedThisFrame)boss.Health.TakeDamage(45);
        if(keys.kKey.wasPressedThisFrame)boss.Health.TakeDamage(boss.Health.CurrentHealth);
        if(keys.iKey.wasPressedThisFrame){protection=!protection;player.Health.SetInvulnerable(this,protection);}
        if(keys.hKey.wasPressedThisFrame)player.Health.Heal(240);
        if(keys.rKey.wasPressedThisFrame){Time.timeScale=1;AudioListener.pause=false;UnityEngine.SceneManagement.SceneManager.LoadScene(gameObject.scene.name);}
        string attacks=combat.PhaseTwo?"F1 Rajada / F2 Leque / F3 Cipós / F4 Balanço / F5 Chute":"F1 Rajada / F2 Rasante / F3 Descarga";
        status.text="TESTE MACAQUINI · "+(combat.PhaseTwo?"CIPÓ":"JETPACK")+" · "+boss.Health.CurrentHealth+" HP\n"+attacks+"  |  J: −45 HP  K: esgotar forma  I: proteção "+(protection?"ON":"OFF")+"  H: curar  R: reiniciar\nMover e atirar: controles normais do jogo · ESC: pausa";
    }
}
