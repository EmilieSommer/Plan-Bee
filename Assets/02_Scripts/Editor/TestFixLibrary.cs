using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TestFixLibrary : MonoBehaviour
{
    [MenuItem("Plan Bee/Test Tile Library Matches")]
    public static void TestMatches()
    {
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/05_Tiles" });
        var sprites = new List<Sprite>();
        foreach (var g in guids)
        {
            var sp = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(g));
            if (sp != null) sprites.Add(sp);
        }

        foreach (HiveTileType t in System.Enum.GetValues(typeof(HiveTileType)))
        {
            if (t == HiveTileType.None || t == HiveTileType.Solid) continue;

            string folderPath = "/05_Tiles/" + t.ToString() + "/";
            Debug.Log($"Testing type {t} with folderPath {folderPath}");

            string borderName = "Corner_TopLeft";
            var foundBorder = sprites.Find(s => s.name == borderName && AssetDatabase.GetAssetPath(s).Contains(folderPath) && !AssetDatabase.GetAssetPath(s).ToLower().Contains("overlay"));
            if (foundBorder != null)
                Debug.Log($"[{t}] Found Border: {foundBorder.name} at {AssetDatabase.GetAssetPath(foundBorder)}");
            else
                Debug.LogWarning($"[{t}] FAILED to find border {borderName}");
                
            string overlayName = "Corner_TopLeft";
            var foundOverlay = sprites.Find(s => (s.name == overlayName || s.name.EndsWith("_" + overlayName)) && AssetDatabase.GetAssetPath(s).Contains(folderPath) && AssetDatabase.GetAssetPath(s).ToLower().Contains("overlay"));
            if (foundOverlay != null)
                Debug.Log($"[{t}] Found Overlay: {foundOverlay.name} at {AssetDatabase.GetAssetPath(foundOverlay)}");
            else
                Debug.LogWarning($"[{t}] FAILED to find overlay {overlayName}");
        }
    }
}
