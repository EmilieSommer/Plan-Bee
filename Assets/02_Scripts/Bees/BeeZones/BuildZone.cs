using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BuildZone : MonoBehaviour
{
    public float buildProgress = 0f;
    public float buildRequired = 10f;

    public bool isSelected = false;
    public bool isBuilt = false;

    public List<BuilderBee> beesInZone = new List<BuilderBee>();

    public SpriteRenderer spriteRenderer;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Only build if:
        // - selected
        // - bees inside
        // - not built
        if (!isSelected || beesInZone.Count == 0 || isBuilt)
            return;

        float buildSpeed = 0f;

        foreach (var bee in beesInZone)
        {
            buildSpeed += bee.buildPower;
        }

        buildProgress += buildSpeed * Time.deltaTime;

        float progress = Mathf.Clamp01(buildProgress / buildRequired);

        Color c = spriteRenderer.color;
        c.a = Mathf.Lerp(0.2f, 1f, progress);
        spriteRenderer.color = c;

        if (buildProgress >= buildRequired)
        {
            CompleteBuild();
        }
    }

    void CompleteBuild()
    {
        isBuilt = true;

        Color c = spriteRenderer.color;
        c.a = 1f;
        spriteRenderer.color = c;

        Debug.Log("Build Complete!");
    }

    public void AssignBee(BuilderBee bee)
    {
        if (!beesInZone.Contains(bee))
        {
            beesInZone.Add(bee);
        }
    }

    public void RemoveBee(BuilderBee bee)
    {
        if (beesInZone.Contains(bee))
        {
            beesInZone.Remove(bee);
        }
    }

    // ---------------- TRIGGERS ----------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        BuilderBee bee = other.GetComponent<BuilderBee>();

        if (bee != null)
        {
            // ONLY assign when physically inside
            AssignBee(bee);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        BuilderBee bee = other.GetComponent<BuilderBee>();

        if (bee != null)
        {
            RemoveBee(bee);
        }
    }

    public bool IsBuilt()
    {
        return isBuilt;
    }
}