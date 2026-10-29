using TMPro;
using UnityEngine;

// Dedicated lake presentation; combat values still come from Health.
public sealed class LakeBossBar : MonoBehaviour
{
    private RectTransform panel;
    private const float FillWidth = 292f;
    public Sprite FrameSprite { get; private set; }
    private CanvasGroup alpha;
    private UnityEngine.UI.Image fill, lag, flash;
    private RectTransform[] ripples;
    private TMP_Text title;
    private float clock, target, shown, lagUntil, hitAt = -10;
    public bool IsBuilt { get; private set; }
    public float DisplayedHealth => shown;
    public bool IsCritical => target > 0 && target <= .4f;
    public float AnimationClock => clock;

    public void SetIdentity(Sprite sprite, TMP_FontAsset font)
    {
        if (IsBuilt) return;
        foreach (Transform child in transform) child.gameObject.SetActive(false);
        panel = Node("LakeTireHUD", transform, Vector2.zero, new Vector2(476,112));
        alpha = panel.gameObject.AddComponent<CanvasGroup>();
        alpha.blocksRaycasts = alpha.interactable = false;
        FrameSprite = Resources.Load<Sprite>("LakeHudV05/hud-frame");
        var frame = Image("LakeArtwork",panel,new Vector2(0,-12),new Vector2(476,476f/3f),Color.white);
        frame.sprite = FrameSprite;
        lag = Image("DamageWake",panel,new Vector2(335,39),new Vector2(FillWidth,18),new Color(.87f,.88f,.58f));
        fill = Image("LakeHealth",panel,new Vector2(335,39),new Vector2(FillWidth,18),new Color(.09f,.63f,.60f));
        flash = Image("HitFoam",panel,new Vector2(335,39),new Vector2(FillWidth,18),Color.white);
        foreach (var image in new[] {lag,fill,flash}) image.rectTransform.pivot = new Vector2(1,0);
        var waterClip = Node("WaterSurface",panel,new Vector2(43,39),new Vector2(FillWidth,18));
        waterClip.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        ripples = new RectTransform[12];
        for (int i=0;i<ripples.Length;i++)
            ripples[i] = Image("Foam"+i,waterClip,Vector2.zero,new Vector2(13+i%3*4,2),new Color(.63f,.96f,.83f,.7f)).rectTransform;
        title = Label("BossName","SAPO DO PNEU",font,panel,new Vector2(203,69),new Vector2(124,15),9,new Color(.91f,.94f,.72f));
        title.alignment = TextAlignmentOptions.Center;
        IsBuilt = true;
    }
    public void Show(float value)
    {
        clock=shown=0; target=Mathf.Clamp01(value); hitAt=-10; lagUntil=0;
        if (IsBuilt) { lag.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,0); Paint(); }
    }
    public void SetHealth(float value)
    {
        value=Mathf.Clamp01(value);
        if (value<target) { hitAt=clock; lagUntil=clock+.4f; }
        target=value;
    }
    private void Update()
    {
        if (!IsBuilt) return;
        clock+=Time.deltaTime;
        shown=clock<1.1f ? target*(1-Mathf.Pow(1-Mathf.InverseLerp(.35f,1.1f,clock),3)) : target;
        Paint();
    }
    private void Paint()
    {
        alpha.alpha=Mathf.Clamp01(clock/.25f);
        float intro=1-Mathf.Pow(1-Mathf.Clamp01(clock/.4f),3);
        float shake=Mathf.Clamp01(1-(clock-hitAt)/.16f);
        panel.anchoredPosition=new Vector2(50*(1-intro)+Mathf.Sin(clock*170)*2*shake,Mathf.Cos(clock*190)*shake);
        fill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,FillWidth*shown);
        float width=Mathf.Max(lag.rectTransform.sizeDelta.x,FillWidth*shown);
        if (clock>=lagUntil) width=Mathf.MoveTowards(width,FillWidth*shown,220*Time.deltaTime);
        lag.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,width);
        flash.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,FillWidth*shown);
        flash.color=new Color(.85f,1,.91f,.8f*Mathf.Clamp01(1-(clock-hitAt)/.16f));
        float pulse=.5f+.5f*Mathf.Sin(clock*8);
        fill.color=IsCritical ? Color.Lerp(new Color(.07f,.48f,.54f),new Color(.22f,.82f,.73f),pulse) : new Color(.09f,.63f,.60f);
        title.color=IsCritical ? Color.Lerp(new Color(.91f,.94f,.72f),new Color(1,.76f,.33f),pulse) : new Color(.91f,.94f,.72f);
        for (int i=0;i<ripples.Length;i++)
        {
            float x=Mathf.Repeat(i*27-clock*(9+i%3*3),310)-17;
            ripples[i].gameObject.SetActive(x>=FillWidth*(1-shown));
            ripples[i].anchoredPosition=new Vector2(Mathf.Round(x),3+(i%3)*5+Mathf.Round(Mathf.Sin(clock*2+i)));
        }
    }
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
