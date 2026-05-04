using UnityEngine;

public class ConstructionSite : MonoBehaviour
{
    public float buildTime = 5f;
    private float progress = 0f;

    public BuildZone parentZone;

    private SpriteRenderer sr;
    private bool isBuilding = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
    }

    private void Update()
    {
        if (!isBuilding) return;

        int builderCount = GetBuilderCount();
        if (builderCount <= 0) return;

        progress += Time.deltaTime * builderCount;

        float t = Mathf.Clamp01(progress / buildTime);

        // Fade IN building
        if (sr != null)
        {
            Color c = sr.color;
            c.a = t;
            sr.color = c;
        }

        // Fade OUT build zone
        if (parentZone != null)
        {
            parentZone.SetTransparency(1f - t);
        }

        if (progress >= buildTime)
        {
            FinishBuild();
        }
    }

    int GetBuilderCount()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            transform.position,
            new Vector2(3f, 3f),
            0f
        );

        int count = 0;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Bee") && hit.GetComponent<BuilderBee>() != null)
            {
                count++;
            }
        }

        return count;
    }

    void FinishBuild()
    {
        Debug.Log("Build complete!");

        isBuilding = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = true;

        if (parentZone != null)
        {
            HouseBeeZone zone = GetComponent<HouseBeeZone>();
            if (zone != null)
            {
                zone.enabled = true;
                zone.ActivateZone();
            }

            parentZone.FinishBuild();
        }

        BuildManager.Instance.FinishCurrent();

        Destroy(this);
    }

    public void StartBuild()
    {
        isBuilding = true;

        HouseBeeZone zone = GetComponent<HouseBeeZone>();
        if (zone != null)
        {
            zone.SetInactive();
        }
    }
}