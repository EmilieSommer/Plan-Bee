using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class AutoBuildUI : EditorWindow
{
    [MenuItem("Plan Bee/Fix Spawn Menu")]
    public static void BuildSpawnMenu()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found!");
            return;
        }

        SpawnPanel spawnPanel = FindObjectOfType<SpawnPanel>();
        if (spawnPanel == null)
        {
            Debug.LogError("Could not find SpawnPanel! Make sure it is attached to your bee_button.");
            return;
        }

        // Check if it already exists
        Transform existing = canvas.transform.Find("SpawnSubmenu");
        if (existing != null)
        {
            DestroyImmediate(existing.gameObject);
        }

        // Create Panel
        GameObject panelObj = new GameObject("SpawnSubmenu");
        panelObj.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(200, 300);
        panelRect.anchoredPosition = new Vector2(0, 0);

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        VerticalLayoutGroup layout = panelObj.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 10;
        layout.padding = new RectOffset(20, 20, 20, 20);

        // Create Buttons
        CreateBeeButton("Spawn Forager (10)", panelObj.transform, spawnPanel.SpawnForager, spawnPanel);
        CreateBeeButton("Spawn Nurse (5)", panelObj.transform, spawnPanel.SpawnNurse, spawnPanel);
        CreateBeeButton("Spawn House (5)", panelObj.transform, spawnPanel.SpawnHouse, spawnPanel);
        CreateBeeButton("Spawn Builder (15)", panelObj.transform, spawnPanel.SpawnBuilder, spawnPanel);
        CreateBeeButton("Spawn Drone (20)", panelObj.transform, spawnPanel.SpawnDrone, spawnPanel);

        // Assign to script and hide
        spawnPanel.spawnSubmenu = panelObj;
        panelObj.SetActive(false);

        // Force Unity to save the changes
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        
        Debug.Log("Successfully built the Spawn Menu and linked it to your bee_button!");
    }

    private static void CreateBeeButton(string labelText, Transform parent, UnityEngine.Events.UnityAction action, SpawnPanel targetScript)
    {
        GameObject buttonObj = new GameObject(labelText + " Button");
        buttonObj.transform.SetParent(parent, false);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.8f, 0.6f, 0.2f, 1f);

        Button btn = buttonObj.AddComponent<Button>();
        
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, action);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = labelText;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 18;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        LayoutElement le = buttonObj.AddComponent<LayoutElement>();
        le.minHeight = 40;
    }
}
