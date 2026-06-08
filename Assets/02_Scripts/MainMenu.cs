using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;
    public string gameSceneName = "K-MainScene";

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    public void StartGame()
    {
        StartCoroutine(FadeOutAndLoad());
    }

    IEnumerator FadeIn()
    {
        fadePanel.blocksRaycasts = true;
        fadePanel.alpha = 1f;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadePanel.alpha = 1f - (t / fadeDuration);
            yield return null;
        }
        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = false;
    }

    IEnumerator FadeOutAndLoad()
    {
        fadePanel.blocksRaycasts = true;
        fadePanel.alpha = 0f;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadePanel.alpha = t / fadeDuration;
            yield return null;
        }
        fadePanel.alpha = 1f;
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }
}