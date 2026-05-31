#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tools → Plan Bee → Organize Tiles
/// Moves tile sprites into per-type subfolders inside 05_Tiles.
/// Uses AssetDatabase.MoveAsset so all GUIDs/references stay intact.
/// </summary>
public static class TileOrganizer
{
    const string Root    = "Assets/05_Tiles";
    const string Tilemap = "Assets/05_Tiles/Tilemap";

    [MenuItem("Tools/Plan Bee/Organize Tiles")]
    static void Organize()
    {
        // ── Create destination folders ───────────────────────────────────────
        string[] folders =
        {
            Root + "/InsideHive",
            Root + "/Brood",
            Root + "/Solid",
            Root + "/Storage",
            Root + "/Outside",
            Root + "/_Archive",
        };
        foreach (var f in folders)
            if (!AssetDatabase.IsValidFolder(f))
                AssetDatabase.CreateFolder(Root, System.IO.Path.GetFileName(f));

        AssetDatabase.Refresh();

        // ── Move helpers ─────────────────────────────────────────────────────
        void Move(string src, string dst)
        {
            if (!System.IO.File.Exists(src)) return;
            string err = AssetDatabase.MoveAsset(src, dst);
            if (!string.IsNullOrEmpty(err))
                Debug.LogWarning($"[TileOrganizer] Could not move {src}: {err}");
        }

        // ── InsideHive ───────────────────────────────────────────────────────
        string[] insideSuffixes = { "01","02_T","03_B","04_L","05_R","06_TB","07_LR","08_TL" };
        foreach (var s in insideSuffixes)
            Move($"{Root}/Inside_Hive_{s}.png", $"{Root}/InsideHive/Inside_Hive_{s}.png");

        string[] insideTilemapSuffixes = { "09_TR","10_BL","11_BR","12_TBL","13_TBR","14_TLR","15_BLR","16_TBLR" };
        foreach (var s in insideTilemapSuffixes)
            Move($"{Tilemap}/Inside_Hive_{s}.png", $"{Root}/InsideHive/Inside_Hive_{s}.png");

        Move($"{Root}/InsideHive_RuleTile.asset", $"{Root}/InsideHive/InsideHive_RuleTile.asset");
        Move($"{Root}/Inside_Hive_01_0.asset",    $"{Root}/InsideHive/Inside_Hive_01_0.asset");

        // ── Brood ────────────────────────────────────────────────────────────
        string[] broodSuffixes = { "01","02_T","03_B","04_L","05_R","06_TB","07_LR","08_TL",
                                   "09_TR","10_BL","11_BR","12_TBL","13_TBR","14_TLR","15_BLR","16_TBLR" };
        foreach (var s in broodSuffixes)
            Move($"{Root}/Brood_{s}.png", $"{Root}/Brood/Brood_{s}.png");

        // ── Solid ────────────────────────────────────────────────────────────
        string[] solidSuffixes = { "01","02_T","03_B","04_L","05_R","06_TB","07_LR","08_TL",
                                   "09_TR","10_BL","11_BR","12_TBL","13_TBR","14_TLR","15_BLR","16_TBLR" };
        foreach (var s in solidSuffixes)
            Move($"{Tilemap}/Solid_{s}.png", $"{Root}/Solid/Solid_{s}.png");

        // ── Storage ──────────────────────────────────────────────────────────
        string[] storageSuffixes = { "01","02_T","03_B","04_L","05_R","06_TB","07_LR","08_TL",
                                     "09_TR","10_BL","11_BR","12_TBL","13_TBR","14_TLR","15_BLR","16_TBLR" };
        foreach (var s in storageSuffixes)
            Move($"{Tilemap}/Storage_{s}.png", $"{Root}/Storage/Storage_{s}.png");

        // ── Outside ──────────────────────────────────────────────────────────
        Move($"{Tilemap}/Outside.png", $"{Root}/Outside/Outside.png");

        // ── Archive (old/unused files) ───────────────────────────────────────
        Move($"{Root}/01_Inside_Hive.png",                    $"{Root}/_Archive/01_Inside_Hive.png");
        Move($"{Root}/Inside_Hive.png",                       $"{Root}/_Archive/Inside_Hive.png");
        Move($"{Root}/Inside_Hive.svg",                       $"{Root}/_Archive/Inside_Hive.svg");
        Move($"{Root}/InsideHive_Tilemap.aseprite",           $"{Root}/_Archive/InsideHive_Tilemap.aseprite");
        Move($"{Root}/InsideHive_Rounded_Preview.aseprite",   $"{Root}/_Archive/InsideHive_Rounded_Preview.aseprite");
        Move($"{Root}/InsideHive_Tilemap.asset",              $"{Root}/_Archive/InsideHive_Tilemap.asset");
        Move($"{Root}/colours",                               $"{Root}/_Archive/colours");

        // ── Leftover Tilemap folder PNGs (32x32 reference sheets) ────────────
        Move($"{Tilemap}/32x32 Tilemap.png",  $"{Root}/_Archive/32x32 Tilemap.png");
        Move($"{Tilemap}/32x32 Tilemap_.png", $"{Root}/_Archive/32x32 Tilemap_.png");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[TileOrganizer] Done! Tiles reorganized into subfolders.");
    }
}
#endif
