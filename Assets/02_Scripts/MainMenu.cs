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
        fadePanel.alpha = 1f;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.alpha = 1f - (t / fadeDuration);
            yield return null;
        }
        fadePanel.alpha = 0f;
    }

    IEnumerator FadeOutAndLoad()
    {
        fadePanel.alpha = 0f;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.alpha = t / fadeDuration;
            yield return null;
        }
        fadePanel.alpha = 1f;
        SceneManager.LoadScene(gameSceneName);
    }
}