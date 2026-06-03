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

    // WinterSystem
    public void StartWinter()
    {
        if (isSnowing) return;
        isSnowing = true;
        if (activeSnow == null) activeSnow = Instantiate(snowPrefab);
        activeSnow.SetActive(true);
        BeeDeathPopup.Instance.ShowMessage("Snowstorm — Forager bees are staying inside the hive!", 4f);
    }

    public void StopWinter()
    {
        if (!isSnowing) return;
        isSnowing = false;
        if (activeSnow != null) activeSnow.SetActive(false);
        BeeDeathPopup.Instance.ShowMessage("Snow has stopped — Forager bees are back to work!", 4f);
    }
}