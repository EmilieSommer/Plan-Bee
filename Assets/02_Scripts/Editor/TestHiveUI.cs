using UnityEngine;
using UnityEditor;
using TMPro;

public class TestHiveUI : MonoBehaviour
{
    [MenuItem("Tools/Plan Bee/Test Hive UI Linking")]
    public static void Test()
    {
        HiveUI ui = FindObjectOfType<HiveUI>();
        if (ui == null)
        {
            Debug.LogError("HiveUI NOT FOUND!");
            return;
        }

        Debug.Log("HiveUI found on: " + ui.gameObject.name);

        TestLink("Queen", "queen");
        TestLink("Nurse", "nurse");
        TestLink("Builder", "builder");
        TestLink("House", "house", "worker");
        TestLink("Forager", "forager");
        TestLink("Drone", "drone");
        TestLink("Total", "total", "capacity");
    }

    private static void TestLink(string name, params string[] keywords)
    {
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
        
        foreach(var t in allTexts)
        {
            if (t.GetComponentInParent<UnityEngine.UI.Button>() != null) continue;
            string n = t.gameObject.name.ToLower();
            
            foreach (var keyword in keywords)
            {
                if (n.Contains(keyword) && int.TryParse(t.text.Trim(), out _))
                {
                    Debug.Log($"[1] SUCCESS for {name}: Found exact match -> {t.gameObject.name} on parent {t.transform.parent.name}");
                    return;
                }
            }
        }

        foreach(var t in allTexts)
        {
            if (t.GetComponentInParent<UnityEngine.UI.Button>() != null) continue;
            string n = t.gameObject.name.ToLower();
            
            foreach (var keyword in keywords)
            {
                if (n.Contains(keyword))
                {
                    foreach (Transform sibling in t.transform.parent)
                    {
                        TextMeshProUGUI siblingText = sibling.GetComponent<TextMeshProUGUI>();
                        if (siblingText != null && int.TryParse(siblingText.text.Trim(), out _))
                        {
                            Debug.Log($"[2] SUCCESS for {name}: Found sibling {sibling.name} via {t.gameObject.name} on parent {t.transform.parent.name}");
                            return;
                        }
                    }
                    Debug.Log($"[3] FALLBACK for {name}: Found title {t.gameObject.name} on parent {t.transform.parent.name}");
                    return;
                }
            }
        }
        Debug.LogError($"FAILED to find {name}!");
    }
}
