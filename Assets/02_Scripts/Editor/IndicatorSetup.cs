#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class IndicatorSetup : MonoBehaviour
{
    [MenuItem("Plan Bee/4. Setup Build Indicators")]
    public static void SetupIndicators()
    {
        GameObject grid = GameObject.Find("Grid");
        if (grid == null)
        {
            Debug.LogError("Could not find Grid!");
            return;
        }

        HiveVisuals visuals = grid.GetComponent<HiveVisuals>();
        if (visuals == null) return;

        // 1. Create the Indicator Tilemap
        string indicatorLayer = "IndicatorTilemap";
        Transform indicatorObj = grid.transform.Find(indicatorLayer);
        Tilemap iMap;
        if (indicatorObj == null)
        {
            GameObject go = new GameObject(indicatorLayer);
            go.transform.SetParent(grid.transform);
            go.transform.localPosition = Vector3.zero;
            iMap = go.AddComponent<Tilemap>();
            TilemapRenderer renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = 50; 
            iMap.color = new Color(1f, 1f, 0f, 0.5f); 
        }
        else iMap = indicatorObj.GetComponent<Tilemap>();

        // 2. Create the Marked Tilemap (for construction ghosts)
        string markedLayer = "MarkedTilemap";
        Transform markedObj = grid.transform.Find(markedLayer);
        Tilemap mMap;
        if (markedObj == null)
        {
            GameObject go = new GameObject(markedLayer);
            go.transform.SetParent(grid.transform);
            go.transform.localPosition = Vector3.zero;
            mMap = go.AddComponent<Tilemap>();
            TilemapRenderer renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = 40; 
        }
        else mMap = markedObj.GetComponent<Tilemap>();

        // 3. Assign to HiveVisuals
        SerializedObject so = new SerializedObject(visuals);
        so.Update();
        so.FindProperty("indicatorTilemap").objectReferenceValue = iMap;
        so.FindProperty("markedTilemap").objectReferenceValue = mMap;
        
        so.ApplyModifiedProperties();

        Debug.Log("<color=green><b>SUCCESS!</b> Added Indicator and Marked layers. You can change their icons on the Grid's HiveVisuals script!</color>");
    }
}
#endif
