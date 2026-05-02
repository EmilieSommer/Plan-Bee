using UnityEngine;

public class BuildZone : MonoBehaviour
{
    public GameObject buildCanvas;

    public GameObject nursePrefab;
    public GameObject housePrefab;
    public GameObject sleepPrefab;

    [Header("Costs")]
    public int nurseCost = 10;
    public int houseCost = 20;
    public int sleepCost = 15;

    private bool isBuilt = false;
    private bool isBuilding = false;

    private ConstructionSite currentSite;

    private float buildTimeout = 10f;
    private float buildTimer;

    // ------------------------
    // BUTTONS
    // ------------------------

    public void BuildNurse()
    {
        TryStartConstruction(nursePrefab, nurseCost);
    }

    public void BuildHouse()
    {
        TryStartConstruction(housePrefab, houseCost);
    }

    public void BuildSleep()
    {
        TryStartConstruction(sleepPrefab, sleepCost);
    }

    // ------------------------
    // BUILD LOGIC (FIXED)
    // ------------------------

    void TryStartConstruction(GameObject prefab, int cost)
    {
        if (isBuilt) return;
        if (isBuilding) return;

        if (CurrencyManager.Instance == null)
        {
            Debug.LogError("CurrencyManager is NULL");
            return;
        }

        // 🔥 safe check BEFORE spending
        if (CurrencyManager.Instance.honey < cost)
        {
            Debug.Log("Not enough honey!");
            return;
        }

        CurrencyManager.Instance.UseHoney(cost);

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

    // ------------------------
    // FINISH BUILD
    // ------------------------

    public void FinishBuild()
    {
        isBuilding = false;
        isBuilt = true;

        Destroy(gameObject);
    }

    // ------------------------
    // SAFETY TIMEOUT (prevents soft-lock)
    // ------------------------

    private void Update()
    {
        if (!isBuilding) return;

        buildTimer += Time.deltaTime;

        if (buildTimer > buildTimeout)
        {
            Debug.LogWarning("Build timeout reset (FinishBuild not called)");

            isBuilding = false;
            buildTimer = 0f;
        }
    }

    // ------------------------
    // VISUAL DISABLE
    // ------------------------

    void DisableZone()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = Color.gray;
    }

    // ------------------------
    // UI OPEN
    // ------------------------

    public void Open()
    {
        if (isBuilt || isBuilding) return;

        if (buildCanvas != null)
            buildCanvas.SetActive(true);
    }
}