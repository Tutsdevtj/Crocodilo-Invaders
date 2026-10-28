using System;
using System.Collections.Generic;
using UnityEngine;

// The existing line owns visibility/path; a textured ribbon supplies the painted bark.
[DefaultExecutionOrder(160)]
public sealed class MacaquiniVineRibbon : MonoBehaviour
{
    [Serializable] public sealed class Join { public string name; public float x,y,dx,dy,width; }
    [Serializable] private sealed class Layout { public Join[] joins; }
    private static Dictionary<string,Join> joins;
    private LineRenderer path;
    private MacaquiniFlightPresentation actor;
    private Mesh mesh;
    private MeshRenderer ribbon;
    private Material material;
    private Vector3[] vertices;
    private Vector2[] uvs;
    private Color32[] colors;
    private int[] triangles;
    private float fixedWidth, fixedTileLength;
    public Color Tint { get; set; } = Color.white;
    public bool UsesPoseJoin => actor != null;
    public bool Visible => ribbon != null && ribbon.enabled;
    public Vector3 JoinPosition { get; private set; }
    public int DrawnSegments { get; private set; }
    public static Join For(Sprite sprite)
    {
        if (joins == null)
        {
            joins = new Dictionary<string, Join>(); var json = Resources.Load<TextAsset>("Art/MacaquiniVineV28/joins");
            if (json != null) foreach (var item in JsonUtility.FromJson<Layout>(json.text).joins) joins[item.name] = item;
        }
        return sprite != null && joins.TryGetValue(sprite.name, out var value) ? value : null;
    }
    public static MacaquiniVineRibbon Attach(LineRenderer line, MacaquiniFlightPresentation source)
    {
        return Create(line,source,0,.4f);
    }
    public static MacaquiniVineRibbon Attach(LineRenderer line, float paintedWidth, float tileLength = .4f)
    {
        return Create(line,null,Mathf.Max(.01f,paintedWidth),Mathf.Max(.01f,tileLength));
    }
    private static MacaquiniVineRibbon Create(LineRenderer line, MacaquiniFlightPresentation source, float width, float tileLength)
    {
        var component = line.gameObject.AddComponent<MacaquiniVineRibbon>(); component.path = line; component.actor = source;
        component.fixedWidth=width;component.fixedTileLength=tileLength;
        line.forceRenderingOff = true;
        var node = new GameObject("PaintedUpperVine"); node.transform.SetParent(line.transform, false);
        var filter = node.AddComponent<MeshFilter>(); component.mesh = new Mesh { name = "MacaquiniUpperVineMesh" }; component.mesh.MarkDynamic(); filter.sharedMesh = component.mesh;
        component.ribbon = node.AddComponent<MeshRenderer>(); component.ribbon.sortingLayerID = line.sortingLayerID; component.ribbon.sortingOrder = line.sortingOrder;
        component.material = new Material(Shader.Find("Sprites/Default")); component.material.mainTexture = Resources.Load<Texture2D>("Art/MacaquiniVineV28/vine_bark");
        component.ribbon.sharedMaterial = component.material; component.ribbon.enabled = false;
        return component;
    }
    private void LateUpdate()
    {
        if (path == null || ribbon == null) return;
        if (!path.enabled || !path.gameObject.activeInHierarchy) { ribbon.enabled = false; return; }
        if (Time.timeScale <= 0 && ribbon.enabled) return;
        int count = path.positionCount;
        if (count < 2) { ribbon.enabled = false; return; }
        if(actor!=null)actor.FillUpperVine(path,actor.VineGripPosition,Point(count-1));
        if (vertices == null || vertices.Length != count * 2)
        {
            vertices = new Vector3[count * 2]; uvs = new Vector2[count * 2]; triangles = new int[(count - 1) * 6];
            colors=new Color32[count*2];for(int i=0;i<colors.Length;i++)colors[i]=new Color32(255,255,255,255);
            for (int i = 0; i < count - 1; i++) { int t = i * 6, v = i * 2; triangles[t] = v; triangles[t+1] = v+2; triangles[t+2] = v+1; triangles[t+3] = v+1; triangles[t+4] = v+2; triangles[t+5] = v+3; }
        }
        float scale = actor == null ? 1 : actor.PaintedScale, width = (actor == null ? fixedWidth : actor.VineJoinWidth) / .56f, distance = 0;
        Vector3 previous = Point(0); JoinPosition = previous;
        for (int i = 0; i < count; i++)
        {
            Vector3 point = Point(i); Vector3 direction = Point(Mathf.Min(count-1,i+1)) - Point(Mathf.Max(0,i-1));
            Vector3 normal = new Vector3(direction.y,-direction.x).normalized;
            if (i == 0) point -= direction.normalized * .035f * scale;
            if (i > 0) distance += Vector3.Distance(previous,point);
            vertices[i*2] = transform.InverseTransformPoint(point - normal*width*.5f);
            vertices[i*2+1] = transform.InverseTransformPoint(point + normal*width*.5f);
            colors[i*2]=colors[i*2+1]=(Color32)Tint;
            float v = distance / (fixedTileLength * scale); uvs[i*2] = new Vector2(0,v); uvs[i*2+1] = new Vector2(1,v); previous = point;
        }
        mesh.Clear(); mesh.vertices = vertices; mesh.uv = uvs; mesh.colors32=colors;mesh.triangles = triangles; mesh.RecalculateBounds();
        ribbon.enabled = true; DrawnSegments = count - 1;
    }
    private Vector3 Point(int i) { Vector3 point = path.GetPosition(i); return path.useWorldSpace ? point : path.transform.TransformPoint(point); }
    private void OnDisable() { if (ribbon != null) ribbon.enabled = false; }
    private void OnDestroy() { if (mesh != null) Destroy(mesh); if (material != null) Destroy(material); }
}
