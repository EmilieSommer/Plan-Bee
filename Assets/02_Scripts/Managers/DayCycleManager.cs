using UnityEngine;
using TMPro;

public class DayCycleManager : MonoBehaviour
{
    public static DayCycleManager Instance;

    [Header("Day Settings")]
    public float dayLength = 180f; // 3 minutes full day
    public int currentDay = 1;

    [Header("UI")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI timeText;

    private float timer;

    public float DifficultyMultiplier => 1f + (currentDay - 1) * 0.25f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        timer = dayLength;
        UpdateUI();
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            currentDay++;
            timer = dayLength;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (dayText != null)
            dayText.text = "Day " + currentDay;

        if (timeText != null)
            timeText.text = GetGameTime();
    }

    string GetGameTime()
    {
        float normalizedTime = 1f - (timer / dayLength); // 0 → 1
        float totalHours = normalizedTime * 24f;

        int hours = Mathf.FloorToInt(totalHours);

        string period = hours >= 12 ? "PM" : "AM";

        int displayHour = hours % 12;
        if (displayHour == 0)
            displayHour = 12;

        return $"{displayHour} {period}";
    }
}