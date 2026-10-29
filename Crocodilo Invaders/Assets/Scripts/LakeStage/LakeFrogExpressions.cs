using UnityEngine;

// Only the eye region is overlaid; the approved body and tire never change cels.
[DefaultExecutionOrder(50)]
public sealed class LakeFrogExpressions : MonoBehaviour
{
    private SapoDoPneuCombat combat;
    private SpriteRenderer visual, eyelids;
    private Sprite patch;
    private float clock, nextBlink = 2.8f, closedUntil;
    private bool doubleBlink;
    public bool IsBlinking { get; private set; }
    public int Blinks { get; private set; }
    public float AnimationClock => clock;
    public bool HasEyelids => eyelids != null && eyelids.sprite != null;

    private void Start()
    {
        combat = GetComponent<SapoDoPneuCombat>();
        visual = GetComponentInChildren<SpriteRenderer>();
        var source = Resources.Load<Sprite>("LakeExpressionsV06/sapo-closed");
        if (visual == null || source == null) { enabled = false; return; }
        patch = EyePatch(source, new Rect(448f/1536f,164f/1024f,500f/1536f,152f/1024f),visual.sprite.pixelsPerUnit);
        eyelids = new GameObject("BlinkEyelids").AddComponent<SpriteRenderer>();
        eyelids.transform.SetParent(visual.transform,false);
        Vector2 center = new Vector2(698,784);
        eyelids.transform.localPosition = (center-visual.sprite.pivot)/visual.sprite.pixelsPerUnit;
        eyelids.sprite = patch; eyelids.sortingLayerID = visual.sortingLayerID;
        eyelids.sortingOrder = visual.sortingOrder+1; eyelids.enabled = false;
    }
    private void Update()
    {
        if (combat == null || !combat.Active) { IsBlinking=false; return; }
        clock += Time.deltaTime;
        if(combat.IsVulnerable || (combat.Tongue!=null && combat.Tongue.MouthOpen)) { IsBlinking=false;return; }
        if (clock >= nextBlink)
        {
            IsBlinking=true; closedUntil=clock+.13f; Blinks++;
            if (doubleBlink) { nextBlink=clock+3.6f; doubleBlink=false; }
            else if (Blinks%3==0) { nextBlink=clock+.32f; doubleBlink=true; }
            else nextBlink=clock+(Blinks%2==0 ? 4.1f : 3.25f);
        }
        else if (clock >= closedUntil) IsBlinking=false;
    }
    private void LateUpdate()
    {
        if (eyelids == null) return;
        eyelids.enabled=IsBlinking;
        eyelids.color=visual.color;
    }
    public static Sprite EyePatch(Sprite source,Rect normalizedTopLeft,float pixelsPerUnit=100)
    {
        var rect=source.rect;
        var crop=new Rect(rect.x+normalizedTopLeft.x*rect.width,
            rect.y+(1-normalizedTopLeft.y-normalizedTopLeft.height)*rect.height,
            normalizedTopLeft.width*rect.width,normalizedTopLeft.height*rect.height);
        var result=Sprite.Create(source.texture,crop,Vector2.one*.5f,pixelsPerUnit,0,SpriteMeshType.FullRect);
        result.name="ClosedEyePatch"; return result;
    }
    private void OnDisable() { IsBlinking=false; if(eyelids!=null)eyelids.enabled=false; }
    private void OnDestroy() { if(patch!=null)Destroy(patch); }
}
