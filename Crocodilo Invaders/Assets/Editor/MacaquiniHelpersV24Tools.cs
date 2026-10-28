using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MacaquiniHelpersV24Tools
{
    [MenuItem("Crocodilo/Fase 2/Primeira forma/Importar voadores v24")]
    public static void Import()
    {
        AssetDatabase.Refresh();
        foreach (string file in Directory.GetFiles("Assets/Resources/MacaquiniHelpersV24", "*.png"))
        {
            string path = file.Replace('\\', '/');
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64; importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.wrapMode = TextureWrapMode.Clamp;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Center; settings.spritePivot = new Vector2(.5f, .5f);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings); importer.SaveAndReimport();
        }
        AssetDatabase.SaveAssets(); Debug.Log("MACAQUINI_HELPERS_V24_IMPORT_OK");
    }
    public static void Build()
    {
        Import(); string path = Path.GetFullPath("Builds/Windows-Macaquini-Voadores-v24/MacaquiniSelva.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { MacaquiniJetpackTools.Test }.Concat(EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path)).Distinct().ToArray(),
            locationPathName = path, target = BuildTarget.StandaloneWindows64, options = BuildOptions.CompressWithLz4HC
        });
        Debug.Log("MACAQUINI_HELPERS_V24_BUILD " + report.summary.result);
        if (Application.isBatchMode) EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
