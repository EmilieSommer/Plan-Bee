using UnityEngine;
using System;

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance { get; private set; }

    public enum GameState
    {
        Playing,
        GameOver,
        Victory
    }

    public GameState CurrentState { get; private set; } = GameState.Playing;

    public event Action<string> OnGameOver;
    public event Action<string> OnVictory;

    private int yearsSurvived = 0;
    private float winCheckTimer = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (SeasonManager.Instance != null)
        {
            SeasonManager.OnSeasonChangedEvent += HandleSeasonChanged;
        }
    }

    private void OnDestroy()
    {
        if (SeasonManager.Instance != null)
        {
            SeasonManager.OnSeasonChangedEvent -= HandleSeasonChanged;
        }
    }

    private void HandleSeasonChanged(Season newSeason)
    {
        if (CurrentState != GameState.Playing) return;

        // Check if we reached Year 2 Spring
        if (newSeason == Season.Spring)
        {
            // The game starts in Spring (Year 0 basically, or Year 1). 
            // If we cycle all the way back to Spring, yearsSurvived increments.
            yearsSurvived++;

            if (yearsSurvived >= 1) // Reached Spring of the next year
            {
                CheckWinCondition();
            }
        }
    }

    public void CheckWinCondition()
    {
        if (CurrentState != GameState.Playing) return;

        int currentHoney = CurrencyManager.Instance != null ? CurrencyManager.Instance.honey : 0;
        int currentPopulation = HiveManager.Instance != null ? HiveManager.Instance.GetTotalBees() : 0;

        if (currentHoney >= 500 && currentPopulation >= 20)
        {
            TriggerVictory("Your colony survived the winter and has grown strong enough to swarm! You win!");
        }
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing) return;

        // Continuously check for win condition if we are in Year 2+
        if (yearsSurvived >= 1)
        {
            winCheckTimer += Time.deltaTime;
            if (winCheckTimer >= 2f)
            {
                winCheckTimer = 0f;
                CheckWinCondition();
            }
        }
    }

    public void TriggerGameOver(string reason)
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.GameOver;
        Debug.Log($"Game Over: {reason}");
        Time.timeScale = 0f; // Pause game

        OnGameOver?.Invoke(reason);
        
        // Ensure UIMessagePopup is available
        if (UIMessagePopup.Instance != null)
        {
            UIMessagePopup.Instance.ShowMessage("GAME OVER\n\n" + reason);
        }
    }

    public void TriggerVictory(string message)
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Victory;
        Debug.Log($"Victory: {message}");
        Time.timeScale = 0f; // Pause game

        OnVictory?.Invoke(message);

        if (UIMessagePopup.Instance != null)
        {
            UIMessagePopup.Instance.ShowMessage("VICTORY!\n\n" + message);
        }
    }
}
