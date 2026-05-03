using UnityEngine;

public class WinterSystem : MonoBehaviour
{
    public static WinterSystem Instance;

    public GameObject snowParticles;

    public bool isSnowing;

    private void Awake()
    {
        Instance = this;
    }

    public void StartWinter()
    {
        if (isSnowing) return;

        isSnowing = true;
        snowParticles?.SetActive(true);
    }

    public void StopWinter()
    {
        if (!isSnowing) return;

        isSnowing = false;
        snowParticles?.SetActive(false);
    }
}