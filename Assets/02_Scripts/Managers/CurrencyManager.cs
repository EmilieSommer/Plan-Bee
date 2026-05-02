using UnityEngine;
using TMPro; // if you're using TextMeshPro

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [Header("Resources")]
    public int pollen;
    public int honey;

    [Header("UI")]
    public TextMeshProUGUI pollenText;
    public TextMeshProUGUI honeyText;

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
    }

    public bool UseHoney(int amount)
    {
        if (honey < amount) return false;

        honey -= amount;
        UpdateUI();
        return true;
    }

    // -------------------
    // ADD METHODS
    // -------------------

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

    // -------------------
    // REMOVE METHODS
    // -------------------

    public bool UsePollen(int amount)
    {
        if (pollen < amount) return false;

        pollen -= amount;
        UpdateUI();
        return true;
    }

    // -------------------
    // UI
    // -------------------

    void UpdateUI()
    {
        if (pollenText != null)
            pollenText.text = "Pollen: " + pollen;

        if (honeyText != null)
            honeyText.text = "Honey: " + honey;
    }
}