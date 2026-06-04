using UnityEngine;

public class HiveWarningSystem : MonoBehaviour
{
    public static HiveWarningSystem Instance;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip warningSound;

    [Header("Check Interval")]
    public float checkInterval = 0.5f;
    private float checkTimer = 0f;

    private float lastQueenHealth = -1f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckQueenHealth();
        }
    }

    void CheckQueenHealth()
    {
        if (QueenBee.Instance == null) return;

        float health = QueenBee.Instance.CurrentHealth;
        int currentLives = Mathf.FloorToInt(health);
        int lastLives = Mathf.FloorToInt(lastQueenHealth);

        if (lastQueenHealth < 0f)
        {
            lastQueenHealth = health;
            return;
        }

        if (currentLives < lastLives)
            TriggerWarning($"The Queen took damage! ({currentLives} lives left)");

        lastQueenHealth = health;
    }

    void TriggerWarning(string message)
    {
        BeeDeathPopup.Instance.ShowMessage(message, 4f);

        if (audioSource != null && warningSound != null)
            audioSource.PlayOneShot(warningSound);
    }
}