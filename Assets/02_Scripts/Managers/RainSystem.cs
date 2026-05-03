using UnityEngine;

public class RainSystem : MonoBehaviour
{
    public static RainSystem Instance;

    [Header("Prefab (NOT scene object)")]
    public GameObject rainPrefab;

    private GameObject activeRain;

    public bool isRaining;

    private void Awake()
    {
        Instance = this;
    }

    public void StartRain()
    {
        if (isRaining) return;

        isRaining = true;

        if (activeRain == null)
        {
            activeRain = Instantiate(rainPrefab);
        }

        activeRain.SetActive(true);
    }

    public void StopRain()
    {
        if (!isRaining) return;

        isRaining = false;

        if (activeRain != null)
        {
            activeRain.SetActive(false);
        }
    }
}