using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, EndOfDay, GameOver }
    public enum Season { Spring, Summer, Autumn, Winter }

    public GameState State { get; private set; } = GameState.Playing;
    public Season CurrentSeason { get; private set; } = Season.Spring;
    public int CurrentDay { get; private set; } = 1;

    [Header("Day Settings")]
    [SerializeField] private float dayDurationSeconds = 120f;

    private float dayTimer;

    public event System.Action<int> OnDayEnd;
    public event System.Action<Season> OnSeasonChange;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        dayTimer = dayDurationSeconds;
    }

    private void Update()
    {
        if (State != GameState.Playing) return;

        dayTimer -= Time.deltaTime;
        if (dayTimer <= 0f)
            EndDay();
    }

    private void EndDay()
    {
        State = GameState.EndOfDay;
        OnDayEnd?.Invoke(CurrentDay);
        // UI will call ContinueToNextDay() after showing the summary screen
    }

    public void ContinueToNextDay()
    {
        CurrentDay++;
        AdvanceSeason();
        dayTimer = dayDurationSeconds;
        State = GameState.Playing;
    }

    private void AdvanceSeason()
    {
        // Each season lasts 7 days
        int seasonIndex = (CurrentDay - 1) / 7;
        Season newSeason = (Season)(seasonIndex % 4);
        if (newSeason != CurrentSeason)
        {
            CurrentSeason = newSeason;
            OnSeasonChange?.Invoke(CurrentSeason);
        }
    }

    public void SetPaused(bool paused)
    {
        if (State == GameState.GameOver || State == GameState.EndOfDay) return;
        State = paused ? GameState.Paused : GameState.Playing;
        Time.timeScale = paused ? 0f : 1f;
    }

    public void TriggerGameOver()
    {
        State = GameState.GameOver;
        Debug.Log("Game Over — Queen has died.");
    }

    public void TriggerSwarm()
    {
        State = GameState.GameOver;
        Debug.Log("Swarm — colony abandoned the hive.");
    }

    /// <summary>Returns a 0-1 float of how far through the current day we are.</summary>
    public float DayProgress => 1f - (dayTimer / dayDurationSeconds);
}
