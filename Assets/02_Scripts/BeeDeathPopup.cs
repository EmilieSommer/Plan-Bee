using UnityEngine;
using TMPro;
using System.Collections;

public class BeeDeathPopup : MonoBehaviour
{
    public static BeeDeathPopup Instance;

    public GameObject panel;
    public TextMeshProUGUI messageText;
    public CanvasGroup canvasGroup;

    [Header("Fade Settings")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.5f;

    private Coroutine hideRoutine;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);

        if (canvasGroup == null)
            canvasGroup = panel.GetComponent<CanvasGroup>();
    }

    public void ShowDeath(string beeType, string beeName, string killedBy, float duration = 3f)
    {
        ShowMessage($"{beeType} \"{beeName}\" was killed by {killedBy}", duration);
    }

    public void ShowMessage(string message, float duration = 3f)
    {
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        messageText.text = message;
        panel.SetActive(true);

        hideRoutine = StartCoroutine(FadeRoutine(duration));
    }

    private IEnumerator FadeRoutine(float duration)
    {
        // Fade in
        float t = 0f;
        canvasGroup.alpha = 0f;

        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;

        // Hold
        yield return new WaitForSeconds(duration);

        // Fade out
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - (t / fadeOutDuration));
            yield return null;
        }

        canvasGroup.alpha = 0f;
        panel.SetActive(false);
    }
}