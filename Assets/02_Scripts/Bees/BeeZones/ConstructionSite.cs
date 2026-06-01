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

        if (sr != null)
        {
            Color c = sr.color;
            c.a = t;
            sr.color = c;
        }

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
        isBuilding = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = true;

        // =========================
        // FIX: PROPER ZONE REGISTRATION
        // =========================

        // Sleep zone
        SleepZone sleep = GetComponent<SleepZone>();
        if (sleep != null)
        {
            HiveManager.Instance.RegisterSleepZone(sleep);
        }

        // Drone zone
        DroneZone drone = GetComponent<DroneZone>();
        if (drone != null)
        {
            DroneZone.allZones.Add(drone);
        }

        if (parentZone != null)
        {
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