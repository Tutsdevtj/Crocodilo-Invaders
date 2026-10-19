using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>Native, repeatable repair. Crops reference the original textures; no PNG is redrawn.</summary>
public static class BaseImprovisadaMapRepairTools
{
    private const float PPU = 80f;
    private const string Slices = "Assets/Art/Environment/BaseImprovisada/TerrainSlices/";
    private static readonly Dictionary<string, Tile> tiles = new Dictionary<string, Tile>();
    private struct Tile { public Sprite sprite; public int width, top; }
    private static readonly (float left, float right)[] Paved =
        { (-5.8f, .2f), (5.3f, 14.5f), (19f, 42.25f), (59f, 70.5f), (72.5f, 82.6f), (84f, 99.2f) };

    [MenuItem("Crocodilo/Base/Repair terrain, scenery and Croc scale")]
    public static void Repair()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene(BaseImprovisadaMapTools.ScenePath, OpenSceneMode.Single);
        BuildTerrain(GameObject.Find("Collisions").transform);
        ArrangeScenery();
        ResizeCroc();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Validate();
        HubCharacterTools.Validate();
        BaseImprovisadaMapTools.Validate();
        CrocodiloWindowsBuild.ValidateProject();
    }

    public static void RepairAndValidateInPlayMode()
    {
        Repair();
        BaseImprovisadaMapPlayValidation.Run();
    }

    public static void BuildTerrain(Transform collisions)
    {
        PrepareTiles();
        var environment = GameObject.Find("Environment").transform;
        var old = environment.Find("Terrain");
        if (old != null) Object.DestroyImmediate(old.gameObject);
        var terrain = Child("Terrain", environment);
        var body = Child("Subsoil", terrain);
        var surface = Child("Surface", terrain);
        // All the way up to the actual walk line: no transparent band between art and physics.
        var ground = environment.Find("Ground");
        ground.position = new Vector3(BaseImprovisadaMapTools.VisualLeft, -6f, 0f);
        ground.localScale = new Vector3(12000f, 600f, 1f);
        var backing = ground.GetComponent<SpriteRenderer>();
        backing.color = new Color(.13f, .10f, .16f, 1f);
        backing.sortingLayerName = "WorldGround"; backing.sortingOrder = -10;
        // Opaque, textured core, with a consistent depth rather than randomly staggered blocks.
        AddTiled("SoilFace", body, tiles["Core"].sprite, BaseImprovisadaMapTools.VisualLeft, 0f, 120f, .65f, 3, new Color(.78f, .72f, .82f));
        AddTiled("SoilDepth", body, tiles["Core"].sprite, BaseImprovisadaMapTools.VisualLeft, -.65f, 120f, .65f, 2, new Color(.54f, .48f, .60f));
        AddTiled("SoilDeep", body, tiles["Core"].sprite, BaseImprovisadaMapTools.VisualLeft, -1.3f, 120f, 4.7f, 1, new Color(.32f, .28f, .38f));
        var floor = collisions.Find("Floor");
        floor.position = new Vector3(46.5f, -2f, 0f);
        floor.GetComponent<BoxCollider2D>().size = new Vector2(120f, 4f);

        var edges = new SortedSet<int> { Pixel(BaseImprovisadaMapTools.VisualLeft), Pixel(BaseImprovisadaMapTools.VisualRight) };
        foreach (var span in Paved) { edges.Add(Pixel(span.left)); edges.Add(Pixel(span.right)); }
        int[] boundaries = edges.ToArray();
        int[] grass = { 1, 2, 5, 3, 6, 1, 5, 2, 3, 8 };
        int[] concrete = { 1, 3, 4, 1, 5, 6, 3 };
        int sequence = 0;
        for (int s = 0; s < boundaries.Length - 1; s++)
        {
            int cursor = boundaries[s], end = boundaries[s + 1];
            bool paved = Paved.Any(p => (cursor + end) * .5f / PPU >= p.left && (cursor + end) * .5f / PPU < p.right);
            while (cursor < end)
            {
                int id = paved ? concrete[sequence % concrete.Length] : grass[sequence % grass.Length];
                Tile tile = tiles[(paved ? "Concrete" : "Grass") + id];
                int width = Mathf.Min(tile.width, end - cursor);
                AddTiled($"{(paved ? "Pavement" : "Grass")}_{sequence:000}", surface, tile.sprite,
                    cursor / PPU, tile.top / PPU, width / PPU, tile.sprite.rect.height / PPU, 20, Color.white);
                // Two source pixels of overlap, integer positions, and a clipped final piece.
                cursor += width == end - cursor ? width : width - 2;
                sequence++;
            }
        }
        var tufts = Child("GrassTufts", terrain);
        foreach (float x in new[] { -9f, 2f, 3.8f, 16.9f, 18f, 43.9f, 46.2f, 55.9f, 58.1f, 71.5f, 83.2f, 101f })
        {
            if (Paved.Any(p => x >= p.left && x < p.right)) continue;
            BaseImprovisadaMapTools.Place("Terrain/03_grama/grama_solta_03", tufts, x, 0f, "WorldBack", 1, .5f);
        }
    }

    private static void PrepareTiles()
    {
        tiles.Clear();
        Directory.CreateDirectory(Slices);
        AssetDatabase.Refresh();
        foreach (int id in new[] { 1, 2, 3, 5, 6, 8 }) PrepareSurface("Grass" + id, $"01_solo/solo_grama_{id:00}", 5);
        foreach (int id in new[] { 1, 3, 4, 5, 6 }) PrepareSurface("Concrete" + id, $"04_pisos_concreto_metal/piso_superior_{id:00}", 3);
        string path = BaseImprovisadaMapTools.Art + "Terrain/01_solo/solo_pedra_01.png";
        var raw = Read(path);
        // Largest entirely opaque rectangle: guarantees a genuinely solid textured foundation.
        var pixels = raw.GetPixels32();
        int[] heights = new int[raw.width];
        RectInt best = default;
        for (int y = 0; y < raw.height; y++)
        {
            for (int x = 0; x < raw.width; x++) heights[x] = pixels[y * raw.width + x].a >= 250 ? heights[x] + 1 : 0;
            var stack = new Stack<int>();
            for (int x = 0; x <= raw.width; x++)
            {
                int h = x == raw.width ? 0 : heights[x];
                while (stack.Count > 0 && heights[stack.Peek()] > h)
                {
                    int height = heights[stack.Pop()], left = stack.Count == 0 ? 0 : stack.Peek() + 1;
                    if ((x - left) * height > best.width * best.height) best = new RectInt(left, y - height + 1, x - left, height);
                }
                if (x < raw.width) stack.Push(x);
            }
        }
        Require(best.width >= 40 && best.height >= 20, "Solid stone texture core available");
        tiles["Core"] = new Tile { sprite = Slice("Core", path, best), width = best.width };
        Object.DestroyImmediate(raw);
        AssetDatabase.SaveAssets();
    }

    private static void PrepareSurface(string name, string relative, int inset)
    {
        string path = BaseImprovisadaMapTools.Art + "Terrain/" + relative + ".png";
        var raw = Read(path);
        RectInt box = BaseImprovisadaMapTools.OpaqueBounds(raw);
        box.xMin += inset; box.xMax -= inset;
        var pixels = raw.GetPixels32();
        int line = -1;
        for (int y = box.yMax - 1; y >= box.yMin + 2; y--)
        {
            bool solid = true;
            for (int x = box.xMin; x < box.xMax && solid; x++)
            for (int dy = 0; dy < 3; dy++) if (pixels[(y - dy) * raw.width + x].a < 250) { solid = false; break; }
            if (solid) { line = box.yMax - 1 - y; break; }
        }
        if (line < 0)
        {
            // A few concrete fragments have broken corners. Keep their widest sound span.
            int bestWidth = 0, bestLeft = 0, bestY = 0;
            for (int y = box.yMax - 1; y >= box.yMin + 2; y--)
            {
                int start = box.xMin;
                for (int x = box.xMin; x <= box.xMax; x++)
                {
                    bool solid = x < box.xMax;
                    if (solid) for (int dy = 0; dy < 3; dy++) if (pixels[(y - dy) * raw.width + x].a < 250) { solid = false; break; }
                    if (solid) continue;
                    if (x - start > bestWidth) { bestWidth = x - start; bestLeft = start; bestY = y; }
                    start = x + 1;
                }
            }
            Require(bestWidth >= box.width * .7f, "Usable concrete span " + name);
            box.xMin = bestLeft; box.width = bestWidth;
            line = box.yMax - 1 - bestY;
        }
        Require(line >= 0 && box.width > 4, "Continuous three-pixel walk band: " + name);
        tiles[name] = new Tile { sprite = Slice(name, path, box), width = box.width, top = line };
        Object.DestroyImmediate(raw);
    }

    private static Sprite Slice(string name, string source, RectInt rect)
    {
        string path = Slices + name + ".asset";
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null) return sprite;
        sprite = Sprite.Create(AssetDatabase.LoadAssetAtPath<Texture2D>(source), new Rect(rect.x, rect.y, rect.width, rect.height), new Vector2(0f, 1f), PPU, 0, SpriteMeshType.FullRect);
        sprite.name = name;
        AssetDatabase.CreateAsset(sprite, path);
        return sprite;
    }

    private static SpriteRenderer AddTiled(string name, Transform parent, Sprite sprite, float x, float y, float width, float height, int order, Color color)
    {
        var node = Child(name, parent);
        node.position = new Vector3(x, y, 0f);
        var renderer = node.gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = new Vector2(width, height);
        renderer.sortingLayerName = "WorldGround"; renderer.sortingOrder = order; renderer.color = color;
        return renderer;
    }

    public static void ArrangeScenery()
    {
        var environment = GameObject.Find("Environment").transform;
        // Front-of-building supplies form small groups at the sides, with readable entries.
        Move(environment, "Entrance/04_barril_azul", -2.4f);
        Move(environment, "Entrance/01_caixas_militares_empilhadas", 2.4f);
        Move(environment, "Entrance/03_bau_militar", 3.7f);
        Move(environment, "Entrance/barreira_baixa_01", 4.75f);
        Move(environment, "Entrance/09_sacos_areia", 5.7f);
        Move(environment, "Infirmary/02_caixotes_madeira", 6.1f);
        Move(environment, "Infirmary/09_sacos_areia", 14.2f);
        Move(environment, "Infirmary/06_caixa_ferramentas", 15.45f);
        Move(environment, "Infirmary/05_barril_ferrugem", 17.6f);
        Move(environment, "Hangar/08_caminhao_carga", 20.9f);
        Move(environment, "Hangar/08_palete_suprimentos", 23.1f);
        Move(environment, "Hangar/07_prateleira_suprimentos", 24.3f);
        Move(environment, "Hangar/02_caixotes_madeira", 25.6f);
        Move(environment, "Hangar/06_caixa_ferramentas", 34.4f);
        Move(environment, "Hangar/04_barril_azul", 41.5f);
        Move(environment, "Camp/01_arvore_de_refugio", 44.5f);
        Move(environment, "Camp/03_bau_militar", 46.3f);
        Move(environment, "Camp/05_barril_ferrugem", 54.2f);
        Move(environment, "Camp/04_barril_azul", 55.1f);
        Move(environment, "ExpansionArea/Gate/barreira_alta_02", BaseImprovisadaMapTools.ExpansionGateX, 0f);
        foreach (var prefab in environment.GetComponentsInChildren<Transform>(true).Where(t => PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject)))
            if (prefab.name.StartsWith("Hub")) prefab.position = new Vector3(prefab.position.x, -.0125f, prefab.position.z);
        // Fence fragments used to cut across checkpoint/medical entrances at order 30.
        foreach (var renderer in environment.GetComponentsInChildren<SpriteRenderer>(true))
            if (AssetDatabase.GetAssetPath(renderer.sprite).Contains("08_cercas_grades_arames")) renderer.sortingOrder = -10;
        // Organic silhouettes only. Dark rectangular supply props read as missing floor.
        var front = GameObject.Find("Foreground");
        if (front != null)
            foreach (var renderer in front.GetComponentsInChildren<SpriteRenderer>(true))
            {
                bool vegetation = renderer.sprite.name.Contains("grama");
                bool rock = renderer.sprite.name.Contains("rocha");
                renderer.gameObject.SetActive(vegetation || rock);
                renderer.transform.position = new Vector3(renderer.transform.position.x, -.0125f, 0f);
                renderer.transform.localScale = Vector3.one * (vegetation ? .4f : .3f);
                renderer.color = new Color(.38f, .35f, .42f, 1f);
            }
    }

    private static void Move(Transform root, string path, float x, float y = 0f)
    {
        Transform item = root.Find(path);
        if (item == null) throw new InvalidOperationException("Missing scenery item " + path);
        item.position = new Vector3(x, y, item.position.z);
    }

    private static void ResizeCroc()
    {
        var root = PrefabUtility.LoadPrefabContents(HubCharacterTools.Prefab);
        root.transform.Find("VisualRoot").localScale = new Vector3(2f, 2f, 1f);
        root.transform.Find("CameraTarget").localPosition = new Vector3(0f, .85f, 0f);
        var collider = root.GetComponent<CapsuleCollider2D>();
        collider.size = new Vector2(.88f, 1.7f);
        collider.offset = new Vector2(0f, .85f);
        PrefabUtility.SaveAsPrefabAsset(root, HubCharacterTools.Prefab);
        PrefabUtility.UnloadPrefabContents(root);
    }

    [MenuItem("Crocodilo/Base/Validate repaired ground coverage")]
    public static void Validate()
    {
        var terrain = GameObject.Find("Environment/Terrain").transform;
        var pieces = terrain.Find("Surface").GetComponentsInChildren<SpriteRenderer>().OrderBy(r => r.bounds.min.x).ToArray();
        float covered = BaseImprovisadaMapTools.VisualLeft;
        foreach (var tile in pieces)
        {
            Require(tile.bounds.min.x <= covered + .0001f && tile.bounds.max.x > covered, "Surface interval coverage " + tile.name);
            Require(Mathf.Abs(tile.transform.position.x * PPU - Mathf.Round(tile.transform.position.x * PPU)) < .001f
                && Mathf.Abs(tile.transform.position.y * PPU - Mathf.Round(tile.transform.position.y * PPU)) < .001f, "Pixel grid " + tile.name);
            covered = tile.bounds.max.x;
        }
        Require(covered >= BaseImprovisadaMapTools.VisualRight, "Entire 120-unit visual route covered");
        AuditPixels(terrain, false);
        AuditPixels(terrain, true);
        Debug.Log("REPAIRED MAP VALIDATION PASSED: interval/grid checks and rendered alpha across all 9600 ground pixels.");
    }

    private static void AuditPixels(Transform terrain, bool surfaceOnly)
    {
        var scene = EditorSceneManager.NewPreviewScene();
        RenderTexture previous = RenderTexture.active;
        RenderTexture texture = null; Texture2D pixels = null;
        try
        {
            var clone = Object.Instantiate(terrain.gameObject);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(clone, scene);
            if (surfaceOnly) foreach (Transform child in clone.transform) if (child.name != "Surface") child.gameObject.SetActive(false);
            var camera = new GameObject("Ground coverage camera").AddComponent<Camera>();
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(camera.gameObject, scene);
            camera.scene = scene;
            camera.orthographic = true; camera.orthographicSize = 1.5f;
            camera.transform.position = new Vector3(46.5f, -1.5f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.clear;
            camera.allowMSAA = false;
            texture = new RenderTexture(9600, 240, 0, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
            camera.targetTexture = texture; camera.aspect = 40f; camera.Render();
            RenderTexture.active = texture;
            pixels = new Texture2D(9600, 240, TextureFormat.RGBA32, false);
            pixels.ReadPixels(new Rect(0, 0, 9600, 240), 0, 0); pixels.Apply();
            Color32[] data = pixels.GetPixels32();
            int start = surfaceOnly ? 237 : 0;
            int gaps = 0;
            for (int y = start; y < 240; y++) for (int x = 0; x < 9600; x++) if (data[y * 9600 + x].a < 240) gaps++;
            Directory.CreateDirectory("Logs/MapRepair");
            File.WriteAllBytes("Logs/MapRepair/" + (surfaceOnly ? "walk_surface_alpha.png" : "soil_alpha.png"), pixels.EncodeToPNG());
            Require(gaps == 0, $"{(surfaceOnly ? "Walk surface" : "Textured soil")}: {gaps} transparent pixels (no solid backdrop in this test)");
        }
        finally
        {
            RenderTexture.active = previous;
            if (texture != null) { texture.Release(); Object.DestroyImmediate(texture); }
            if (pixels != null) Object.DestroyImmediate(pixels);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }

    private static Texture2D Read(string path) { var t = new Texture2D(2, 2, TextureFormat.RGBA32, false); t.LoadImage(File.ReadAllBytes(path)); return t; }
    private static Transform Child(string name, Transform parent) { var t = new GameObject(name).transform; t.SetParent(parent, false); return t; }
    private static int Pixel(float value) => Mathf.RoundToInt(value * PPU);
    private static void Require(bool ok, string message) { if (!ok) throw new InvalidOperationException("MAP REPAIR CHECK FAILED: " + message); }
}
