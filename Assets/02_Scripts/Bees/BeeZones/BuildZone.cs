using UnityEngine;
using TMPro;

public class BuildZone : MonoBehaviour
{
    public GameObject buildCanvas;

    public GameObject nursePrefab;
    public GameObject housePrefab;
    public GameObject sleepPrefab;
    public GameObject dronePrefab;

    [Header("Costs")]
    public int nurseCost = 10;
    public int houseCost = 20;
    public int sleepCost = 15;
    public int droneCost = 25;

    [Header("UI")]
    public TextMeshProUGUI buildProgressText;

    private bool isBuilt = false;
    private bool isBuilding = false;

    private ConstructionSite currentSite;

    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // timer starts OFF
        if (buildProgressText != null)
        {
            buildProgressText.gameObject.SetActive(false);
            buildProgressText.text = "";
        }
    }

    // -------------------------
    // BUILD BUTTONS
    // -------------------------
    public void BuildNurse() => TryStartConstruction(nursePrefab, nurseCost);
    public void BuildHouse() => TryStartConstruction(housePrefab, houseCost);
    public void BuildSleep() => TryStartConstruction(sleepPrefab, sleepCost);
    public void BuildDrone() => TryStartConstruction(dronePrefab, droneCost);

    // -------------------------
    // START BUILD
    // -------------------------
    void TryStartConstruction(GameObject prefab, int cost)
    {
        if (isBuilt || isBuilding) return;

        if (!CurrencyManager.Instance.UseHoney(cost))
        {
            UIMessagePopup.Instance.ShowMessage("Not enough honey!");
            return;
        }

        isBuilding = true;

        if (buildCanvas != null)
            buildCanvas.SetActive(false);

        DisableZone();

        // 🟢 ACTIVATE TIMER HERE
        if (buildProgressText != null)
        {
            buildProgressText.gameObject.SetActive(true);
            buildProgressText.text = "Building: 0%";
        }

        GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity);

        currentSite = obj.AddComponent<ConstructionSite>();
        currentSite.parentZone = this;
        currentSite.StartBuild();

        currentSite.OnProgressChanged += UpdateBuildProgress;

        BuildManager.Instance.AddToQueue(currentSite);
    }

    // -------------------------
    // TIMER UPDATE
    // -------------------------
    void UpdateBuildProgress(float progress)
    {
        if (buildProgressText == null) return;

        int percent = Mathf.RoundToInt(progress * 100f);
        buildProgressText.text = "Building: " + percent + "%";
    }

    void ClearProgress()
    {
        if (buildProgressText == null) return;

        buildProgressText.text = "";
        buildProgressText.gameObject.SetActive(false);
    }

    // -------------------------
    // FINISH BUILD
    // -------------------------
    public void FinishBuild()
    {
        isBuilding = false;
        isBuilt = true;

        ClearProgress();

        Destroy(gameObject);
    }

    // -------------------------
    // ZONE CONTROL
    // -------------------------
    void DisableZone()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    public void Open()
    {
        if (isBuilt || isBuilding) return;

        if (buildCanvas != null)
            buildCanvas.SetActive(true);
    }

    // -------------------------
    // TRANSPARENCY (used by ConstructionSite)
    // -------------------------
    public void SetTransparency(float alpha)
    {
        if (sr == null) return;

        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }
}