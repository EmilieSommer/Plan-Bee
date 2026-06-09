using UnityEngine;
using TMPro;
using System.Collections;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [Header("Resources")]
    public int pollen;
    public int maxPollen;
    public int honey;
    public int maxHoney;

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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Force starting honey to 0 so inspector values don't override it
        pollen = 0;
        maxPollen = 0;
        honey = 0; 
        maxHoney = 50;
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

    public void AddCapacity(int pollenCap, int honeyCap)
    {
        maxPollen += pollenCap;
        maxHoney += honeyCap;
        UpdateUI();
    }

    public void RemoveCapacity(int pollenCap, int honeyCap)
    {
        maxPollen = Mathf.Max(0, maxPollen - pollenCap);
        maxHoney = Mathf.Max(0, maxHoney - honeyCap);
        
        pollen = Mathf.Min(pollen, maxPollen);
        honey = Mathf.Min(honey, maxHoney);
        UpdateUI();
    }

    public bool HasPollenSpace() => pollen < maxPollen;
    public bool HasHoneySpace() => true; // No cap for honey!

    // -------------------------
    // HONEY
    // -------------------------
    public bool UseHoney(int amount)
    {
        if (honey < amount)
        {
            TriggerNotEnoughResource("Not enough honey!");
            return false;
        }

        honey -= amount;
        UpdateUI();
        return true;
    }

    public void AddHoney(int amount)
    {
        honey += amount; // Removed maxHoney limit
        UpdateUI();
    }

    // -------------------------
    // POLLEN
    // -------------------------
    public bool UsePollen(int amount)
    {
        if (pollen < amount)
        {
            // Silently fail, as HouseBees check this automatically
            return false;
        }

        pollen -= amount;
        UpdateUI();
        return true;
    }

    public void AddPollen(int amount)
    {
        pollen = Mathf.Min(pollen + amount, maxPollen);
        UpdateUI();
    }

    // -------------------------
    // UI
    // -------------------------
    void UpdateUI()
    {
        if (pollenText == null || honeyText == null)
        {
            var allTexts = FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var t in allTexts)
            {
                if (pollenText == null && t.name.ToLower().Contains("pollen"))
                    pollenText = t;
                if (honeyText == null && t.name.ToLower().Contains("honey"))
                    honeyText = t;
            }
        }

        if (pollenText != null) pollenText.text = $"Pollen: {pollen} / {maxPollen}";
        if (honeyText != null) honeyText.text = $"{honey}";
    }

    // -------------------------
    // WARNING SYSTEM
    // -------------------------
    void TriggerNotEnoughResource(string message)
    {
        ShowWarning();

        if (UIMessagePopup.Instance != null)
            UIMessagePopup.Instance.ShowMessage(message);
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

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            warningPanel.alpha = t / fadeDuration;
            yield return null;
        }

        warningPanel.alpha = 1;
        yield return new WaitForSeconds(showDuration);

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