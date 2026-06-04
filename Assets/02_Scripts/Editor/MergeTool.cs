#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MergeTool : MonoBehaviour
{
    [MenuItem("Plan Bee/1. Merge Emilie's Tiles into K-MainScene")]
    public static void MergeScenes()
    {
        // 1. Ensure we are in K-MainScene
        Scene currentScene = EditorSceneManager.GetActiveScene();
        if (!currentScene.name.Contains("K-MainScene"))
        {
            EditorUtility.DisplayDialog("Error", "Please open K-MainScene before running this!", "OK");
            return;
        }

        // 2. Open E-MainScene in the background
        Scene eScene = EditorSceneManager.OpenScene("Assets/01_Scenes/E-MainScene.unity", OpenSceneMode.Additive);

        // 3. Find Emilie's Grid and BuildPanel
        GameObject gridToCopy = null;
        GameObject buildPanelToCopy = null;

        foreach (GameObject root in eScene.GetRootGameObjects())
        {
            if (root.name == "Grid") gridToCopy = root;
            if (root.name == "Canvas")
            {
                Transform bp = root.transform.Find("BuildPanel");
                if (bp != null) buildPanelToCopy = bp.gameObject;
            }
        }

        // 4. Bring the Grid over
        if (gridToCopy != null)
        {
            // Protect Kristoffer's old Grid (it holds the camera!)
            GameObject oldGrid = GameObject.Find("Grid");
            if (oldGrid != null && oldGrid.scene == currentScene)
            {
                oldGrid.name = "Old_Grid_DoNotDelete";
            }
            
            // Move Emilie's Grid to K-MainScene
            SceneManager.MoveGameObjectToScene(gridToCopy, currentScene);
            gridToCopy.name = "Grid_NewTileSystem";
        }

        // 5. Bring the BuildPanel over
        if (buildPanelToCopy != null)
        {
            // Find Kristoffer's Canvas
            GameObject kCanvas = GameObject.Find("MainCanvass");
            if (kCanvas == null) kCanvas = GameObject.Find("Canvas");

            if (kCanvas != null)
            {
                // Instantiate a copy of Emilie's BuildPanel into Kristoffer's Canvas
                GameObject newBP = Instantiate(buildPanelToCopy, kCanvas.transform);
                newBP.name = "BuildPanel";
                // Move it to the top of the canvas so it doesn't block other UI
                newBP.transform.SetAsFirstSibling(); 
            }
        }

        // 6. Close E-MainScene
        EditorSceneManager.CloseScene(eScene, true);
        EditorSceneManager.MarkSceneDirty(currentScene);

        Debug.Log("<color=green><b>MERGE COMPLETE!</b> Emilie's Tile System and UI have been successfully imported into K-MainScene!</color>");
    }
}
#endif
