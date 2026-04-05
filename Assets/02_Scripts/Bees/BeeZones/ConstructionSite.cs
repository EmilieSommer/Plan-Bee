using UnityEngine;

public class ConstructionSite : MonoBehaviour
{
    public float buildTime = 5f;
    private float progress = 0f;

    private SpriteRenderer sr;

    private bool isActive = false;

    public BuildZone parentZone;


    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        // Start invisible
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }
    }

    public void StartBuild()
    {
        isActive = true;
    }

    private void Update()
    {
        if (!isActive) return;

        int builderCount = GetBuilderCount();

        if (builderCount <= 0) return;

        progress += Time.deltaTime * builderCount;

        float t = progress / buildTime;

        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Clamp01(t);
            sr.color = c;
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
            if (hit.CompareTag("Bee"))
            {
                BuilderBee bee = hit.GetComponent<BuilderBee>();
                if (bee != null)
                    count++;
            }
        }

        return count;
    }

    void FinishBuild()
    {
        Debug.Log("Build complete!");

        BuilderBee.SetActiveSite(null);

        // 🔥 Re-enable zone
        if (parentZone != null)
        {
            parentZone.EnableZone();
        }

        Destroy(this);
    }
}