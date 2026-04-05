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

        // start invisible
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }
    }

    public void StartBuild()
    {
        isBuilding = true;
    }

    private void Update()
    {
        if (!isBuilding) return;

        int builderCount = GetBuilderCount();

        if (builderCount <= 0) return;

        progress += Time.deltaTime * builderCount;

        float t = progress / buildTime;

        // fade in
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
                if (hit.GetComponent<BuilderBee>() != null)
                    count++;
            }
        }

        return count;
    }

    void FinishBuild()
    {
        Debug.Log("Build complete!");

        BuilderBee.SetActiveSite(null);

        if (parentZone != null)
        {
            parentZone.FinishBuild();
        }

        Destroy(this);
    }
}