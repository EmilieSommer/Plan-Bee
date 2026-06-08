#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class PauseMenuBuilder
{
    [MenuItem("Tools/Plan Bee/Build Pause Menu")]
    public static void Build()
    {
        var canvasGO = GameObject.Find("MainCanvass");
        if (canvasGO == null)
        {
            EditorUtility.DisplayDialog("Pause Menu Builder",
                "Could not find a GameObject named 'MainCanvass' in the scene. Open K-MainScene first.", "OK");
            return;
        }

        // Replace if it exists
        var existing = GameObject.Find("PauseMenuPanel");
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("Pause Menu Builder",
                "PauseMenuPanel already exists. Replace it?", "Replace", "Cancel"))
                return;
            Object.DestroyImmediate(existing);
        }

        // Load assets
        var cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/04_Sprites/UI/cards/Card_medium.png");
        var smallCardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/04_Sprites/UI/cards/Card_small.png");
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/PixelifySans-Regular SDF.asset");
        var fontBoldAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/PixelifySans-Bold SDF.asset");
        if (fontAsset == null) fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (fontBoldAsset == null) fontBoldAsset = fontAsset;

        // === Root: full-screen overlay ===
        var root = new GameObject("PauseMenuPanel", typeof(RectTransform));
        root.transform.SetParent(canvasGO.transform, false);
        var rt = root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // === Dim background ===
        var dim = NewChild(root, "Dim");
        var dimRt = dim.AddComponent<RectTransform>();
        Stretch(dimRt);
        var dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.55f);
        dimImg.raycastTarget = true; // catches clicks

        // === Panel ===
        var panel = NewChild(root, "Panel");
        var panelRt = panel.AddComponent<RectTransform>();
        panelRt.sizeDelta = new Vector2(420, 540);
        panelRt.anchoredPosition = Vector2.zero;
        var panelImg = panel.AddComponent<Image>();
        if (cardSprite != null) { panelImg.sprite = cardSprite; panelImg.type = Image.Type.Sliced; }
        else panelImg.color = new Color(0.95f, 0.85f, 0.55f);

        // === Title "PAUSED" ===
        var title = NewChild(panel, "Title");
        var titleRt = title.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0, -50);
        titleRt.sizeDelta = new Vector2(360, 60);
        var titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "PAUSED";
        titleText.font = fontBoldAsset;
        titleText.fontSize = 44;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.4f, 0.2f, 0.1f);

        // === Buttons stacked ===
        string[] labels = { "RESUME", "RESTART", "MAIN MENU" };
        string[] methods = { "Resume", "Restart", "GoToMainMenu" };
        float startY = -150;
        float spacing = 90;

        var controllerGO = canvasGO.GetComponent<PauseMenuController>();
        if (controllerGO == null) controllerGO = canvasGO.AddComponent<PauseMenuController>();
        controllerGO.pauseMenuPanel = root;

        for (int i = 0; i < labels.Length; i++)
        {
            var btnGO = NewChild(panel, labels[i] + "Button");
            var btnRt = btnGO.AddComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 1f);
            btnRt.anchorMax = new Vector2(0.5f, 1f);
            btnRt.pivot = new Vector2(0.5f, 1f);
            btnRt.anchoredPosition = new Vector2(0, startY - i * spacing);
            btnRt.sizeDelta = new Vector2(300, 70);
            var btnImg = btnGO.AddComponent<Image>();
            if (smallCardSprite != null) { btnImg.sprite = smallCardSprite; btnImg.type = Image.Type.Sliced; }
            else btnImg.color = new Color(0.7f, 0.55f, 0.3f);
            var btn = btnGO.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = colors;

            // Button label
            var labelGO = NewChild(btnGO, "Label");
            var labelRt = labelGO.AddComponent<RectTransform>();
            Stretch(labelRt);
            var labelText = labelGO.AddComponent<TextMeshProUGUI>();
            labelText.text = labels[i];
            labelText.font = fontAsset;
            labelText.fontSize = 28;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = new Color(0.3f, 0.15f, 0.05f);

            // Wire button onClick → controller method via SetPersistentListener
            var method = controllerGO.GetType().GetMethod(methods[i]);
            if (method != null)
            {
                var action = (UnityAction)System.Delegate.CreateDelegate(typeof(UnityAction), controllerGO, method);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, action);
            }
        }

        root.SetActive(false);

        // Mark dirty so scene saves
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Selection.activeGameObject = root;
        Debug.Log("[PauseMenuBuilder] Pause menu built. Press ESC during play to test.");
        EditorUtility.DisplayDialog("Pause Menu Builder",
            "Done! Pause menu added to K-MainScene. Save the scene (Cmd+S) and press Play.\n\nESC will toggle it.", "OK");
    }

    private static GameObject NewChild(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    private static GameObject NewChild(Component parent, string name) =>
        NewChild(parent.gameObject, name);

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
#endif
