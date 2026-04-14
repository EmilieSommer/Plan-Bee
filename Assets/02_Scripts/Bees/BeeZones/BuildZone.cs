using UnityEngine;

public class BuildZone : MonoBehaviour
{
    public GameObject buildCanvas;

    public GameObject nursePrefab;
    public GameObject housePrefab;
    public GameObject sleepPrefab;

    private bool isBuilt = false;
    private bool isBuilding = false;

    private ConstructionSite currentSite;

    // ------------------------
    // BUTTONS
    // ------------------------

    public void BuildNurse()
    {
        StartConstruction(nursePrefab);
    }

    public void BuildHouse()
    {
        StartConstruction(housePrefab);
    }

    public void BuildSleep()
    {
        StartConstruction(sleepPrefab);
    }

    void StartConstruction(GameObject prefab)
    {
        if (isBuilding) return;

        isBuilding = true;

        if (buildCanvas != null)
            buildCanvas.SetActive(false);

        DisableZone();

        GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity);

        currentSite = obj.AddComponent<ConstructionSite>();
        currentSite.parentZone = this;

        currentSite.StartBuild();

        BuildManager.Instance.AddToQueue(currentSite);
    }

    public void FinishBuild()
    {
        isBuilding = false;
        isBuilt = true;

        // 🔥 IMPORTANT: Do NOT keep this zone
        // It should be replaced by the new zone (prefab)
        Destroy(gameObject);
    }

    void DisableZone()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = Color.gray;
    }

    public void Open()
    {
        if (isBuilt || isBuilding) return;

        if (buildCanvas != null)
            buildCanvas.SetActive(true);
    }
}