using UnityEngine;
using UnityEngine.EventSystems;

public class BuildZone : MonoBehaviour
{
    public GameObject buildCanvas;

    public GameObject nursePrefab;
    public GameObject housePrefab;
    public GameObject sleepPrefab;

    private GameObject previewObject;
    private ConstructionSite currentSite;

    private bool isPlacing = false;
    private bool isBuilt = false;

    private void Update()
    {
        HandleClick();

        if (isPlacing && Input.GetMouseButtonDown(0))
        {
            ConfirmBuild();
        }
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
                return;
            }

            if (buildCanvas.activeSelf)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                buildCanvas.SetActive(false);
            }
        }
    }

    // ------------------------
    // UI BUTTONS
    // ------------------------

    public void BuildNurse()
    {
        BeginPlacement(nursePrefab);
    }

    public void BuildHouse()
    {
        BeginPlacement(housePrefab);
    }

    public void BuildSleep()
    {
        BeginPlacement(sleepPrefab);
    }

    void BeginPlacement(GameObject prefab)
    {
        buildCanvas.SetActive(false);

        // Spawn preview
        previewObject = Instantiate(prefab, transform.position, Quaternion.identity);

        currentSite = previewObject.AddComponent<ConstructionSite>();

        // 🔥 link the zone
        currentSite.parentZone = this;

        isPlacing = true;

        // 🔥 disable zone while building
        DisableZone();
    }

    void ConfirmBuild()
    {
        isPlacing = false;

        BuilderBee.SetActiveSite(currentSite);

        currentSite.StartBuild();

        isBuilt = true;
    }

    public void EnableZone()
    {
        GetComponent<Collider2D>().enabled = true;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = Color.white;
    }

    void DisableZone()
    {
        GetComponent<Collider2D>().enabled = false;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = Color.gray;
    }
}