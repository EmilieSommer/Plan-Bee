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

        // 🔒 CLOSE UI
        if (buildCanvas != null)
            buildCanvas.SetActive(false);

        // 🔒 DISABLE CLICKING IMMEDIATELY
        DisableZone();

        // 🏗️ SPAWN BUILDING (this will become final object)
        GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity);

        currentSite = obj.AddComponent<ConstructionSite>();
        currentSite.parentZone = this;

        currentSite.StartBuild();

        BuilderBee.SetActiveSite(currentSite);
    }

    public void FinishBuild()
    {
        isBuilding = false;
        isBuilt = true;

        // 💀 DESTROY THIS ZONE COMPLETELY
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
        // 🚫 DO NOT OPEN if building or built
        if (isBuilt || isBuilding) return;

        if (buildCanvas != null)
        {
            buildCanvas.SetActive(true);
        }
    }
}