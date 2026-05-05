using UnityEngine;
using TMPro;
using System.Collections;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [Header("Resources")]
    public int pollen;
    public int honey;

    [Header("UI")]
    public TextMeshProUGUI pollenText;
    public TextMeshProUGUI honeyText;

    [Header("Not Enough Money Feedback")]
    public CanvasGroup warningPanel;
    public float fadeDuration = 0.5f;
    public float showDuration = 1.5f;

    public AudioSource audioSource;
    public AudioClip warningSound;

    private Coroutine warningCoroutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateUI();

        if (warningPanel != null)
        {
            warningPanel.alpha = 0;
            warningPanel.gameObject.SetActive(false);
        }
    }

    public bool UseHoney(int amount)
    {
        if (honey < amount)
        {
            ShowWarning();
            return false;
        }

        honey -= amount;
        UpdateUI();
        return true;
    }

    public void AddPollen(int amount)
    {
        pollen += amount;
        UpdateUI();
    }

    public void AddHoney(int amount)
    {
        honey += amount;
        UpdateUI();
    }

    public bool UsePollen(int amount)
    {
        if (pollen < amount)
        {
            ShowWarning();
            return false;
        }

        pollen -= amount;
        UpdateUI();
        return true;
    }

    void UpdateUI()
    {
        if (pollenText != null)
            pollenText.text = "Pollen: " + pollen;

        if (honeyText != null)
            honeyText.text = "Honey: " + honey;
    }

    void ShowWarning()
    {
        if (warningCoroutine != null)
            StopCoroutine(warningCoroutine);

        warningCoroutine = StartCoroutine(FadeWarning());

        if (audioSource != null && warningSound != null)
            audioSource.PlayOneShot(warningSound);
    }

    IEnumerator FadeWarning()
    {
        warningPanel.gameObject.SetActive(true);

        // Fade in
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            warningPanel.alpha = t / fadeDuration;
            yield return null;
        }

        warningPanel.alpha = 1;

        yield return new WaitForSeconds(showDuration);

        // Fade out
        t = fadeDuration;
        while (t > 0)
        {
            t -= Time.deltaTime;
            warningPanel.alpha = t / fadeDuration;
            yield return null;
        }

        warningPanel.alpha = 0;
        warningPanel.gameObject.SetActive(false);
    }
}