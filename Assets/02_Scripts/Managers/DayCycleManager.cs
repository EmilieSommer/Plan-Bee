using UnityEngine;
using TMPro;

public class DayCycleManager : MonoBehaviour
{
    public static DayCycleManager Instance;

    public const int DaysPerSeason = 7;
    public const int DaysPerYear = DaysPerSeason * 4;

    [Header("Day Settings")]
    public float dayLength = 600f; // 10 minutes full day
    public int currentDay = 1;

    [Header("Game Mode")]
    public bool endlessMode = false;

    [Header("UI")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI timeText;

    private float timer;

    public bool Victory { get; private set; }
    public event System.Action<int> OnDayChanged;
    public event System.Action OnVictory;

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
        if (Victory) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            currentDay++;
            timer = dayLength;
            OnDayChanged?.Invoke(currentDay);

            if (!endlessMode && currentDay > DaysPerYear)
            {
                Victory = true;
                OnVictory?.Invoke();
                Debug.Log($"Victory! Survived {DaysPerYear} days.");
            }
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