using UnityEngine;

public class RainSystem : MonoBehaviour
{
    public static RainSystem Instance;

    public GameObject rainParticles;

    public bool isRaining;

    [Header("Effects")]
    public float foragerSpeedMultiplier = 0.5f;
    public float pollenMultiplier = 0.6f;

    private void Awake()
    {
        Instance = this;
    }

    public void StartRain()
    {
        if (isRaining) return;

        isRaining = true;
        rainParticles?.SetActive(true);
    }

    public void StopRain()
    {
        if (!isRaining) return;

        isRaining = false;
        rainParticles?.SetActive(false);
    }
}