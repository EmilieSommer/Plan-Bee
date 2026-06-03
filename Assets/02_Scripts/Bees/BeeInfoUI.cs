using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BeeInfoUI : MonoBehaviour
{
    public static BeeInfoUI Instance;

    [Header("UI")]
    public GameObject panel;

    public TMP_Text typeText;
    public TMP_Text nameText;
    public TMP_Text speedText;

    [Header("Hearts")]
    public Image[] hearts;

    [Header("Health Settings")]
    public int maxLives = 10;

    [Header("Heal Settings")]
    public int healCost = 5;
    public int healAmount = 2;

    [Header("Speed Settings")]
    public int speedCost = 5;
    public float speedUpgradeAmount = 0.5f;
    public float maxSpeed = 6f;

    private Bee currentBee;
    private int lastHealth = -1;
    private float lastSpeed = -1f;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    void Update()
    {
        // Close if bee died or was destroyed
        if (panel.activeSelf && (currentBee == null || currentBee.CurrentHealth <= 0f))
        {
            Close();
            return;
        }

        HandleAutoClose();

        if (!panel.activeSelf || currentBee == null)
            return;

        int health = Mathf.RoundToInt(currentBee.CurrentHealth);
        if (health != lastHealth)
        {
            lastHealth = health;
            UpdateHearts(health);
        }

        if (currentBee.moveSpeed != lastSpeed)
        {
            lastSpeed = currentBee.moveSpeed;
            speedText.text = $"Speed: {currentBee.moveSpeed:F1}";
        }
    }

    // ======================================================
    // OPEN
    // ======================================================
    public void Open(Bee bee)
    {
        currentBee = bee;

        panel.SetActive(true);

        typeText.text  = $"Type: {bee.beeType}";
        nameText.text  = $"Name: {bee.beeName}";
        speedText.text = $"Speed: {bee.moveSpeed:F1}";

        lastHealth = -1;
        lastSpeed  = -1f;
        UpdateHearts(Mathf.RoundToInt(bee.CurrentHealth));
    }

    // ======================================================
    // CLOSE
    // ======================================================
    public void Close()
    {
        panel.SetActive(false);
        currentBee = null;
    }

    // ======================================================
    // CLICK OUTSIDE TO CLOSE
    // ======================================================
    void HandleAutoClose()
    {
        if (!panel.activeSelf)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current == null)
                return;

            if (EventSystem.current.IsPointerOverGameObject())
                return;

            Close();
        }
    }

    // ======================================================
    // HEART UPDATE
    // ======================================================
    void UpdateHearts(int currentHealth)
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxLives);

        for (int i = 0; i < hearts.Length; i++)
            hearts[i].enabled = i < currentHealth;
    }

    // ======================================================
    // HEAL BUTTON
    // ======================================================
    public void BuyHeal()
    {
        if (currentBee == null)
            return;

        int currentHealth = Mathf.RoundToInt(currentBee.CurrentHealth);

        if (currentHealth >= maxLives)
            return;

        if (!CurrencyManager.Instance.UseHoney(healCost))
            return;

        int newHealth = Mathf.Min(currentHealth + healAmount, maxLives);
        currentBee.SetHealth(newHealth);
        UpdateHearts(newHealth);
    }

    // ======================================================
    // SPEED BUTTON
    // ======================================================
    public void BuySpeed()
    {
        if (currentBee == null) return;
        if (currentBee.moveSpeed >= maxSpeed) return;

        if (!CurrencyManager.Instance.UseHoney(speedCost)) return;

        currentBee.UpgradeSpeed(speedUpgradeAmount, maxSpeed);
        speedText.text = $"Speed: {currentBee.moveSpeed:F1}";
        lastSpeed = currentBee.moveSpeed;
    }
}