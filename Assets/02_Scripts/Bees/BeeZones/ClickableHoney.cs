using UnityEngine;
using System.Collections;

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

        StartCoroutine(SpawnJuice());
    }

    private IEnumerator SpawnJuice()
    {
        Vector3 originalScale = transform.localScale;

        // 1. Scale pop (juice)
        float elapsed = 0f;
        float duration = 0.15f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Pop up to 1.4x scale, then settle back
            float scaleMult = Mathf.Lerp(1.4f, 1.0f, t);
            transform.localScale = originalScale * scaleMult;
            yield return null;
        }
        transform.localScale = originalScale;

        // 2. White flash overlay for 2 seconds
        if (sr != null && sr.sprite != null)
        {
            GameObject flashGO = new GameObject("FlashOverlay");
            flashGO.transform.SetParent(transform);
            flashGO.transform.localPosition = Vector3.zero;
            flashGO.transform.localScale = Vector3.one;

            SpriteRenderer flashSr = flashGO.AddComponent<SpriteRenderer>();
            flashSr.sprite = sr.sprite;
            flashSr.sortingOrder = sr.sortingOrder + 1; // On top of honey

            // Use GUI/Text Shader to render it solid white
            Shader guiTextShader = Shader.Find("GUI/Text Shader");
            if (guiTextShader != null)
            {
                flashSr.material = new Material(guiTextShader);
            }
            flashSr.color = Color.white;

            // Stay solid white for 1.7 seconds, then fade out for 0.3 seconds
            yield return new WaitForSeconds(1.7f);

            float fadeElapsed = 0f;
            float fadeDuration = 0.3f;
            while (fadeElapsed < fadeDuration)
            {
                fadeElapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, fadeElapsed / fadeDuration);
                flashSr.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }

            Destroy(flashGO);
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

    public void Collect()
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
