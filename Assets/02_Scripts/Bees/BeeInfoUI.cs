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

    private Bee currentBee;
    private int lastHealth = -1;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    void Update()
    {
        HandleAutoClose();

        if (!panel.activeSelf || currentBee == null)
            return;

        int health = Mathf.RoundToInt(currentBee.CurrentHealth);

        if (health != lastHealth)
        {
            lastHealth = health;
            UpdateHearts(health);
        }
    }

    // ======================================================
    // OPEN
    // ======================================================
    public void Open(Bee bee)
    {
        currentBee = bee;

        panel.SetActive(true);

        typeText.text = $"Type: {bee.beeType}";
        nameText.text = $"Name: {bee.beeName}";
        speedText.text = $"Speed: {bee.moveSpeed}";

        lastHealth = -1;
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

            // click on UI → ignore
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            Close();
        }
    }

    // ======================================================
    // HEART UPDATE (clamped to max 10)
    // ======================================================
    void UpdateHearts(int currentHealth)
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxLives);

        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i < currentHealth;
        }
    }

    // ======================================================
    // HEAL BUTTON
    // ======================================================
    public void BuyHeal()
    {
        if (currentBee == null)
            return;

        int currentHealth = Mathf.RoundToInt(currentBee.CurrentHealth);

        // already full
        if (currentHealth >= maxLives)
            return;

        // ❌ check currency FIRST
        if (!CurrencyManager.Instance.UseHoney(healCost))
        {
            // CurrencyManager already shows warning + sound
            return;
        }

        // ✔ apply heal
        int newHealth = Mathf.Min(currentHealth + healAmount, maxLives);

        currentBee.SetHealth(newHealth);

        UpdateHearts(newHealth);
    }
}