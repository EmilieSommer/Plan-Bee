using UnityEngine;

public class BuildZone : MonoBehaviour
{
    public GameObject buildCanvas;

    public GameObject nursePrefab;
    public GameObject housePrefab;
    public GameObject sleepPrefab;
    public GameObject dronePrefab; // ✅ NEW

    [Header("Costs")]
    public int nurseCost = 10;
    public int houseCost = 20;
    public int sleepCost = 15;
    public int droneCost = 25; // ✅ NEW

    private bool isBuilt = false;
    private bool isBuilding = false;

    private ConstructionSite currentSite;

    private float buildTimer;

    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void BuildNurse() => TryStartConstruction(nursePrefab, nurseCost);
    public void BuildHouse()  => TryStartConstruction(housePrefab, houseCost);
    public void BuildSleep()  => TryStartConstruction(sleepPrefab, sleepCost);

    // ✅ NEW
    public void BuildDrone() => TryStartConstruction(dronePrefab, droneCost);

    void TryStartConstruction(GameObject prefab, int cost)
    {
        if (isBuilt || isBuilding) return;

        if (!CurrencyManager.Instance.UseHoney(cost))
            return;

        isBuilding = true;
        buildTimer = 0f;

        if (buildCanvas != null)
            buildCanvas.SetActive(false);

        DisableZone();

        GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity);

        currentSite = obj.AddComponent<ConstructionSite>();
        currentSite.parentZone = this;
        currentSite.StartBuild();

        BuildManager.Instance.AddToQueue(currentSite);
    }

    public void SetTransparency(float alpha)
    {
        if (sr == null) return;

        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }

    public void FinishBuild()
    {
        isBuilding = false;
        isBuilt = true;

        Destroy(gameObject);
    }

    void DisableZone()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
    }

    public void Open()
    {
        if (isBuilt || isBuilding) return;

        if (buildCanvas != null)
            buildCanvas.SetActive(true);
    }
}