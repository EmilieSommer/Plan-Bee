using UnityEngine;

public class ClickableHoney : MonoBehaviour
{
    public int amount = 1;
    
    private SpriteRenderer sr;
    private GameObject outlineObj;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Create a duplicate sprite renderer behind it, slightly larger and white
            outlineObj = new GameObject("Outline");
            outlineObj.transform.SetParent(transform);
            outlineObj.transform.localPosition = Vector3.zero;
            outlineObj.transform.localScale = new Vector3(1.15f, 1.15f, 1f);
            
            SpriteRenderer outlineSr = outlineObj.AddComponent<SpriteRenderer>();
            outlineSr.sprite = sr.sprite;
            outlineSr.color = Color.white;
            outlineSr.sortingOrder = sr.sortingOrder - 1; // Put behind honey
        }
    }

    private float lifetime = 30f;

    private void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Collect();
            return;
        }

        // Make the outline pulsate to catch the player's eye
        if (outlineObj != null)
        {
            float alpha = 0.5f + Mathf.Sin(Time.time * 5f) * 0.5f; // Pulsate between 0 and 1
            SpriteRenderer outlineSr = outlineObj.GetComponent<SpriteRenderer>();
            Color c = outlineSr.color;
            c.a = alpha;
            outlineSr.color = c;
        }
    }

    private void OnMouseDown()
    {
        Collect();
    }

    private void Collect()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayHoneyCollect();

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddHoney(amount);
            FloatingText.Create(transform.position, $"+{amount} Honey", new Color(1f, 0.8f, 0.2f));
        }
        Destroy(gameObject);
    }
}
