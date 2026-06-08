using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Tooltip("The root GameObject of the pause menu panel (hidden by default).")]
    public GameObject pauseMenuPanel;

    [Tooltip("Key that opens/closes the pause menu.")]
    public KeyCode toggleKey = KeyCode.Escape;

    [Tooltip("Name of the main menu scene to load. Leave blank to reload current scene instead.")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused;

    private void Start()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (GameManager.Instance != null) GameManager.Instance.SetPaused(true);
        else Time.timeScale = 0f;
    }

    public void Resume()
    {
        isPaused = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (GameManager.Instance != null) GameManager.Instance.SetPaused(false);
        else Time.timeScale = 1f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(mainMenuSceneName) &&
            Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            // Fallback: just reload current scene if MainMenu scene doesn't exist yet.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
