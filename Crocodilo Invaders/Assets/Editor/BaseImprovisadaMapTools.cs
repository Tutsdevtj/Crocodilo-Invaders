using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

// Mapa da BaseImprovisada a partir de BaseImprovisada_Kit_Implementacao_v01 (SourceArt/BaseImprovisada).
// Install é mutação idempotente: importação, cópias reduzidas dos suprimentos, halos, prefabs e cena.
// Preserva câmera, SceneSystems e os objetos de colisão existentes (só reposiciona/redimensiona).
// Preserva o personagem/câmera HUB; sem eles, cria somente uma referência EditorOnly.
public static class BaseImprovisadaMapTools
{
    public const string ScenePath = "Assets/Scenes/BaseImprovisada.unity";
    public const string Art = "Assets/Art/Environment/BaseImprovisada/";
    public const string PrefabDir = "Assets/Prefabs/Environment/Base/";
    private const string Originals = "SourceArt/BaseImprovisada/Originals/02_suprimentos/";
    private const string HaloTexture = Art + "Lighting/base_halo_steps.png";
    private const string HaloMaterial = "Assets/Materials/BaseLightHalo.mat";
    // 80 px do blueprint = 1 unidade: 1280 px de vista = 16 unidades na câmera atual (tamanho 4,5).
    public const float DesignPPU = 80f;
    public const float DesignOrigin = 6f;
    public const float CameraY = 2.4f;
    public static readonly string[] SortingLayers =
    {
        "BackgroundSky", "BackgroundClouds", "BackgroundMountains", "BackgroundRuins", "BackgroundNear",
        "WorldBack", "WorldGround", "WorldCharacters", "WorldFront", "AmbientEffects",
    };
    public static readonly string[] Generated = { "Background", "Props", "Lighting", "NPCAnchors", "Foreground" };
    public static readonly string[] Anchors =
        { "PlayerSpawn", "GoosiniAnchor", "MedicAnchor", "WoundedSoldierAnchor", "WoundedCivilianAnchor", "SoldiersConversationAnchor", "CampfireAnchor", "RadioLockedAnchor" };

    public static float X(float design) => design / DesignPPU - DesignOrigin;
    public const float PlayableLeft = -5.3f, PlayableRight = 98.6f, ExpansionGateX = 84.6f, VisualLeft = -13.5f, VisualRight = 106.5f;

    // Altura (h) ou largura (w) em unidades do recorte opaco.
    private static readonly Dictionary<string, (char axis, float size)> Targets = new Dictionary<string, (char, float)>
    {
        { "Ambient/01_arvore_de_refugio", ('h', 3.6f) }, { "Ambient/02_tenda_descanso", ('w', 5.5f) },
        { "Ambient/03_plataforma_elevada", ('h', 2.6f) }, { "Ambient/04_posto_avancado", ('h', 2.8f) },
        { "Ambient/05_lona_cobertura", ('w', 3.0f) }, { "Ambient/06_poste_eletrico", ('h', 3.4f) },
        { "Ambient/07_suprimentos_caixas", ('w', 2.2f) }, { "Ambient/08_caminhao_carga", ('w', 3.8f) },
        { "Ambient/09_torre_observacao", ('h', 6.0f) }, { "Ambient/10_lampada_poste", ('h', 2.5f) },
        { "Buildings/01_hangar_goosini", ('w', 10.5f) }, { "Buildings/02_oficina_auxiliar", ('w', 7.0f) },
        { "Buildings/03_enfermaria", ('w', 9.0f) }, { "Buildings/04_comando_danificado", ('w', 9.5f) },
        { "Buildings/05_radio_comunicacoes", ('w', 8.5f) },
    };

    // Suprimentos (≈1230 px, "pixel" desenhado ≈18 px): cópias reduzidas por moda de bloco para PPU 80.
    private static readonly Dictionary<string, int> SupplyHeights = new Dictionary<string, int>
    {
        { "01_caixas_militares_empilhadas", 88 }, { "02_caixotes_madeira", 68 }, { "03_bau_militar", 44 },
        { "04_barril_azul", 60 }, { "05_barril_ferrugem", 60 }, { "06_caixa_ferramentas", 36 },
        { "07_prateleira_suprimentos", 104 }, { "08_palete_suprimentos", 68 }, { "09_sacos_areia", 50 },
        { "10_abrigo_suprimentos", 120 },
    };

    // Lâmpadas desenhadas na arte (px do PNG original, origem no topo) e diâmetro do halo em unidades.
    private static readonly Dictionary<string, (int x, int y, float size)[]> Lights = new Dictionary<string, (int, int, float)[]>
    {
        { "Buildings/01_hangar_goosini", new[] { (262, 350, 1.1f), (122, 532, .9f), (367, 512, .9f), (497, 490, 1.2f), (793, 490, 1.2f), (1053, 510, .9f), (897, 627, .8f), (1168, 680, .7f) } },
        { "Buildings/02_oficina_auxiliar", new[] { (122, 478, 1f), (630, 488, 1.2f), (950, 520, .8f), (1345, 518, 1f) } },
        { "Buildings/03_enfermaria", new[] { (342, 322, 1f), (1100, 318, 1f), (517, 522, .8f), (645, 510, .8f), (1016, 500, .8f), (1108, 537, .8f) } },
        { "Buildings/04_comando_danificado", new[] { (150, 660, .9f), (470, 393, .8f), (686, 412, .8f), (952, 170, .9f), (1060, 450, .7f), (1260, 500, .8f), (500, 670, .8f), (640, 655, .8f), (1075, 700, .8f) } },
        { "Buildings/05_radio_comunicacoes", new[] { (280, 598, .9f), (458, 158, .7f), (535, 630, .8f), (700, 655, .8f), (915, 660, .8f), (1378, 655, .9f) } },
        { "Ambient/02_tenda_descanso", new[] { (215, 155, 1.1f) } },
        { "Ambient/10_lampada_poste", new[] { (135, 85, 1.7f) } },
    };

    private static readonly Color Amber = new Color(1f, .62f, .26f, .32f);
    private static readonly Color RuinTint = new Color(.36f, .28f, .55f, 1f);
    private static readonly Color NearTint = new Color(.3f, .23f, .45f, 1f);
    private static readonly Color FrontTint = new Color(.05f, .04f, .07f, 1f);
    private static readonly Dictionary<string, RectInt> boxes = new Dictionary<string, RectInt>();
    private static readonly Dictionary<string, Vector2Int> sizes = new Dictionary<string, Vector2Int>();
    private static Material halo;
    private static Sprite haloSprite;
    private static readonly List<SpriteRenderer> flicker = new List<SpriteRenderer>();

    [MenuItem("Crocodilo/Base/Install BaseImprovisada map")]
    public static void Install()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        AssetDatabase.Refresh();
        EnsureSortingLayers();
        GenerateSupplies();
        GenerateHalo();
        AssetDatabase.Refresh();
        ConfigureImports();
        Directory.CreateDirectory(PrefabDir);
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        flicker.Clear();
        BuildPrefabs();
        BuildScene(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("BASE IMPROVISADA MAP INSTALLED");
        Validate();
    }

    // ------------------------------------------------------------------ importação
    private static void EnsureSortingLayers()
    {
        var manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers = manager.FindProperty("m_SortingLayers");
        var existing = Enumerable.Range(0, layers.arraySize).Select(i => layers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue).ToList();
        foreach (string name in SortingLayers)
        {
            if (existing.Contains(name)) continue;
            layers.InsertArrayElementAtIndex(layers.arraySize);
            var layer = layers.GetArrayElementAtIndex(layers.arraySize - 1);
            layer.FindPropertyRelative("name").stringValue = name;
            layer.FindPropertyRelative("uniqueID").longValue = (uint)(Animator.StringToHash("Crocodilo.Sorting." + name) & 0x7fffffff);
            var locked = layer.FindPropertyRelative("locked");
            if (locked != null) locked.intValue = 0;
            existing.Add(name);
        }
        manager.ApplyModifiedPropertiesWithoutUndo();
    }

    // Moda por bloco (sem mistura de cores): cada texel final recebe a cor mais frequente do bloco.
    private static void GenerateSupplies()
    {
        Directory.CreateDirectory(Art + "Supplies");
        foreach (var supply in SupplyHeights)
        {
            var source = Load(Originals + supply.Key + ".png");
            RectInt box = OpaqueBounds(source);
            float scale = box.height / (float)supply.Value;
            int width = Mathf.Max(1, Mathf.RoundToInt(box.width / scale)), height = supply.Value;
            var src = source.GetPixels32();
            var dst = new Color32[width * height];
            var counts = new Dictionary<int, (int n, int r, int g, int b)>();
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int x0 = box.xMin + Mathf.FloorToInt(x * scale), x1 = Mathf.Max(x0 + 1, box.xMin + Mathf.FloorToInt((x + 1) * scale));
                int y0 = box.yMin + Mathf.FloorToInt(y * scale), y1 = Mathf.Max(y0 + 1, box.yMin + Mathf.FloorToInt((y + 1) * scale));
                counts.Clear();
                int opaque = 0, area = 0;
                for (int sy = y0; sy < y1 && sy < source.height; sy++)
                for (int sx = x0; sx < x1 && sx < source.width; sx++)
                {
                    Color32 c = src[sy * source.width + sx];
                    area++;
                    if (c.a < 128) continue;
                    opaque++;
                    int key = (c.r >> 3) << 10 | (c.g >> 3) << 5 | c.b >> 3;
                    counts.TryGetValue(key, out var v);
                    counts[key] = (v.n + 1, v.r + c.r, v.g + c.g, v.b + c.b);
                }
                if (opaque * 2 < area || counts.Count == 0) { dst[y * width + x] = new Color32(0, 0, 0, 0); continue; }
                var best = counts.Values.OrderByDescending(v => v.n).First();
                dst[y * width + x] = new Color32((byte)(best.r / best.n), (byte)(best.g / best.n), (byte)(best.b / best.n), 255);
            }
            var output = new Texture2D(width, height, TextureFormat.RGBA32, false);
            output.SetPixels32(dst);
            output.Apply();
            File.WriteAllBytes(Art + "Supplies/" + supply.Key + ".png", output.EncodeToPNG());
            Object.DestroyImmediate(output);
            Object.DestroyImmediate(source);
        }
    }

    // Halo em degraus (sem gradiente suave), branco: a cor vem do SpriteRenderer.
    private static void GenerateHalo()
    {
        const int size = 32;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float[] steps = { .55f, .34f, .17f, .07f };
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float r = new Vector2(x - 15.5f, y - 15.5f).magnitude / 16f;
            float a = r < .22f ? steps[0] : r < .45f ? steps[1] : r < .7f ? steps[2] : r < .95f ? steps[3] : 0f;
            texture.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        texture.Apply();
        File.WriteAllBytes(HaloTexture, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
    }

    private static void ConfigureImports()
    {
        boxes.Clear(); sizes.Clear();
        var paths = Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories).Select(p => p.Replace('\\', '/')).ToArray();
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string path in paths)
            {
                string key = Key(path);
                var texture = Load(path);
                RectInt box = OpaqueBounds(texture);
                boxes[key] = box; sizes[key] = new Vector2Int(texture.width, texture.height);
                Object.DestroyImmediate(texture);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.maxTextureSize = 4096;
                importer.spritePixelsPerUnit = PixelsPerUnit(key, box);
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                bool big = key.StartsWith("Buildings/") || key.StartsWith("Ambient/");
                settings.spriteMeshType = big ? SpriteMeshType.Tight : SpriteMeshType.FullRect;
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = Pivot(key, box, sizes[key]);
                importer.SetTextureSettings(settings);
                if (key.StartsWith(SoilPrefix)) CropSoil(importer, key, box);
                importer.SaveAndReimport();
            }
        }
        finally { AssetDatabase.StopAssetEditing(); }
        AssetDatabase.Refresh();
    }

    // Os blocos de solo do Lote 01 têm contorno escuro nas laterais: repetidos lado a lado, viram uma
    // grade de emendas. O recorte lateral é só metadado do importer (o PNG original não muda).
    private const string SoilPrefix = "Terrain/01_solo/solo_";
    public const int SoilInset = 4;

    private static void CropSoil(TextureImporter importer, string key, RectInt box)
    {
        importer.spriteImportMode = SpriteImportMode.Multiple;
        var factory = new UnityEditor.U2D.Sprites.SpriteDataProviderFactories();
        factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        var existing = provider.GetSpriteRects();
        var rect = new UnityEditor.SpriteRect
        {
            name = Path.GetFileName(key),
            rect = new Rect(box.xMin + SoilInset, box.yMin, Mathf.Max(1, box.width - SoilInset * 2), box.height),
            alignment = SpriteAlignment.TopLeft,
            pivot = new Vector2(0f, 1f),
            spriteID = existing.Length > 0 ? existing[0].spriteID : GUID.Generate(),
        };
        provider.SetSpriteRects(new[] { rect });
        var names = provider.GetDataProvider<UnityEditor.U2D.Sprites.ISpriteNameFileIdDataProvider>();
        names?.SetNameFileIdPairs(new[] { new UnityEditor.SpriteNameFileIdPair(rect.name, rect.spriteID) });
        provider.Apply();
    }

    public static float PixelsPerUnit(string key, RectInt box)
    {
        if (key.StartsWith("Lighting/")) return 32f;
        if (Targets.TryGetValue(key, out var target)) return (target.axis == 'h' ? box.height : box.width) / target.size;
        return DesignPPU;
    }

    // Terreno de superfície: canto superior esquerdo do recorte opaco (o topo encosta no caminho).
    // Demais: centro inferior do recorte, para apoiar no chão sem flutuar.
    private static Vector2 Pivot(string key, RectInt box, Vector2Int size)
    {
        if (key.StartsWith("Lighting/") || key.StartsWith("Background/")) return new Vector2(.5f, .5f);
        if (key.StartsWith("_Reference/")) return new Vector2(.5f, 1f / 72f);
        if (key.StartsWith("Terrain/01_solo") || key.StartsWith("Terrain/04_pisos"))
            return new Vector2(box.xMin / (float)size.x, box.yMax / (float)size.y);
        return new Vector2((box.xMin + box.width * .5f) / size.x, box.yMin / (float)size.y);
    }

    // Recorte opaco (alpha ≥ 40), ignorando linhas/colunas com menos de 2 pixels (ruído do recorte).
    public static RectInt OpaqueBounds(Texture2D texture)
    {
        var pixels = texture.GetPixels32();
        int w = texture.width, h = texture.height;
        var rows = new int[h]; var cols = new int[w];
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
            if (pixels[y * w + x].a >= 40) { rows[y]++; cols[x]++; }
        int x0 = Array.FindIndex(cols, c => c >= 2), x1 = Array.FindLastIndex(cols, c => c >= 2);
        int y0 = Array.FindIndex(rows, c => c >= 2), y1 = Array.FindLastIndex(rows, c => c >= 2);
        if (x0 < 0 || y0 < 0) return new RectInt(0, 0, w, h);
        return new RectInt(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
    }

    // ------------------------------------------------------------------ prefabs
    private static void BuildPrefabs()
    {
        halo = AssetDatabase.LoadAssetAtPath<Material>(HaloMaterial);
        if (halo == null)
        {
            var shader = Shader.Find("Mobile/Particles/Additive") ?? Shader.Find("Legacy Shaders/Particles/Additive");
            halo = new Material(shader) { name = "BaseLightHalo" };
            AssetDatabase.CreateAsset(halo, HaloMaterial);
        }
        haloSprite = AssetDatabase.LoadAssetAtPath<Sprite>(HaloTexture);
        halo.mainTexture = haloSprite.texture;
        EditorUtility.SetDirty(halo);
        SavePrefab("HubInfirmary", ("Buildings/03_enfermaria", 0f));
        SavePrefab("HubHangar", ("Buildings/01_hangar_goosini", 0f), ("Buildings/02_oficina_auxiliar", 8.75f));
        SavePrefab("HubCamp", ("Ambient/02_tenda_descanso", 0f), ("Ambient/05_lona_cobertura", 4.75f), ("Ambient/07_suprimentos_caixas", -3.0f));
        SavePrefab("HubCommandRuins", ("Buildings/04_comando_danificado", 0f));
        SavePrefab("HubRadio", ("Buildings/05_radio_comunicacoes", 0f));
        SavePrefab("HubLampPost", ("Ambient/10_lampada_poste", 0f));
    }

    private static void SavePrefab(string name, params (string key, float x)[] parts)
    {
        var root = new GameObject(name);
        int order = 0;
        foreach (var part in parts)
        {
            var art = Place(part.key, root.transform, part.x, 0f, "WorldBack", 5 + order++);
            art.name = Path.GetFileName(part.key);
            if (!Lights.TryGetValue(part.key, out var lamps)) continue;
            var sprite = art.sprite;
            float ppu = sprite.pixelsPerUnit;
            Vector2 pivot = sprite.pivot;
            int index = 0;
            foreach (var lamp in lamps)
            {
                var local = new Vector2((lamp.x - pivot.x) / ppu, (sprite.texture.height - lamp.y - pivot.y) / ppu);
                var light = new GameObject("Light_" + index++).AddComponent<SpriteRenderer>();
                light.transform.SetParent(art.transform, false);
                light.transform.localPosition = local;
                light.transform.localScale = Vector3.one * lamp.size;
                light.sprite = haloSprite;
                light.sharedMaterial = halo;
                light.color = Amber;
                light.sortingLayerName = "AmbientEffects";
            }
        }
        PrefabUtility.SaveAsPrefabAsset(root, PrefabDir + name + ".prefab");
        Object.DestroyImmediate(root);
    }

    // ------------------------------------------------------------------ cena
    private static void BuildScene(UnityEngine.SceneManagement.Scene scene)
    {
        var roots = scene.GetRootGameObjects().ToDictionary(r => r.name);
        foreach (string name in Generated) if (roots.TryGetValue(name, out var old)) Object.DestroyImmediate(old);
        // Objetos originais preservados: Environment/Ground e Collisions/Floor, LeftWall, RightWall.
        var environment = roots["Environment"].transform;
        foreach (Transform child in environment.Cast<Transform>().ToArray())
            if (child.name != "Ground") Object.DestroyImmediate(child.gameObject);
        var collisions = roots["Collisions"].transform;
        foreach (Transform child in collisions.Cast<Transform>().ToArray())
            if (child.name != "Floor" && child.name != "LeftWall" && child.name != "RightWall") Object.DestroyImmediate(child.gameObject);

        // Câmera existente: só o enquadramento vertical, para mostrar construções e céu (tamanho e X intactos).
        var camera = roots["Cameras"].GetComponentInChildren<Camera>(true);
        var cameraPosition = camera.transform.position;
        camera.transform.position = new Vector3(cameraPosition.x, CameraY, cameraPosition.z);
        camera.backgroundColor = new Color(.12f, .08f, .2f, 1f);

        var random = new System.Random(20261018);
        BuildBackground(camera, random);
        BuildGround(collisions, random);
        BuildZones(environment, random);
        BuildForeground(camera, random);
        BaseImprovisadaMapRepairTools.ArrangeScenery();
        BuildMarkers(collisions);

        var lighting = new GameObject("Lighting");
        var flickerData = new SerializedObject(lighting.AddComponent<BaseLightFlicker>());
        var list = flickerData.FindProperty("halos");
        var chosen = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include)
            .Where(r => r.gameObject.scene == scene && r.sprite == haloSprite).OrderBy(r => r.transform.position.x).Where((r, i) => i % 3 == 0).ToArray();
        list.arraySize = chosen.Length;
        for (int i = 0; i < chosen.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = chosen[i];
        flickerData.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BuildBackground(Camera camera, System.Random random)
    {
        var background = new GameObject("Background").transform;
        // BG0: um único panorama opaco (montanhas + sol). Nenhuma outra imagem opaca é empilhada.
        var sky = Layer("BG0_Panorama", background, camera, .03f);
        Place("Background/03_BG2_montanhas_referencia_RGB", sky, 0f, CameraY, "BackgroundSky", 0);
        // Reservados: nuvens e montanhas separadas ainda não existem como arte RGBA.
        new GameObject("BG1_Clouds (reservado)").transform.SetParent(background, false);
        new GameObject("BG2_Mountains (no panorama)").transform.SetParent(background, false);

        var ruins = Layer("BG3_Ruins", background, camera, .3f);
        string[] ruinArt = { "Ambient/09_torre_observacao", "Ambient/06_poste_eletrico", "Buildings/04_comando_danificado", "Ambient/04_posto_avancado",
            "Ambient/03_plataforma_elevada", "Terrain/06_paredes_estruturas/trelica_metalica", "Buildings/05_radio_comunicacoes", "Ambient/06_poste_eletrico" };
        float[] ruinScale = { .45f, .6f, .42f, .55f, .5f, 2.2f, .4f, .5f };
        int r = 0;
        for (float x = -12f; x <= 40f; x += 4.2f + (float)random.NextDouble() * 1.6f, r++)
        {
            int i = r % ruinArt.Length;
            Place(ruinArt[i], ruins, x, .55f, "BackgroundRuins", r % 3, ruinScale[i], RuinTint, random.Next(2) == 0);
        }

        var near = Layer("BG4_Near", background, camera, .6f);
        // Só silhuetas com recortes (cercas, treliças, árvores, postes): rochas maciças tingidas viravam manchas pretas.
        string[] nearArt = { "Terrain/08_cercas_grades_arames/cerca_malha_02", "Ambient/01_arvore_de_refugio", "Terrain/06_paredes_estruturas/trelica_metalica",
            "Terrain/08_cercas_grades_arames/cerca_arame_01", "Ambient/06_poste_eletrico", "Terrain/06_paredes_estruturas/madeiramento",
            "Terrain/08_cercas_grades_arames/cerca_malha_01", "Ambient/04_posto_avancado" };
        float[] nearScale = { 1.6f, .62f, 1.7f, 1.7f, .72f, 1.6f, 1.6f, .6f };
        int n = 0;
        for (float x = -12f; x <= 66f; x += 2.6f + (float)random.NextDouble() * 1.8f, n++)
        {
            int i = n % nearArt.Length;
            Place(nearArt[i], near, x, -.25f, "BackgroundNear", n % 4, nearScale[i], NearTint, random.Next(2) == 0);
        }
    }

    // Integer grid and opacity-aware slices; the repair owns the current terrain recipe.
    private static void BuildGround(Transform collisions, System.Random random)
    {
        BaseImprovisadaMapRepairTools.BuildTerrain(collisions);
        Wall(collisions.Find("LeftWall"), PlayableLeft);
        Wall(collisions.Find("RightWall"), PlayableRight);
    }

    private static void BuildZones(Transform environment, System.Random random)
    {
        // A — entrada / spawn
        var entrance = Zone(environment, "Entrance", "entrance", BaseSector.State.Open);
        Place("Ambient/04_posto_avancado", entrance, -3.6f, 0f, "WorldBack", 20);
        Fence(entrance, -6.3f, new[] { "cerca_malha_02", "cerca_longa" });
        Fence(entrance, -1.9f, new[] { "cerca_longa", "cerca_chapa_vertical", "cerca_malha_01", "cerca_arame_01", "cerca_malha_02" });
        Lamp(entrance, 1.0f);
        Place("Supplies/04_barril_azul", entrance, -2.3f, 0f, "WorldBack", 50);
        Place("Supplies/01_caixas_militares_empilhadas", entrance, 2.7f, 0f, "WorldBack", 50);
        Place("Supplies/03_bau_militar", entrance, 3.7f, 0f, "WorldBack", 51);
        Place("Terrain/10_barreiras_defesas/barreira_baixa_01", entrance, 4.7f, 0f, "WorldBack", 45);
        Place("Supplies/09_sacos_areia", entrance, 5.4f, 0f, "WorldBack", 52);
        Place("Terrain/10_barreiras_defesas/barreira_alta_02", entrance, -5.6f, 0f, "WorldBack", 46);

        // B — enfermaria
        var infirmary = Zone(environment, "Infirmary", "infirmary", BaseSector.State.Open);
        Prefab("HubInfirmary", infirmary, 9.75f);
        Place("Supplies/02_caixotes_madeira", infirmary, 4.9f, 0f, "WorldBack", 50);
        Place("Supplies/09_sacos_areia", infirmary, 14.8f, 0f, "WorldBack", 52);
        Place("Supplies/06_caixa_ferramentas", infirmary, 15.7f, 0f, "WorldBack", 53);
        Lamp(infirmary, 16.4f);
        Place("Supplies/05_barril_ferrugem", infirmary, 18.1f, 0f, "WorldBack", 50);

        // C — hangar / oficina do Goosini (com a área externa entre a enfermaria e o hangar)
        var hangar = Zone(environment, "Hangar", "hangar", BaseSector.State.Open);
        Place("Ambient/08_caminhao_carga", hangar, 20.6f, 0f, "WorldBack", 22);
        Place("Supplies/08_palete_suprimentos", hangar, 22.7f, 0f, "WorldBack", 50);
        Place("Supplies/07_prateleira_suprimentos", hangar, 23.7f, 0f, "WorldBack", 48);
        Prefab("HubHangar", hangar, 29.75f);
        Place("Supplies/02_caixotes_madeira", hangar, 25.1f, 0f, "WorldBack", 50);
        Place("Supplies/06_caixa_ferramentas", hangar, 34.3f, 0f, "WorldBack", 50);
        Place("Supplies/04_barril_azul", hangar, 41.5f, 0f, "WorldBack", 50);

        // D — acampamento
        var camp = Zone(environment, "Camp", "camp", BaseSector.State.Open);
        Lamp(camp, 43.0f);
        Place("Ambient/01_arvore_de_refugio", camp, 44.7f, 0f, "WorldBack", 18);
        Prefab("HubCamp", camp, 50.25f);
        Place("Supplies/03_bau_militar", camp, 46.1f, 0f, "WorldBack", 50);
        Place("Supplies/05_barril_ferrugem", camp, 52.6f, 0f, "WorldBack", 50);
        Place("Supplies/04_barril_azul", camp, 53.3f, 0f, "WorldBack", 51);

        // E — comando danificado: cenográfico, entrada barricada (não funciona como loja)
        var command = Zone(environment, "CommandRuins", "command_ruins", BaseSector.State.Scenic);
        Place("Ambient/06_poste_eletrico", command, 58.75f, 0f, "WorldBack", 18);
        Prefab("HubCommandRuins", command, 64.75f);
        var commandBarricade = Group(command, "Barricade");
        Place("Terrain/10_barreiras_defesas/barreira_alta_01", commandBarricade, 63.0f, 0f, "WorldBack", 55);
        Place("Terrain/10_barreiras_defesas/barreira_alta_03", commandBarricade, 64.1f, 0f, "WorldBack", 56);
        Place("Supplies/09_sacos_areia", commandBarricade, 65.5f, 0f, "WorldBack", 57);
        Place("Terrain/10_barreiras_defesas/barreira_baixa_03", commandBarricade, 67.9f, 0f, "WorldBack", 55);
        SetSector(command, new[] { commandBarricade.gameObject });

        // F — rádio: fechado até progresso futuro
        var radio = Zone(environment, "Radio", "radio", BaseSector.State.Locked);
        Lamp(radio, 71.3f);
        Prefab("HubRadio", radio, 77.5f);
        var radioBarricade = Group(radio, "Barricade");
        Place("Terrain/08_cercas_grades_arames/cerca_chapa_vertical", radioBarricade, 76.1f, 0f, "WorldBack", 55);
        Place("Terrain/10_barreiras_defesas/barreira_alta_04", radioBarricade, 77.2f, 0f, "WorldBack", 56);
        Place("Terrain/10_barreiras_defesas/barreira_baixa_02", radioBarricade, 78.4f, 0f, "WorldBack", 56);
        Place("Supplies/02_caixotes_madeira", radioBarricade, 79.6f, 0f, "WorldBack", 57);
        SetSector(radio, new[] { radioBarricade.gameObject });

        // G — expansão em obras: limite físico de progressão
        var expansion = Zone(environment, "ExpansionArea", "expansion", BaseSector.State.Locked);
        Lamp(expansion, 83.0f);
        var gate = Group(expansion, "Gate");
        // Portão fechado em obras: grade + chapas atrás, barricadas listradas e sacos na frente.
        Place("Terrain/08_cercas_grades_arames/cerca_chapa_vertical", gate, ExpansionGateX - .85f, 0f, "WorldBack", 56);
        Place("Terrain/06_paredes_estruturas/porta_grade", gate, ExpansionGateX, 0f, "WorldBack", 57);
        Place("Terrain/08_cercas_grades_arames/cerca_chapa_vertical", gate, ExpansionGateX + .85f, 0f, "WorldBack", 56, 1f, null, true);
        Place("Terrain/10_barreiras_defesas/barreira_baixa_04", gate, ExpansionGateX - .5f, 0f, "WorldBack", 60);
        Place("Terrain/10_barreiras_defesas/barreira_baixa_01", gate, ExpansionGateX + .5f, 0f, "WorldBack", 61);
        Place("Terrain/10_barreiras_defesas/barreira_alta_02", gate, ExpansionGateX, .62f, "WorldBack", 62);
        var blocker = new GameObject("Blocker").AddComponent<BoxCollider2D>();
        blocker.transform.SetParent(gate, false);
        blocker.transform.position = new Vector3(ExpansionGateX, 2.5f, 0f);
        blocker.size = new Vector2(.4f, 5f);
        var works = Group(expansion, "WorksInProgress");
        Place("Ambient/09_torre_observacao", works, 87.2f, 0f, "WorldBack", 12);
        for (int i = 0; i < 9; i++)
            Place($"Terrain/07_paineis_chapas/chapa_ondulada_0{1 + (i * 5 + 2) % 6}", works, 85.6f + i * .55f, 0f, "WorldBack", 30 + i % 2);
        Place("Terrain/06_paredes_estruturas/trelica_metalica", works, 89.0f, 0f, "WorldBack", 25);
        Place("Ambient/03_plataforma_elevada", works, 91.0f, 0f, "WorldBack", 20);
        Place("Terrain/06_paredes_estruturas/madeiramento", works, 92.9f, 0f, "WorldBack", 26);
        Place("Terrain/09_escadas/escada_plataforma", works, 93.8f, 0f, "WorldBack", 27);
        Place("Supplies/08_palete_suprimentos", works, 94.9f, 0f, "WorldBack", 50);
        Place("Supplies/10_abrigo_suprimentos", works, 96.2f, 0f, "WorldBack", 40);
        Fence(works, 97.2f, new[] { "cerca_longa", "cerca_malha_01", "cerca_chapa_vertical", "cerca_malha_02" });
        Place("Terrain/10_barreiras_defesas/barreira_alta_01", works, 98.9f, 0f, "WorldBack", 46);
        SetSector(expansion, new[] { gate.gameObject });
    }

    // Entulho escuro no rodapé da tela, à frente do mundo (move-se mais rápido: fator 1,25).
    private static void BuildForeground(Camera camera, System.Random random)
    {
        var front = Layer("Foreground", null, camera, 1.25f);
        // Silhuetas de contorno legível sobre as fileiras de pedra; os topos ficam abaixo do caminho.
        string[] art = { "Supplies/09_sacos_areia", "Terrain/02_rochas/rocha_solida", "Supplies/05_barril_ferrugem", "Terrain/03_grama/grama_solta_01",
            "Supplies/02_caixotes_madeira", "Terrain/03_grama/grama_solta_05", "Terrain/10_barreiras_defesas/barreira_baixa_03", "Terrain/03_grama/grama_solta_02" };
        float[] scale = { 1.6f, 1.9f, 1.5f, 1.8f, 1.4f, 1.8f, 1.5f, 1.7f };
        int k = 0;
        for (float x = -11f; x < 125f; x += 3.4f + (float)random.NextDouble() * 3f, k++)
        {
            int i = k % art.Length;
            Place(art[i], front, x, -2.25f - (float)random.NextDouble() * .15f, "WorldFront", 10 + k % 3, scale[i], FrontTint, random.Next(2) == 0);
        }
    }

    private static void BuildMarkers(Transform collisions)
    {
        var existing = GameObject.Find("Characters");
        var characters = existing != null ? existing.transform : new GameObject("Characters").transform;
        var spawn = characters.Find("PlayerSpawn");
        if (spawn == null) spawn = Marker("PlayerSpawn", characters, X(430));
        // Só referência de escala no Editor (48×72, PPU 80 → 0,9 unidade). EditorOnly não entra no build.
        if (characters.GetComponentInChildren<HubCharacterMotor2D>(true) == null && !characters.Cast<Transform>().Any(t => t.name.StartsWith("CrocScaleReference")))
        {
            var reference = Place("_Reference/croc_hub_master", characters, spawn.position.x, 0f, "WorldCharacters", 0);
            reference.gameObject.name = "CrocScaleReference (EditorOnly)";
            reference.gameObject.tag = "EditorOnly";
        }

        var anchors = new GameObject("NPCAnchors").transform;
        Marker("GoosiniAnchor", anchors, X(3020));
        Marker("MedicAnchor", anchors, X(1460));
        Marker("WoundedSoldierAnchor", anchors, 8.0f);
        Marker("WoundedCivilianAnchor", anchors, 6.6f);
        Marker("SoldiersConversationAnchor", anchors, X(4630));
        Marker("CampfireAnchor", anchors, 48.9f);
        Marker("RadioLockedAnchor", anchors, X(6740));

        var extents = new GameObject("MapExtents").transform;
        extents.SetParent(collisions, false);
        Marker("PlayableLeft", extents, PlayableLeft);
        Marker("PlayableRight", extents, PlayableRight);
        Marker("VisualLeft", extents, VisualLeft);
        Marker("VisualRight", extents, VisualRight);
    }

    // ------------------------------------------------------------------ utilitários
    private static Transform Layer(string name, Transform parent, Camera camera, float factor)
    {
        var layer = new GameObject(name);
        if (parent != null) layer.transform.SetParent(parent, false);
        var data = new SerializedObject(layer.AddComponent<ParallaxLayer2D>());
        data.FindProperty("targetCamera").objectReferenceValue = camera;
        data.FindProperty("scrollFactor").floatValue = factor;
        data.FindProperty("referenceCameraX").floatValue = camera.transform.position.x;
        data.ApplyModifiedPropertiesWithoutUndo();
        return layer.transform;
    }

    private static Transform Zone(Transform environment, string name, string id, BaseSector.State state)
    {
        var zone = new GameObject(name);
        zone.transform.SetParent(environment, false);
        var data = new SerializedObject(zone.AddComponent<BaseSector>());
        data.FindProperty("sectorId").stringValue = id;
        data.FindProperty("state").enumValueIndex = (int)state;
        data.ApplyModifiedPropertiesWithoutUndo();
        return zone.transform;
    }

    private static void SetSector(Transform zone, GameObject[] closedOnly)
    {
        var data = new SerializedObject(zone.GetComponent<BaseSector>());
        var list = data.FindProperty("closedOnly");
        list.arraySize = closedOnly.Length;
        for (int i = 0; i < closedOnly.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = closedOnly[i];
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Transform Group(Transform parent, string name)
    {
        var group = new GameObject(name).transform;
        group.SetParent(parent, false);
        return group;
    }

    private static GameObject Prefab(string name, Transform parent, float x)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + name + ".prefab"), parent);
        instance.transform.position = new Vector3(x, -.06f, 0f);
        return instance;
    }

    private static void Lamp(Transform parent, float x) => Prefab("HubLampPost", parent, x).transform.position = new Vector3(x, 0f, 0f);

    private static void Fence(Transform parent, float start, string[] names)
    {
        float x = start;
        foreach (string name in names)
        {
            // start é a borda esquerda da fileira; cada peça encosta na anterior (pivô no centro inferior).
            var fence = Place("Terrain/08_cercas_grades_arames/" + name, parent, x, 0f, "WorldBack", 30);
            float width = fence.sprite.bounds.size.x;
            fence.transform.position = new Vector3(x + width * .5f, 0f, 0f);
            x += width - .04f;
        }
    }

    private static Transform Marker(string name, Transform parent, float x)
    {
        var marker = new GameObject(name);
        marker.transform.SetParent(parent, false);
        marker.transform.position = new Vector3(x, 0f, 0f);
        var icon = EditorGUIUtility.IconContent("sv_label_3").image as Texture2D;
        if (icon != null) EditorGUIUtility.SetIconForObject(marker, icon);
        return marker.transform;
    }

    private static void Wall(Transform wall, float x)
    {
        wall.position = new Vector3(x, 2.5f, 0f);
        wall.GetComponent<BoxCollider2D>().size = new Vector2(.2f, 5f);
    }

    public static SpriteRenderer Place(string key, Transform parent, float x, float y, string layer, int order, float scale = 1f, Color? tint = null, bool flip = false)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + key + ".png");
        if (sprite == null) throw new InvalidOperationException("Missing sprite " + key);
        var renderer = new GameObject(Path.GetFileName(key)).AddComponent<SpriteRenderer>();
        if (parent != null) renderer.transform.SetParent(parent, false);
        renderer.transform.position = new Vector3(x, y, 0f);
        renderer.transform.localScale = Vector3.one * scale;
        renderer.sprite = sprite;
        renderer.sortingLayerName = layer;
        renderer.sortingOrder = order;
        renderer.flipX = flip;
        if (tint.HasValue) renderer.color = tint.Value;
        return renderer;
    }

    public static string Key(string path) => path.Substring(Art.Length, path.Length - Art.Length - 4);

    private static Texture2D Load(string path)
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(File.ReadAllBytes(path));
        return texture;
    }

    // ------------------------------------------------------------------ validação estática
    private static int checks;

    [MenuItem("Crocodilo/Base/Validate BaseImprovisada map")]
    public static void Validate()
    {
        checks = 0;
        var paths = Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories).Select(p => p.Replace('\\', '/')).ToArray();
        Require(paths.Length == 82 + 10 + 10 + 5 + 1 + 1 + 1, $"{paths.Length} runtime PNGs (82 terreno, 10 suprimentos reduzidos, 10 ambientação, 5 construções, 1 panorama, 1 halo, 1 referência)");
        foreach (string path in paths)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Require(importer.textureType == TextureImporterType.Sprite && importer.filterMode == FilterMode.Point && !importer.mipmapEnabled
                && importer.textureCompression == TextureImporterCompression.Uncompressed && importer.alphaIsTransparency, "Pixel import " + path);
        }
        foreach (var supply in SupplyHeights)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Art + "Supplies/" + supply.Key + ".png");
            Require(texture != null && texture.height == supply.Value && texture.width < 260 && File.Exists(Originals + supply.Key + ".png"),
                $"Reduced supply {supply.Key} {texture?.width}×{texture?.height}; original preserved in SourceArt");
        }
        var croc = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "_Reference/croc_hub_master.png");
        Require(croc != null && croc.rect.width == 48 && croc.rect.height == 72 && Mathf.Approximately(croc.bounds.size.y, .9f), "Croc master 48×72 at PPU 80 = 0.9 units");
        var layers = UnityEngine.SortingLayer.layers.Select(l => l.name).ToArray();
        Require(SortingLayers.All(layers.Contains), "Sorting layers present");
        Require(SortingLayers.Select(n => Array.IndexOf(layers, n)).Zip(SortingLayers.Select(n => Array.IndexOf(layers, n)).Skip(1), (a, b) => a < b).All(v => v), "Sorting layers in back-to-front order");
        foreach (string name in new[] { "HubInfirmary", "HubHangar", "HubCamp", "HubCommandRuins", "HubRadio", "HubLampPost" })
            Require(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + name + ".prefab") != null, "Prefab " + name);
        var chapter = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Data/Flow/Chapter1.asset");
        Require(chapter != null && new SerializedObject(chapter).FindProperty("nextScene").stringValue == "BaseImprovisada", "Chapter1.nextScene still BaseImprovisada");
        Require(EditorBuildSettings.scenes.Any(s => s.enabled && s.path == ScenePath), "BaseImprovisada enabled in build settings");

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var roots = scene.GetRootGameObjects();
        var all = roots.SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
        Require(all.All(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) == 0), "No missing scripts");
        var renderers = roots.SelectMany(r => r.GetComponentsInChildren<SpriteRenderer>(true)).ToArray();
        Require(renderers.All(r => r.sprite != null), $"All {renderers.Length} sprite renderers have sprites");
        Require(renderers.All(r => layers.Contains(r.sortingLayerName) && r.sortingLayerName != "Default"), "Every renderer on a named base sorting layer");
        Require(roots.Count(r => r.GetComponentInChildren<Camera>(true) != null) == 1 && roots.SelectMany(r => r.GetComponentsInChildren<Camera>(true)).Count() == 1, "Single existing camera");
        var camera = roots.SelectMany(r => r.GetComponentsInChildren<Camera>(true)).Single();
        Require(camera.orthographic && Mathf.Approximately(camera.orthographicSize, 4.5f)
            && camera.GetComponents<MonoBehaviour>().All(c => c is HubCameraController2D || c is UnityEngine.U2D.PixelPerfectCamera), "Base camera: authored size 4.5, only dedicated HUB components");
        var panorama = renderers.Where(r => r.sprite.texture.name.Contains("BG2")).ToArray();
        Require(panorama.Length == 1, "Exactly one opaque panorama in the background");
        var view = new Rect(camera.transform.position.x - 10.5f, camera.transform.position.y - 4.5f, 21f, 9f);
        Require(panorama[0].bounds.min.x <= view.xMin && panorama[0].bounds.max.x >= view.xMax && panorama[0].bounds.min.y <= view.yMin && panorama[0].bounds.max.y >= view.yMax,
            "Panorama covers a 21:9 view at the authored camera");
        Require(roots.SelectMany(r => r.GetComponentsInChildren<PolygonCollider2D>(true)).Count() == 0, "No silhouette PolygonCollider2D");
        var boxes2D = roots.SelectMany(r => r.GetComponentsInChildren<BoxCollider2D>(true)).ToArray();
        Require(boxes2D.Length == 4, $"Only floor, two walls and the expansion blocker collide ({boxes2D.Length})");
        var floor = boxes2D.Single(b => b.name == "Floor");
        Require(floor.bounds.min.x <= PlayableLeft && floor.bounds.max.x >= PlayableRight && Mathf.Abs(floor.bounds.max.y) < .001f, "One continuous floor collider, top at y = 0");
        foreach (string id in new[] { "Infirmary", "Hangar", "Camp", "CommandRuins", "Radio", "ExpansionArea", "Entrance", "Terrain" })
            Require(all.Any(t => t.name == id && t.parent != null && t.parent.name == "Environment"), "Environment/" + id);
        var sectors = roots.SelectMany(r => r.GetComponentsInChildren<BaseSector>(true)).ToDictionary(s => s.SectorId);
        Require(sectors["hangar"].Current == BaseSector.State.Open && sectors["infirmary"].Current == BaseSector.State.Open && sectors["camp"].Current == BaseSector.State.Open
            && sectors["command_ruins"].Current == BaseSector.State.Scenic && sectors["radio"].Current == BaseSector.State.Locked && sectors["expansion"].Current == BaseSector.State.Locked,
            "Open: entrada, enfermaria, hangar, acampamento; fechados: comando, rádio, expansão");
        foreach (string anchor in Anchors) Require(all.Count(t => t.name == anchor) == 1, "Anchor " + anchor);
        foreach (var building in all.Where(t => t.name.StartsWith("Hub") && PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject)))
            Require(building.position.y <= 0f && building.position.y >= -.1f, $"{building.name} seated on the ground ({building.position.y:0.00})");
        var reference = all.SingleOrDefault(t => t.name.StartsWith("CrocScaleReference"));
        Require(reference != null ? reference.CompareTag("EditorOnly") : all.Count(t => t.GetComponent<HubCharacterMotor2D>() != null) == 1, "Editor scale marker or single simplified HUB character");
        Require(roots.SelectMany(r => r.GetComponentsInChildren<ParallaxLayer2D>(true)).Count() == 4, "Four parallax layers with real art (BG0, BG3, BG4, foreground)");
        Require(roots.SelectMany(r => r.GetComponentsInChildren<MonoBehaviour>(true)).Count(b => !(b is ParallaxLayer2D) && !(b is BaseSector) && !(b is BaseLightFlicker)
            && !(b is HubPlayerInput) && !(b is HubCharacterMotor2D) && !(b is HubCameraController2D) && !(b is UnityEngine.U2D.PixelPerfectCamera)
            && b.GetComponentInParent<PauseMenuView>(true) == null && !(b is UnityEngine.EventSystems.EventSystem) && !(b is UnityEngine.InputSystem.UI.InputSystemUIInputModule)) == 0,
            "No per-decoration scripts (only parallax roots, sector states and one flicker manager)");
        Debug.Log($"BASE IMPROVISADA MAP VALIDATION PASSED: {checks} checks; {renderers.Length} sprite renderers.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("BASE MAP CHECK FAILED: " + message);
        checks++;
    }
}
