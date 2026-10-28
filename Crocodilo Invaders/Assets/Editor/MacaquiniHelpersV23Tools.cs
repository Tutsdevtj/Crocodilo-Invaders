using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public sealed class MacaquiniHelpersV23Importer : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/Resources/MacaquiniHelpersV23/")) return;
        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 64; importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed; importer.wrapMode = TextureWrapMode.Clamp;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Center; settings.spritePivot = new Vector2(.5f, .5f);
        settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
    }
}

public static class MacaquiniHelpersV23Tools
{
    [MenuItem("Crocodilo/Fase 2/Primeira forma/Importar ajudantes v23")]
    public static void Import()
    {
        foreach (string path in Directory.GetFiles("Assets/Resources/MacaquiniHelpersV23", "*.png"))
            AssetDatabase.ImportAsset(path.Replace('\\', '/'), ImportAssetOptions.ForceUpdate);
        AssetDatabase.SaveAssets();
        Debug.Log("MACAQUINI_HELPERS_V23_IMPORT_OK");
    }
    public static void Build()
    {
        Import();
        string path = Path.GetFullPath("Builds/Windows-Macaquini-Ajudantes-v23/MacaquiniSelva.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { MacaquiniJetpackTools.Test }.Concat(EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path)).Distinct().ToArray(), locationPathName = path,
            target = BuildTarget.StandaloneWindows64, options = BuildOptions.CompressWithLz4HC
        });
        Debug.Log("MACAQUINI_HELPERS_V23_BUILD " + report.summary.result);
        if (Application.isBatchMode) EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
