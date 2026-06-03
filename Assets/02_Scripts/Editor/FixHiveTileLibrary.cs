using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class FixHiveTileLibrary : EditorWindow
{
    [MenuItem("Plan Bee/1. Fix Tile Library")]
    public static void FixLibrary()
    {
        string[] guids = AssetDatabase.FindAssets("t:HiveTileLibrary");
        if (guids.Length == 0) return;
        var library = AssetDatabase.LoadAssetAtPath<HiveTileLibrary>(AssetDatabase.GUIDToAssetPath(guids[0]));

        string[] borderNames = {
            "Isolated", "DeadEnd_Top", "DeadEnd_Bottom", "Tunnel_Vertical",
            "DeadEnd_Left", "Corner_BottomRight", "Corner_TopRight", "Wall_Right",
            "DeadEnd_Right", "Corner_BottomLeft", "Corner_TopLeft", "Wall_Left",
            "Tunnel_Horizontal", "Wall_Bottom", "Wall_Top", "Center"
        };

        string[] overlayNames = {
            "Center", "Wall_Top", "Wall_Bottom", "Tunnel_Horizontal",
            "Wall_Left", "Corner_TopLeft", "Corner_BottomLeft", "DeadEnd_Right",
            "Wall_Right", "Corner_TopRight", "Corner_BottomRight", "DeadEnd_Left",
            "Tunnel_Vertical", "DeadEnd_Bottom", "DeadEnd_Top", "Isolated"
        };

        string[] allSpriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/05_Tiles" });
        var allSprites = new List<Sprite>();
        foreach (var g in allSpriteGuids) allSprites.Add(AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(g)));

        foreach (var set in library.sets)
        {
            if (set.border == null || set.border.Length != 16) set.border = new Sprite[16];
            if (set.overlay == null || set.overlay.Length != 16) set.overlay = new Sprite[16];

            string folderHint = set.type.ToString();
            if (folderHint == "InsideHive") folderHint = "Inside";

            for (int i = 0; i < 16; i++)
            {
                // Find border sprite
                string targetBorder = borderNames[i];
                Sprite borderSprite = allSprites.Find(s => 
                    s.name == targetBorder && 
                    AssetDatabase.GetAssetPath(s).Contains(folderHint) && 
                    !AssetDatabase.GetAssetPath(s).ToLower().Contains("overlay")
                );
                set.border[i] = borderSprite;

                // Find overlay sprite
                string targetOverlay = overlayNames[i];
                Sprite overlaySprite = allSprites.Find(s => 
                    (s.name == targetOverlay || s.name.EndsWith("_" + targetOverlay)) && 
                    AssetDatabase.GetAssetPath(s).Contains(folderHint) && 
                    AssetDatabase.GetAssetPath(s).ToLower().Contains("overlay")
                );
                set.overlay[i] = overlaySprite;
            }
        }

        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();
        Debug.Log("Hive Tile Library has been automatically populated with the perfect mapping!");
    }
}
