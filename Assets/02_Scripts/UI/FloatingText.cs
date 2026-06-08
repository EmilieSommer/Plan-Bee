using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float lifetime = 1.5f;
    
    private TextMeshPro textMesh;
    private Color originalColor;

    public void Setup(string text, Color color)
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null)
        {
            textMesh = gameObject.AddComponent<TextMeshPro>();
            textMesh.fontSize = 6;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.sortingOrder = 100; // Always on top of bees
        }
        textMesh.text = text;
        textMesh.color = color;
        originalColor = color;
        
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        if (textMesh != null)
        {
            float alpha = textMesh.color.a - (Time.deltaTime / lifetime);
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }
    }
    
    // Quick helper to spawn one anywhere
    public static void Create(Vector2 position, string text, Color color)
    {
        GameObject go = new GameObject("FloatingText");
        go.transform.position = position;
        FloatingText ft = go.AddComponent<FloatingText>();
        ft.Setup(text, color);
    }
}
