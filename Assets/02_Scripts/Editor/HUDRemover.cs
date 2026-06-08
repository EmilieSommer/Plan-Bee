#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class HUDRemover
{
    [MenuItem("Tools/Plan Bee/Remove Auto-Built HUD")]
    public static void Remove()
    {
        var hud = GameObject.Find("HUD");
        if (hud != null)
        {
            Object.DestroyImmediate(hud);
            Debug.Log("[HUDRemover] Removed HUD GameObject from scene.");
        }

        var pauseMenu = GameObject.Find("PauseMenuPanel");
        if (pauseMenu != null)
        {
            Object.DestroyImmediate(pauseMenu);
            Debug.Log("[HUDRemover] Removed PauseMenuPanel GameObject from scene.");
        }

        // Detach controllers we may have added
        var canvas = GameObject.Find("MainCanvass");
        if (canvas != null)
        {
            var hudCtrl = canvas.GetComponent<HUDController>();
            if (hudCtrl != null) Object.DestroyImmediate(hudCtrl);
            var pauseCtrl = canvas.GetComponent<PauseMenuController>();
            if (pauseCtrl != null) Object.DestroyImmediate(pauseCtrl);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("HUD Remover",
            "Removed any auto-built HUD / Pause Menu objects.\n\n" +
            "Save the scene (Cmd+S) to make it permanent.", "OK");
    }
}
#endif
