using UnityEngine;

public class RobberBee : Enemy
{
    [Header("Robber Settings")]
    public float stealRange = 1f;

    [Header("Honey Carry Settings")]
    public Vector2 honeyCarryOffset = new Vector2(0.3f, 0f);

    [Header("Flee Settings")]
    public float fleeDroneRange = 4f;
    public float fleeSpeed = 3.5f;

    [Header("World Bounds")]
    public float minX = -30f;
    public float maxX = 30f;
    public float minY = -20f;
    public float maxY = 20f;

    private Honey targetHoney;
    private bool carryingHoney = false;
    private Vector2 escapeDirection;

    protected override void Awake()
    {
        transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        base.Awake();
        enemyType = EnemyType.RobberBee;
    }

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

        DroneBee closestDrone = FindClosestDroneInRange();
        if (closestDrone != null)
            FleeFromDrone(closestDrone);
    }

    // -------------------------
    // FIND HONEY
    // -------------------------
    void FindHoney()
    {
        if (targetHoney != null) return;

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

        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer <= 0f || currentPath == null || currentPath.Count == 0)
        {
            currentPath = AStar.FindPathWorld(transform.position, targetHoney.transform.position);
            currentPathIndex = 0;
            pathUpdateTimer = 0.5f;
        }

        if (currentPath != null && currentPathIndex < currentPath.Count)
        {
            Vector2 targetPos = currentPath[currentPathIndex];
            float distToWaypoint = Vector2.Distance(transform.position, targetPos);
            
            if (distToWaypoint < 0.1f)
            {
                currentPathIndex++;
                if (currentPathIndex >= currentPath.Count) return;
                targetPos = currentPath[currentPathIndex];
            }

            Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
            Vector3 step = (Vector3)(dir * moveSpeed * Time.deltaTime);
            Vector3 nextPos = transform.position + step;

            if (HiveGrid.Instance != null && HiveGrid.Instance.IsBlockingAt(nextPos))
            {
                pathUpdateTimer = 0f;
            }
            else
            {
                transform.position = nextPos;
            }
        }
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
    }

    // -------------------------
    // KEEP HONEY ATTACHED
    // -------------------------
    void CarryHoney()
    {
        if (targetHoney == null) return;
        // Follow in world space — unaffected by parent scale
        targetHoney.transform.position = (Vector2)transform.position + honeyCarryOffset;
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

        targetHoney.SetCarried(false);
        carryingHoney = false;
        targetHoney = null;
    }
}