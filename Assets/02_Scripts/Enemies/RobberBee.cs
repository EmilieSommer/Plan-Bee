using UnityEngine;

public class RobberBee : Enemy
{
    [Header("Robber Settings")]
    public float stealRange = 1f;

    [Header("Flee Settings")]
    public float fleeDroneRange = 4f;
    public float fleeSpeed = 3.5f;

    [Header("World Bounds")]
    public float minX = -15f;
    public float maxX = 15f;
    public float minY = -15f;
    public float maxY = 15f;

    private Honey targetHoney;
    private bool carryingHoney = false;
    private Vector2 escapeDirection;

    protected override void Update()
    {
        if (carryingHoney)
        {
            CarryHoney();
            Escape();
            return;
        }

        FindHoney();

        if (targetHoney != null)
        {
            MoveToHoney();
            return;
        }

        // No honey — flee from drones if nearby
        DroneBee closestDrone = FindClosestDroneInRange();
        if (closestDrone != null)
        {
            FleeFromDrone(closestDrone);
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
            if (honey == null) continue;

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
        if (targetHoney == null) return;

        float dist = Vector2.Distance(transform.position, targetHoney.transform.position);

        if (dist <= stealRange)
        {
            StealHoney();
            return;
        }

        Vector2 direction = (targetHoney.transform.position - transform.position).normalized;
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
    }

    // -------------------------
    // STEAL
    // -------------------------
    void StealHoney()
    {
        if (targetHoney == null) return;

        carryingHoney = true;
        targetHoney.SetCarried(true);

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
        if (targetHoney == null) return;
        targetHoney.transform.localPosition = new Vector3(0.5f, 0f, 0f);
    }

    // -------------------------
    // ESCAPE
    // -------------------------
    void Escape()
    {
        transform.position += (Vector3)(escapeDirection * moveSpeed * Time.deltaTime);

        if (IsOutsideWorld())
        {
            if (targetHoney != null)
            {
                targetHoney.SetCarried(false);
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
    // FLEE FROM DRONE
    // -------------------------
    DroneBee FindClosestDroneInRange()
    {
        DroneBee[] drones = FindObjectsOfType<DroneBee>();

        DroneBee closest = null;
        float closestDist = Mathf.Infinity;

        foreach (DroneBee drone in drones)
        {
            if (drone == null) continue;

            float dist = Vector2.Distance(transform.position, drone.transform.position);
            if (dist <= fleeDroneRange && dist < closestDist)
            {
                closestDist = dist;
                closest = drone;
            }
        }

        return closest;
    }

    void FleeFromDrone(DroneBee drone)
    {
        Vector2 fleeDir = ((Vector2)transform.position - (Vector2)drone.transform.position).normalized;
        transform.position += (Vector3)(fleeDir * fleeSpeed * Time.deltaTime);
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
        if (!carryingHoney || targetHoney == null) return;

        targetHoney.transform.SetParent(null);
        targetHoney.SetCarried(false);

        carryingHoney = false;
        targetHoney = null;
    }
}