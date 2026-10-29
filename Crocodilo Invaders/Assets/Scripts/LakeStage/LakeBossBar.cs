using TMPro;
using UnityEngine;

// Dedicated lake presentation; combat values still come from Health.
public sealed class LakeBossBar : MonoBehaviour
{
    private RectTransform panel;
    private const float FillWidth = 292f;
    public Sprite FrameSprite { get; private set; }
    private CanvasGroup alpha;
    private LakeHudWater water;
    private LakeHudSpill spill;
    public LakeHudWater Water => water;
    public int Spills => spill != null ? spill.Bursts : 0;
    public float SpilledVolume => spill != null ? spill.LostVolume : 0;
    public bool IsPouring => spill != null && spill.IsPouring;
    private TMP_Text title;
    private TMP_Text vulnerableLabel;
    private SapoDoPneuCombat combat;
    public TMP_Text Title => title;
    private UnityEngine.UI.Image blinkEyes;
    private LakeFrogExpressions expression;
    private Sprite eyePatch;
    private UnityEngine.UI.Image companionIcon;
    private SapererecaAssets companionArt;
    private TMP_Text companionName;
    private bool companionJumping,companionGone;
    private float companionJumpAt;
    private Vector2 companionStart,companionLanding,companionStartSize,companionEndSize;
    public bool CompanionInHud => companionIcon!=null && companionIcon.enabled && !companionJumping && !companionGone;
    public bool CompanionJumping => companionJumping;
    public UnityEngine.UI.Image CompanionIcon => companionIcon;
    public bool IsBlinking => blinkEyes != null && blinkEyes.enabled;
    private float clock, target, shown, hitAt = -10;
    public bool IsBuilt { get; private set; }
    public float DisplayedHealth => shown;
    public bool IsCritical => target > 0 && target <= .5f;
    public float AnimationClock => clock;

    public void SetIdentity(Sprite sprite, TMP_FontAsset font, LakeFrogExpressions expressions = null)
    {
        expression=expressions;
        combat=expressions!=null?expressions.GetComponent<SapoDoPneuCombat>():FindAnyObjectByType<SapoDoPneuCombat>();
        if (IsBuilt) return;
        foreach (Transform child in transform) child.gameObject.SetActive(false);
        panel = Node("LakeTireHUD", transform, Vector2.zero, new Vector2(476,112));
        alpha = panel.gameObject.AddComponent<CanvasGroup>();
        alpha.blocksRaycasts = alpha.interactable = false;
        FrameSprite = Resources.Load<Sprite>("LakeHudV05/hud-frame");
        var frame = Image("LakeArtwork",panel,new Vector2(0,-12),new Vector2(476,476f/3f),Color.white);
        frame.sprite = FrameSprite;
        var closed = Resources.Load<Sprite>("LakeExpressionsV06/hud-closed");
        if (closed != null)
        {
            eyePatch=LakeFrogExpressions.EyePatch(closed,new Rect(1675f/2172f,295f/724f,285f/2172f,93f/724f));
            blinkEyes=Image("PortraitBlink",panel,new Vector2(476f*1675f/2172f,-12f+(476f/3f)*336f/724f),
                new Vector2(476f*285f/2172f,(476f/3f)*93f/724f),Color.white);
            blinkEyes.sprite=eyePatch; blinkEyes.enabled=false;
        }
        Image("EmptyWaterChannel",panel,new Vector2(43,39),new Vector2(FillWidth,18),new Color(.025f,.1f,.11f));
        water=Node("WaterSurface",panel,new Vector2(43,39),new Vector2(FillWidth,18)).gameObject.AddComponent<LakeHudWater>();
        spill=Node("SpilledLife",panel,Vector2.zero,new Vector2(476,112)).gameObject.AddComponent<LakeHudSpill>();
        Image("WideNameMoss",panel,new Vector2(103,65),new Vector2(236,28),new Color(.24f,.34f,.13f));
        Image("WideNameWood",panel,new Vector2(106,68),new Vector2(230,22),new Color(.1f,.12f,.075f));
        title = Label("BossName","SAPENEU CURURU",font,panel,new Vector2(110,69),new Vector2(222,20),11,new Color(.91f,.94f,.72f));
        title.alignment = TextAlignmentOptions.Center;
        title.textWrappingMode=TextWrappingModes.NoWrap;title.overflowMode=TextOverflowModes.Overflow;
        vulnerableLabel=Label("BurpVulnerability","ARROTO · DANO x2",font,panel,new Vector2(86,110),new Vector2(270,22),11,new Color(1,.94f,.48f));
        vulnerableLabel.alignment=TextAlignmentOptions.Center;vulnerableLabel.enabled=false;
        companionArt=Resources.Load<SapererecaAssets>("SapererecaV12/art");
        if(companionArt!=null)
        {
            companionIcon=Image("SapererecaHud",panel,new Vector2(386,110),new Vector2(44,39),Color.white);
            companionIcon.sprite=companionArt.frames[0];companionIcon.preserveAspect=true;
            companionName=Label("CompanionEntranceName","SAPERERECA\nA MULHER DO SAPO",font,panel,new Vector2(30,160),new Vector2(290,35),9,new Color(1,.87f,.68f));
            companionName.alignment=TextAlignmentOptions.Center;companionName.enabled=false;
        }
        IsBuilt = true;
    }
    public void BeginCompanionJump(Vector2 landing)
    {
        if(companionIcon==null || companionGone || companionJumping)return;
        Canvas.ForceUpdateCanvases();var camera=Camera.main;var canvas=panel.GetComponentInParent<Canvas>();
        var screen=camera.WorldToScreenPoint(landing);var uiCamera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(panel,screen,uiCamera,out companionLanding);
        companionStart=companionIcon.rectTransform.anchoredPosition;companionStartSize=companionIcon.rectTransform.sizeDelta;
        float scale=camera.pixelHeight/(camera.orthographicSize*2)/canvas.scaleFactor;
        companionEndSize=companionArt.frames[2].bounds.size*scale*SapererecaCompanion.WorldScale;
        companionLanding-=new Vector2(companionEndSize.x*.5f,companionEndSize.y*.03f);
        companionJumpAt=clock;companionJumping=true;companionName.enabled=true;
        companionIcon.rectTransform.SetAsLastSibling();
    }
    public void HideCompanion()
    {
        companionJumping=false;companionGone=true;if(companionIcon!=null)companionIcon.enabled=false;if(companionName!=null)companionName.enabled=false;
    }
    public void Show(float value)
    {
        clock=shown=0; target=Mathf.Clamp01(value); hitAt=-10;
        if (IsBuilt) { spill.ResetWater(); Paint(); }
    }
    public void SetHealth(float value)
    {
        value=Mathf.Clamp01(value);
        if (value<target) { hitAt=clock; if(spill!=null)spill.Release(target-value,clock,shown); }
        target=value;
    }
    private void Update()
    {
        if (!IsBuilt) return;
        clock+=Time.deltaTime;
        shown=clock<1.1f ? target*(1-Mathf.Pow(1-Mathf.InverseLerp(.35f,1.1f,clock),3)) : Mathf.MoveTowards(shown,target,Time.deltaTime*.9f);
        Paint();
        if(companionIcon!=null)
        {
            if(companionJumping)
            {
                float t=Mathf.Clamp01((clock-companionJumpAt)/SapererecaCompanion.EntranceDuration);
                companionIcon.sprite=companionArt.frames[t<.12f?1:2];
                companionIcon.rectTransform.anchoredPosition=Vector2.Lerp(companionStart,companionLanding,t)+Vector2.up*(Mathf.Sin(t*Mathf.PI)*170);
                companionIcon.rectTransform.sizeDelta=Vector2.Lerp(companionStartSize,companionEndSize,t);
                if(t>=1){companionJumping=false;companionGone=true;companionIcon.enabled=false;}
            }
            else if(!companionGone)companionIcon.sprite=companionArt.frames[Mathf.Repeat(clock,3.7f)<.13f?5:0];
            if(companionName!=null && companionGone && clock-companionJumpAt>3.2f)companionName.enabled=false;
        }
    }
    private void Paint()
    {
        alpha.alpha=Mathf.Clamp01(clock/.25f);
        float intro=1-Mathf.Pow(1-Mathf.Clamp01(clock/.4f),3);
        float shake=Mathf.Clamp01(1-(clock-hitAt)/.16f);
        panel.anchoredPosition=new Vector2(50*(1-intro)+Mathf.Sin(clock*170)*2*shake,Mathf.Cos(clock*190)*shake);
        water.Present(clock,shown,Mathf.Clamp01(1-(clock-hitAt)/.65f));
        spill.Present(clock,shown);
        float pulse=.5f+.5f*Mathf.Sin(clock*8);
        title.color=IsCritical ? Color.Lerp(new Color(.91f,.94f,.72f),new Color(1,.76f,.33f),pulse) : new Color(.91f,.94f,.72f);
        vulnerableLabel.enabled=combat!=null && combat.IsVulnerable;
    }
    private void LateUpdate()
    {
        if (blinkEyes != null) blinkEyes.enabled=expression != null && expression.IsBlinking;
    }
    private void OnDisable() { if(blinkEyes!=null)blinkEyes.enabled=false; }
    private void OnDestroy() { if(eyePatch!=null)Destroy(eyePatch); }
    private static RectTransform Node(string name,Transform parent,Vector2 at,Vector2 size)
    {
        var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent,false); rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.zero;
        rect.anchoredPosition=at; rect.sizeDelta=size; return rect;
    }
    private static UnityEngine.UI.Image Image(string name,Transform parent,Vector2 at,Vector2 size,Color color)
    {
        var image=Node(name,parent,at,size).gameObject.AddComponent<UnityEngine.UI.Image>();
        image.color=color; image.raycastTarget=false; return image;
    }
    private static TMP_Text Label(string name,string text,TMP_FontAsset font,Transform parent,Vector2 at,Vector2 size,float height,Color color)
    {
        var label=Node(name,parent,at,size).gameObject.AddComponent<TextMeshProUGUI>();
        label.text=text; label.font=font; label.fontSize=height; label.color=color;
        label.enableAutoSizing=true; label.fontSizeMin=height-2; label.fontSizeMax=height;
        label.alignment=TextAlignmentOptions.MidlineLeft; label.raycastTarget=false;
        return label;
    }
}
