using System.Collections.Generic;
using UnityEngine;

// Cut a real opening in the UI mesh; the original painted pixels remain untouched.
[RequireComponent(typeof(UnityEngine.UI.Image))]
public sealed class MacaquiniFrameAperture : MonoBehaviour
{
    private Sprite aperture;
    private void Awake()
    {
        var image = GetComponent<UnityEngine.UI.Image>();
        var source = image.sprite;
        if (source == null) return;
        aperture = Sprite.Create(source.texture, source.rect, Vector2.one * .5f, 100, 0, SpriteMeshType.FullRect);
        aperture.name = source.name + "_aperture";
        float width = source.rect.width, height = source.rect.height;
        Vector2 center = new Vector2(441,89), radius = new Vector2(28,26);
        var angles = new List<float>();
        for (int i=0;i<64;i++) angles.Add(i*Mathf.PI*2/64);
        foreach (Vector2 corner in new[]{Vector2.zero,new Vector2(width,0),new Vector2(width,height),new Vector2(0,height)})
            angles.Add(Mathf.Repeat(Mathf.Atan2(corner.y-center.y,corner.x-center.x),Mathf.PI*2));
        angles.Sort();
        var vertices = new Vector2[angles.Count*2]; var triangles = new ushort[angles.Count*6];
        for(int i=0;i<angles.Count;i++)
        {
            Vector2 direction = new Vector2(Mathf.Cos(angles[i]),Mathf.Sin(angles[i]));
            float distance = Mathf.Min((direction.x>=0?width-center.x:center.x)/Mathf.Max(.00001f,Mathf.Abs(direction.x)),
                (direction.y>=0?height-center.y:center.y)/Mathf.Max(.00001f,Mathf.Abs(direction.y)));
            // OverrideGeometry accepts pixel positions in Sprite Rect space, not Sprite.vertices world units.
            vertices[i*2] = center+Vector2.Scale(direction,radius);
            Vector2 outer=center+direction*distance;
            vertices[i*2+1] = new Vector2(Mathf.Clamp(outer.x,0,width),Mathf.Clamp(outer.y,0,height));
            int next = (i+1)%angles.Count;
            int t=i*6;
            triangles[t]=(ushort)(i*2);triangles[t+1]=(ushort)(i*2+1);triangles[t+2]=(ushort)(next*2);
            triangles[t+3]=(ushort)(next*2);triangles[t+4]=(ushort)(i*2+1);triangles[t+5]=(ushort)(next*2+1);
        }
        aperture.OverrideGeometry(vertices,triangles); image.sprite=aperture; image.useSpriteMesh=true;
    }
    private void OnDestroy(){if(aperture!=null)Destroy(aperture);}
}
