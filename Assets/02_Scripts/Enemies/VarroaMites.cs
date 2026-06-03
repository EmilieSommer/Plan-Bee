using UnityEngine;

public class VarroaMites : Enemy
{
    [Header("Parasite Settings")]
    public float attachRange = 0.5f;
    public float slowMultiplier = 0.3f;

    [Header("Drone Fear")]
    public float droneFearRange = 2.5f;
    public float fleeSpeed = 3.5f;

    private Bee targetBeeAttached;
    private bool isAttached = false;

    private float originalSpeed;
    private float originalWorkSpeed;

    protected override void Update()
    {
        // -----------------------
        // ATTACHED STATE
        // -----------------------
        if (isAttached)
        {
            if (targetBeeAttached == null)
            {
                Destroy(gameObject);
                return;
            }

            transform.localPosition = Vector3.zero;
            return;
        }

        // -----------------------
        // FREE STATE
        // -----------------------
        bool droneNearby = IsDroneNearby();

        if (droneNearby)
        {
            // While fleeing, still grab any bee we pass close to
            Bee closeBee = FindBeeInAttachRange();
            if (closeBee != null)
            {
                Attach(closeBee);
                return;
            }

            FleeFromDrone();
            return;
        }

        Bee safeTarget = FindSafeBee();
        if (safeTarget == null) return;

        targetBee = safeTarget;
        MoveTowardsBee();
        TryAttach();
    }

    // -----------------------
    // ATTACH
    // -----------------------
    void TryAttach()
    {
        if (targetBee == null) return;

        float dist = Vector2.Distance(transform.position, targetBee.transform.position);
        if (dist <= attachRange)
            Attach(targetBee);
    }

    void Attach(Bee bee)
    {
        isAttached = true;
        targetBeeAttached = bee;

        // Slow movement
        originalSpeed = bee.moveSpeed;
        bee.moveSpeed *= slowMultiplier;

        // Slow work
        originalWorkSpeed = bee.workSpeedMultiplier;
        bee.workSpeedMultiplier *= slowMultiplier;

        // For forager specifically, also slow baseMoveSpeed
        ForagerBee forager = bee as ForagerBee;
        if (forager != null)
            forager.ApplyMiteSlow(slowMultiplier);

        Transform attachPoint = bee.transform.Find("MiteAttachPoint");
        transform.SetParent(attachPoint != null ? attachPoint : bee.transform);
        transform.localPosition = Vector3.zero;
    }

    // -----------------------
    // FIND BEE IN ATTACH RANGE (for fleeing grab)
    // -----------------------
    Bee FindBeeInAttachRange()
    {
        Bee[] bees = FindObjectsOfType<Bee>();
        foreach (Bee bee in bees)
        {
            if (bee == null) continue;
            if (Vector2.Distance(transform.position, bee.transform.position) <= attachRange)
                return bee;
        }
        return null;
    }

    // -----------------------
    // MOVEMENT
    // -----------------------
    void MoveTowardsBee()
    {
        if (targetBee == null) return;
        Vector2 dir = (targetBee.transform.position - transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
    }

    // -----------------------
    // SAFE TARGETING
    // -----------------------
    Bee FindSafeBee()
    {
        Bee[] bees = FindObjectsOfType<Bee>();
        Bee best = null;
        float bestDist = Mathf.Infinity;

        foreach (Bee bee in bees)
        {
            if (bee == null) continue;
            if (IsDroneNearBee(bee)) continue;

            float dist = Vector2.Distance(transform.position, bee.transform.position);
            if (dist < bestDist) { bestDist = dist; best = bee; }
        }

        return best;
    }

    bool IsDroneNearBee(Bee bee)
    {
        DroneBee[] drones = FindObjectsOfType<DroneBee>();
        foreach (DroneBee d in drones)
        {
            if (d == null) continue;
            if (Vector2.Distance(bee.transform.position, d.transform.position) <= droneFearRange)
                return true;
        }
        return false;
    }

    // -----------------------
    // DRONE FEAR
    // -----------------------
    bool IsDroneNearby()
    {
        DroneBee[] drones = FindObjectsOfType<DroneBee>();
        foreach (DroneBee d in drones)
        {
            if (d == null) continue;
            if (Vector2.Distance(transform.position, d.transform.position) <= droneFearRange)
                return true;
        }
        return false;
    }

    void FleeFromDrone()
    {
        DroneBee closest = FindClosestDrone();
        if (closest == null) return;
        Vector2 dir = (transform.position - closest.transform.position).normalized;
        transform.position += (Vector3)(dir * fleeSpeed * Time.deltaTime);
    }

    DroneBee FindClosestDrone()
    {
        DroneBee[] drones = FindObjectsOfType<DroneBee>();
        DroneBee best = null;
        float bestDist = Mathf.Infinity;

        foreach (DroneBee d in drones)
        {
            if (d == null) continue;
            float dist = Vector2.Distance(transform.position, d.transform.position);
            if (dist < bestDist) { bestDist = dist; best = d; }
        }

        return best;
    }

    // -----------------------
    // DAMAGE / REMOVAL
    // -----------------------
    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        if (currentHealth <= 0f)
            RemoveEffect();
    }

    void RemoveEffect()
    {
        if (targetBeeAttached != null)
        {
            targetBeeAttached.moveSpeed = originalSpeed;
            targetBeeAttached.workSpeedMultiplier = originalWorkSpeed;

            ForagerBee forager = targetBeeAttached as ForagerBee;
            if (forager != null)
                forager.RemoveMiteSlow(originalSpeed);
        }

        isAttached = false;
        targetBeeAttached = null;
        transform.SetParent(null);
    }

    protected override void Attack() { }
}