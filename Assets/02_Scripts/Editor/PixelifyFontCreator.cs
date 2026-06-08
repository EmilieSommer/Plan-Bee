#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.TextCore.LowLevel;

public static class PixelifyFontCreator
{
    [MenuItem("Tools/Create Pixelify Font Assets")]
    public static void CreateFontAssets()
    {
        CreateFontAsset(
            "Assets/Fonts/PixelifySans-Regular.ttf",
            "Assets/Fonts/PixelifySans-Regular SDF.asset");
        CreateFontAsset(
            "Assets/Fonts/PixelifySans-Bold.ttf",
            "Assets/Fonts/PixelifySans-Bold SDF.asset");
        AssetDatabase.Refresh();
        Debug.Log("[PixelifyFontCreator] Done. Both TMP font assets created.");
    }

    private static void CreateFontAsset(string ttfPath, string assetPath)
    {
        var fontFile = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
        if (fontFile == null)
        {
            Debug.LogError($"[PixelifyFontCreator] Source font not found at {ttfPath}");
            return;
        }

        var fontAsset = TMP_FontAsset.CreateFontAsset(
            font: fontFile,
            samplingPointSize: 16,
            atlasPadding: 4,
            renderMode: GlyphRenderMode.SDFAA,
            atlasWidth: 512,
            atlasHeight: 512,
            atlasPopulationMode: AtlasPopulationMode.Dynamic,
            enableMultiAtlasSupport: true);

        if (fontAsset == null)
        {
            Debug.LogError($"[PixelifyFontCreator] Failed to create font asset for {ttfPath}");
            return;
        }

        AssetDatabase.CreateAsset(fontAsset, assetPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"[PixelifyFontCreator] Wrote {assetPath}");
    }
}
#endif
