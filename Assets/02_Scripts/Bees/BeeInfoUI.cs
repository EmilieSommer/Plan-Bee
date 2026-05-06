using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BeeInfoUI : MonoBehaviour
{
    public static BeeInfoUI Instance;

    [Header("UI")]
    public GameObject panel;

    public TMP_Text typeText;
    public TMP_Text nameText;
    public TMP_Text speedText;

    [Header("Hearts (assign in Inspector)")]
    public Image[] hearts;

    private Bee currentBee;
    private int lastHealth = -1;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    void Update()
    {
        if (currentBee == null || !panel.activeSelf)
            return;

        int health = Mathf.RoundToInt(currentBee.CurrentHealth);

        if (health != lastHealth)
        {
            lastHealth = health;
            UpdateHearts(health);
        }
    }

    public void Show(Bee bee)
    {
        currentBee = bee;
        panel.SetActive(true);

        typeText.text = $"Type: {bee.beeType}";
        nameText.text = $"Name: {bee.beeName}";
        speedText.text = $"Speed: {bee.moveSpeed}";

        lastHealth = -1;

        UpdateHearts(Mathf.RoundToInt(bee.CurrentHealth));
    }

    public void Hide()
    {
        panel.SetActive(false);
        currentBee = null;
    }

    void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i < currentHealth;
        }
    }
}