using UnityEngine;
using TMPro;

public class BeeInfoUI : MonoBehaviour
{
    public static BeeInfoUI Instance;

    [Header("UI")]
    public GameObject panel;
    public TMP_Text healthText;
    public TMP_Text speedText;
    public TMP_Text typeText;

    private Bee currentBee;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    void Update()
    {
        // 🔥 live update while UI is open
        if (currentBee != null && panel.activeSelf)
        {
            healthText.text = $"Health: {currentBee.CurrentHealth}/{currentBee.maxHealth}";
        }
    }

    public void Show(Bee bee)
    {
        currentBee = bee;
        panel.SetActive(true);

        typeText.text = $"Type: {bee.beeType}";
        healthText.text = $"Health: {bee.CurrentHealth}/{bee.maxHealth}";
        speedText.text = $"Speed: {bee.moveSpeed}";
    }

    public void Hide()
    {
        panel.SetActive(false);
        currentBee = null;
    }
}