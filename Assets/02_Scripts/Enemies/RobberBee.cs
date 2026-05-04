using UnityEngine;

public class RobberBee : Enemy
{
    [Header("Robber Settings")]
    public float stealRange = 1f;

    [Header("World Bounds")]
    public float minX = -30f;
    public float maxX = 30f;
    public float minY = -20f;
    public float maxY = 20f;

    private Honey targetHoney;
    private bool carryingHoney = false;
    private Vector2 escapeDirection;

    protected override void Update()
    {
        if (carryingHoney)
        {
            CarryHoney();
            Escape();
        }
        else
        {
            FindHoney();
            MoveToHoney();
        }
    }

    // -------------------------
    // FIND HONEY
    // -------------------------
    void FindHoney()
    {
        if (targetHoney != null)
            return;

        Honey[] honeyObjects = FindObjectsOfType<Honey>();

        Honey closest = null;
        float closestDist = Mathf.Infinity;

        foreach (Honey honey in honeyObjects)
        {
            if (honey == null)
                continue;

            float dist = Vector2.Distance(transform.position, honey.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = honey;
            }
        }

        targetHoney = closest;
    }

    // -------------------------
    // MOVE TO HONEY
    // -------------------------
    void MoveToHoney()
    {
        if (targetHoney == null)
            return;

        float dist = Vector2.Distance(transform.position, targetHoney.transform.position);

        if (dist <= stealRange)
        {
            StealHoney();
            return;
        }

        Vector2 direction =
            (targetHoney.transform.position - transform.position).normalized;

        transform.position +=
            (Vector3)(direction * moveSpeed * Time.deltaTime);
    }

    // -------------------------
    // STEAL
    // -------------------------
    void StealHoney()
    {
        if (targetHoney == null)
            return;

        carryingHoney = true;

        targetHoney.SetCarried(true); // ✅ mark as stolen

        escapeDirection = ((Vector2)transform.position).normalized;

        if (escapeDirection == Vector2.zero)
            escapeDirection = Vector2.right;

        targetHoney.transform.SetParent(transform);
        targetHoney.transform.localPosition = new Vector3(0.5f, 0f, 0f);
    }

    // -------------------------
    // KEEP HONEY ATTACHED
    // -------------------------
    void CarryHoney()
    {
        if (targetHoney == null)
            return;

        targetHoney.transform.localPosition =
            new Vector3(0.5f, 0f, 0f);
    }

    // -------------------------
    // ESCAPE
    // -------------------------
    void Escape()
    {
        transform.position +=
            (Vector3)(escapeDirection * moveSpeed * Time.deltaTime);

        if (IsOutsideWorld())
        {
            if (targetHoney != null)
            {
                targetHoney.SetCarried(false); // just in case
                Destroy(targetHoney.gameObject);
            }

            Destroy(gameObject);
        }
    }

    bool IsOutsideWorld()
    {
        return transform.position.x < minX ||
               transform.position.x > maxX ||
               transform.position.y < minY ||
               transform.position.y > maxY;
    }

    // -------------------------
    // DAMAGE
    // -------------------------
    public override void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            DropHoney();
            Destroy(gameObject);
        }
    }

    // -------------------------
    // DROP HONEY
    // -------------------------
    void DropHoney()
    {
        if (!carryingHoney || targetHoney == null)
            return;

        targetHoney.transform.SetParent(null);

        targetHoney.SetCarried(false); // ✅ now collectible again

        carryingHoney = false;
        targetHoney = null;
    }
}