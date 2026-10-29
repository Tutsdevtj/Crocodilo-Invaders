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
    private UnityEngine.UI.Image blinkEyes;
    private LakeFrogExpressions expression;
    private Sprite eyePatch;
    public bool IsBlinking => blinkEyes != null && blinkEyes.enabled;
    private float clock, target, shown, hitAt = -10;
    public bool IsBuilt { get; private set; }
    public float DisplayedHealth => shown;
    public bool IsCritical => target > 0 && target <= .4f;
    public float AnimationClock => clock;

    public void SetIdentity(Sprite sprite, TMP_FontAsset font, LakeFrogExpressions expressions = null)
    {
        expression=expressions;
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
        water=Node("WaterSurface",panel,new Vector2(43,39),new Vector2(FillWidth,18)).gameObject.AddComponent<LakeHudWater>();
        spill=Node("SpilledLife",panel,Vector2.zero,new Vector2(476,112)).gameObject.AddComponent<LakeHudSpill>();
        title = Label("BossName","SAPO DO PNEU",font,panel,new Vector2(203,69),new Vector2(124,15),9,new Color(.91f,.94f,.72f));
        title.alignment = TextAlignmentOptions.Center;
        IsBuilt = true;
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
