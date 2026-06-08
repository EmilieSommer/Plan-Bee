using UnityEngine;
using System.Collections;

public class GameSceneFader : MonoBehaviour
{
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        fadePanel.alpha = 1f;
        fadePanel.blocksRaycasts = true;   // block during fade in

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.alpha = 1f - (t / fadeDuration);
            yield return null;
        }

        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = false;  // ⭐ THIS fixes your issue
    }
}