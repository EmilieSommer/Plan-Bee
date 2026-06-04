#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class KristofferSetup : MonoBehaviour
{
    [MenuItem("Plan Bee/2. Restore Kristoffer's Layout & Remove Grey UI")]
    public static void RestoreAndUpgrade()
    {
        // 1. Remove the ugly grey background from BuildPanel
        GameObject buildPanel = GameObject.Find("BuildPanel");
        if (buildPanel != null)
        {
            Image bgImage = buildPanel.GetComponent<Image>();
            if (bgImage != null)
            {
                DestroyImmediate(bgImage);
                Debug.Log("Removed the ugly grey background from BuildPanel!");
            }
        }

        // 2. Remove Emilie's generated Grid
        GameObject emilieGrid = GameObject.Find("Grid_NewTileSystem");
        if (emilieGrid != null)
        {
            DestroyImmediate(emilieGrid);
            Debug.Log("Removed Emilie's generated Grid.");
        }

        // 3. Upgrade Kristoffer's original Grid to support the new Tile logic
        GameObject kGrid = GameObject.Find("Old_Grid_DoNotDelete");
        if (kGrid == null) kGrid = GameObject.Find("Grid");

        if (kGrid != null)
        {
            kGrid.name = "Grid"; // Ensure it's named Grid

            // Add the scripts if they don't exist
            HiveVisuals visuals = kGrid.GetComponent<HiveVisuals>();
            if (visuals == null) visuals = kGrid.AddComponent<HiveVisuals>();

            HiveGrid hiveGrid = kGrid.GetComponent<HiveGrid>();
            if (hiveGrid == null) hiveGrid = kGrid.AddComponent<HiveGrid>();

            // Find Kristoffer's hand-painted Tilemap
            Tilemap kTilemap = kGrid.GetComponentInChildren<Tilemap>();
            if (kTilemap != null)
            {
                SerializedObject so = new SerializedObject(visuals);
                so.Update();
                
                so.FindProperty("builtTilemap").objectReferenceValue = kTilemap;
                
                // Assign the library
                string[] guids = AssetDatabase.FindAssets("t:HiveTileLibrary");
                if (guids.Length > 0)
                {
                    so.FindProperty("library").objectReferenceValue = AssetDatabase.LoadAssetAtPath<HiveTileLibrary>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }

                so.ApplyModifiedProperties();

                // Generate the 4 Overlay layers automatically
                SetupOverlayTilemaps(kGrid, visuals);
                
                Debug.Log("<color=green><b>SUCCESS!</b> Kristoffer's Grid is now fully upgraded with the new logic, but keeps your original hand-painted layout!</color>");
            }
            else
            {
                Debug.LogError("Could not find a Tilemap inside Kristoffer's Grid!");
            }
        }
    }

    private static void SetupOverlayTilemaps(GameObject grid, HiveVisuals visuals)
    {
        var overlayMaps = new Tilemap[4];
        for (int i = 0; i < 4; i++)
        {
            string layerName = "OverlayTilemap_" + i;
            Transform overlayObj = grid.transform.Find(layerName);
            if (overlayObj == null)
            {
                GameObject go = new GameObject(layerName);
                go.transform.SetParent(grid.transform);
                go.transform.localPosition = Vector3.zero;
                overlayMaps[i] = go.AddComponent<Tilemap>();
                TilemapRenderer renderer = go.AddComponent<TilemapRenderer>();
                renderer.sortingOrder = 10 + i;
            }
            else
            {
                overlayMaps[i] = overlayObj.GetComponent<Tilemap>();
            }
        }

        SerializedObject so = new SerializedObject(visuals);
        so.Update();
        SerializedProperty arrayProp = so.FindProperty("overlayTilemaps");
        arrayProp.arraySize = 4;
        for (int i = 0; i < 4; i++)
        {
            arrayProp.GetArrayElementAtIndex(i).objectReferenceValue = overlayMaps[i];
        }
        so.ApplyModifiedProperties();
    }
}
#endif
