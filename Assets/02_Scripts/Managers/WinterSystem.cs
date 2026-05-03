using UnityEngine;

public class WinterSystem : MonoBehaviour
{
    public static WinterSystem Instance;

    [Header("Prefab (NOT scene object)")]
    public GameObject snowPrefab;

    private GameObject activeSnow;

    public bool isSnowing;

    private void Awake()
    {
        Instance = this;
    }

    public void StartWinter()
    {
        if (isSnowing) return;

        isSnowing = true;

        if (activeSnow == null)
        {
            activeSnow = Instantiate(snowPrefab);
        }

        activeSnow.SetActive(true);
    }

    public void StopWinter()
    {
        if (!isSnowing) return;

        isSnowing = false;

        if (activeSnow != null)
        {
            activeSnow.SetActive(false);
        }
    }
}