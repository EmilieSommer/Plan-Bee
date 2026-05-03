using UnityEngine;

public class Mice : Enemy
{
    [Header("Mice Stats")]
    public float detectionRange = 8f;

    [Header("Eating")]
    public float eatRange = 0.8f;
    public float eatTime = 2.5f;

    private Honey targetHoney;
    private float eatTimer;
    private bool isEating;

    protected override void Update()
    {
        base.Update(); // optional enemy behaviour (can remove if unwanted)

        HandleHoneyLogic();
    }

    // -------------------------
    // MAIN LOGIC
    // -------------------------
    void HandleHoneyLogic()
    {
        if (isEating)
        {
            EatHoney();
            return;
        }

        // 🔥 ALWAYS refresh target if invalid or missing
        if (targetHoney == null)
            targetHoney = FindClosestHoney();

        if (targetHoney == null)
        {
            // no honey → stay still
            return;
        }

        float dist = Vector2.Distance(transform.position, targetHoney.transform.position);

        if (dist <= eatRange)
        {
            StartEating();
            return;
        }

        MoveTowardsHoney();
    }

    // -------------------------
    // FIND HONEY (IMPORTANT FIX)
    // -------------------------
    Honey FindClosestHoney()
    {
        Honey[] all = FindObjectsOfType<Honey>();

        Honey closest = null;
        float bestDist = Mathf.Infinity;

        foreach (Honey h in all)
        {
            if (h == null) continue;

            float d = Vector2.Distance(transform.position, h.transform.position);

            if (d < bestDist && d <= detectionRange)
            {
                bestDist = d;
                closest = h;
            }
        }

        return closest;
    }

    // -------------------------
    // MOVE
    // -------------------------
    void MoveTowardsHoney()
    {
        if (targetHoney == null) return;

        Vector2 dir = (targetHoney.transform.position - transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
    }

    // -------------------------
    // EATING
    // -------------------------
    void StartEating()
    {
        isEating = true;
        eatTimer = eatTime;
    }

    void EatHoney()
    {
        if (targetHoney == null)
        {
            isEating = false;
            return;
        }

        eatTimer -= Time.deltaTime;

        if (eatTimer <= 0f)
        {
            // 🔥 capture reference first
            Honey eaten = targetHoney;

            // immediately clear state (IMPORTANT)
            targetHoney = null;
            isEating = false;

            // remove instantly
            if (eaten != null)
            {
                Destroy(eaten.gameObject);
            }
        }
    }

    // -------------------------
    // OPTIONAL: enemy damage override later
    // -------------------------
    protected override void Attack()
    {
        // mice don't attack bees
    }
}