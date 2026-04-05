using UnityEngine;
using UnityEngine.EventSystems;

public class BuildZone : MonoBehaviour
{
    public GameObject buildCanvas;

    public GameObject nursePrefab;
    public GameObject housePrefab;
    public GameObject sleepPrefab;

    private bool isBuilt = false;
    private bool isBuilding = false;

    private ConstructionSite currentSite;

    private void Update()
    {
        HandleClick();
    }

    void HandleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null && hit.gameObject == gameObject && !isBuilt)
            {
                buildCanvas.SetActive(true);
            }
            else
            {
                if (buildCanvas.activeSelf)
                {
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                        return;

                    buildCanvas.SetActive(false);
                }
            }
        }
    }

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

        buildCanvas.SetActive(false);

        isBuilding = true;

        // Spawn the real object immediately
        GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity);

        currentSite = obj.AddComponent<ConstructionSite>();

        // 🔥 IMPORTANT: link this zone
        currentSite.parentZone = this;

        // 🔥 start building instantly
        currentSite.StartBuild();

        // 🔥 send bees to work immediately
        BuilderBee.SetActiveSite(currentSite);

        // 🔥 lock the zone
        DisableZone();
    }

    public void FinishBuild()
    {
        isBuilding = false;
        isBuilt = true;

        EnableZone();
    }

    void DisableZone()
    {
        GetComponent<Collider2D>().enabled = false;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = Color.gray;
    }

    void EnableZone()
    {
        GetComponent<Collider2D>().enabled = true;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = Color.white;
    }
}