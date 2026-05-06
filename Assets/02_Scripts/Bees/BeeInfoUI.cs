using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BeeInfoUI : MonoBehaviour
{
    public static BeeInfoUI Instance;

    [Header("UI")]
    public GameObject panel;
    public Slider healthSlider;

    public TMP_Text typeText;
    public TMP_Text nameText;
    public TMP_Text speedText;

    private Bee currentBee;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    void Update()
    {
        // 🔥 live update health slider
        if (currentBee != null && panel.activeSelf)
        {
            healthSlider.value = currentBee.CurrentHealth;
        }
    }

    public void Show(Bee bee)
    {
        currentBee = bee;
        panel.SetActive(true);

        // 🐝 type
        typeText.text = $"Type: {bee.beeType}";

        // 🏷️ custom name (NEW)
        nameText.text = $"Name: {bee.gameObject.name}";

        // 🚀 speed
        speedText.text = $"Speed: {bee.moveSpeed}";

        // ❤️ health slider setup
        healthSlider.maxValue = bee.maxHealth;
        healthSlider.value = bee.CurrentHealth;
    }

    public void Hide()
    {
        panel.SetActive(false);
        currentBee = null;
    }
}