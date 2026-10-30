using UnityEngine;

[DefaultExecutionOrder(-60)]
public sealed class LakeLilyPads : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    private LakeBackdrop lake;
    private readonly SpriteRenderer[] pads=new SpriteRenderer[6];
    private readonly float[] scales=new float[6];
    private static readonly float[] Fractions={.08f,.24f,.37f,.73f,.88f,.96f};
    private static readonly float[] Widths={1.1f,1.4f,.85f,1.25f,1.5f,.85f};
    private static readonly float[] Depths={-.40f,-.82f,-.28f,-.62f,-.36f,-1.05f};
    public int Count => pads[0]!=null?pads.Length:0;
    public SpriteRenderer Pad(int index) => pads[index];
    // Reuse only the approved small, shallow leaves for creatures at the waterline.
    public Sprite CreatureLeaf(int index) => sprites[new[]{0,2}[Mathf.Abs(index)%2]];
    private void Start()
    {
        lake=GetComponent<LakeBackdrop>();if(lake==null || sprites==null || sprites.Length!=6){enabled=false;return;}
        int[] variants={0,4,2,3,5,0};
        for(int i=0;i<pads.Length;i++)
        {
            pads[i]=new GameObject("DecorativeLilyPad_"+i).AddComponent<SpriteRenderer>();
            pads[i].transform.SetParent(transform,false);pads[i].sprite=sprites[variants[i]];
            pads[i].sortingOrder=-174;
            scales[i]=Widths[i]/pads[i].sprite.bounds.size.x;
            pads[i].transform.localScale=Vector3.one*scales[i];
        }
        PositionPads();
    }
    private float X(float fraction)
    {
        var camera=lake.GameplayCamera;return camera.transform.position.x+(fraction-.5f)*camera.orthographicSize*2*camera.aspect;
    }
    private float Bob(int index)
    {
        float t=lake.MotionClock,phase=index+2;
        return Mathf.Sin(t*(.82f+phase*.047f)+phase*1.7f)*.041f+Mathf.Sin(t*1.73f+phase)*.014f;
    }
    private float Tilt(int index) => Mathf.Sin(lake.MotionClock*(.57f+(index+2)*.03f)+(index+2)*2.2f)*1.8f;
    private float StretchY(int index) => 1+Mathf.Sin(lake.MotionClock*.91f+(index+2)*1.3f)*.035f;
    private Vector3 Position(int index)
    {
        float t=lake.MotionClock;var camera=lake.GameplayCamera;float width=camera.orthographicSize*2*camera.aspect;
        float left=camera.transform.position.x-width*.5f-Widths[index]-.4f,span=width+2*(Widths[index]+.4f);
        float current=t*(.11f+((index+2)%4)*.025f);
        float x=left+Mathf.Repeat(X(Fractions[index])-left-current+Mathf.Sin(t*.47f+index+2)*.14f,span);
        return new Vector3(x,lake.WaterY+Depths[index]+Bob(index),0);
    }
    private void PositionPads()
    {
        if(pads[0]==null)return;
        for(int i=0;i<pads.Length;i++)
        {
            pads[i].transform.position=Position(i);
            pads[i].transform.rotation=Quaternion.Euler(0,0,Tilt(i));
            pads[i].transform.localScale=new Vector3(scales[i],scales[i]*StretchY(i),scales[i]);
        }
    }
#if UNITY_EDITOR
    // Kept only so historical v15-v19 capture tools compile; gameplay no longer uses leaf supports.
    public Vector2 Support(float fraction) => new Vector2(X(fraction),lake.WaterY+.06f);
#endif
    private void LateUpdate(){PositionPads();}
}
