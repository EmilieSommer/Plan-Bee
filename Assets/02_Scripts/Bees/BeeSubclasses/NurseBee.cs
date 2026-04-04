using UnityEngine;

public class NurseBee : Bee
{
    [Header("Nurse Settings")]
    [SerializeField] private NurseBeeZone assignedZone;

    [Header("Work Settings")]
    public float workDistanceThreshold = 1.5f;

    [Header("Idle Settings")]
    public float idleWanderRadius = 2f;
    public float idleMoveCooldownMin = 2f;
    public float idleMoveCooldownMax = 5f;

    private float idleMoveTimer = 0f;
    private Egg assignedEgg;

    private bool isTending = false;
    private float orbitAngle = 0f;

    protected override void Awake()
    {
        base.Awake();

        if (assignedZone == null)
        {
            assignedZone = FindObjectOfType<NurseBeeZone>();
        }

        if (assignedZone != null)
        {
            homePosition = assignedZone.transform.position;
        }
        else
        {
            Debug.LogError("❌ No NurseBeeZone found in scene!");
        }
    }

    private void Start()
    {
        if (assignedZone == null)
        {
            Debug.LogError("❌ NurseBeeZone NOT assigned on " + gameObject.name);
        }
    }

    // ---------------------------
    // STATES
    // ---------------------------

    protected override void IdleBehavior()
    {
        // 🔁 Keep tending current egg if valid
        if (assignedEgg != null && !assignedEgg.IsHatched())
        {
            MoveToEgg(assignedEgg);
            return;
        }

        // 🧹 Clean up if egg is gone
        if (assignedEgg != null)
        {
            assignedEgg.RemoveNurse();
            assignedEgg = null;
        }

        isTending = false;

        // 🔍 Find a free egg
        assignedEgg = FindClosestEgg();

        if (assignedEgg != null)
        {
            MoveToEgg(assignedEgg);
        }
        else
        {
            NormalIdleMovement();
        }
    }

    protected override void WorkBehavior()
    {
        // ❌ Egg invalid → reset
        if (assignedEgg == null || assignedEgg.IsHatched())
        {
            if (assignedEgg != null)
            {
                assignedEgg.RemoveNurse();
            }

            assignedEgg = null;
            isTending = false;
            currentState = BeeState.Idle;
            return;
        }

        float dist = Vector2.Distance(transform.position, assignedEgg.transform.position);

        // 🔁 Move closer if needed
        if (dist > workDistanceThreshold)
        {
            MoveToEgg(assignedEgg);
            return;
        }

        // 🐝 Try to claim egg (ONLY ONE BEE ALLOWED)
        if (!isTending)
        {
            if (assignedEgg.TryAssignNurse(this))
            {
                isTending = true;
                Debug.Log($"{name} started nursing {assignedEgg.name}");
            }
            else
            {
                // ❌ Someone else took it → go idle
                assignedEgg = null;
                isTending = false;
                currentState = BeeState.Idle;
                return;
            }
        }

        // 🔄 Orbit egg
        OrbitEgg();
    }

    protected override void ReturnBehavior()
    {
        // Not used
    }

    protected override void OnReachedTarget()
    {
        if (assignedEgg != null && !assignedEgg.IsHatched())
        {
            currentState = BeeState.Working;
        }
        else
        {
            assignedEgg = null;
            isTending = false;
            currentState = BeeState.Idle;
        }
    }

    // ---------------------------
    // HELPERS
    // ---------------------------

    private void MoveToEgg(Egg egg)
    {
        if (egg == null) return;

        Vector2 offset = Random.insideUnitCircle.normalized * 0.3f;
        targetPosition = (Vector2)egg.transform.position + offset;

        if (currentState != BeeState.Moving)
        {
            currentState = BeeState.Moving;
        }
    }

    private Egg FindClosestEgg()
    {
        Egg closest = null;
        float closestDist = Mathf.Infinity;

        if (Egg.allEggs == null) return null;

        foreach (Egg egg in Egg.allEggs)
        {
            if (egg == null || egg.IsHatched())
                continue;

            // 🚫 Skip eggs that already have a nurse
            if (egg.HasNurse())
                continue;

            float dist = Vector2.Distance(transform.position, egg.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = egg;
            }
        }

        return closest;
    }

    private void NormalIdleMovement()
    {
        idleMoveTimer -= Time.deltaTime;

        if (idleMoveTimer <= 0f)
        {
            idleMoveTimer = Random.Range(idleMoveCooldownMin, idleMoveCooldownMax);

            Vector2 randomPoint;

            if (assignedZone != null)
            {
                randomPoint = assignedZone.GetRandomPoint();
            }
            else
            {
                randomPoint = (Vector2)transform.position + Random.insideUnitCircle * idleWanderRadius;
            }

            targetPosition = randomPoint;
            currentState = BeeState.Moving;
        }
    }

    private void OrbitEgg()
    {
        if (assignedEgg == null) return;

        orbitAngle += Time.deltaTime * 2f;

        float radius = 0.4f;

        Vector2 offset = new Vector2(
            Mathf.Cos(orbitAngle),
            Mathf.Sin(orbitAngle)
        ) * radius;

        targetPosition = (Vector2)assignedEgg.transform.position + offset;

        if (currentState != BeeState.Moving)
        {
            currentState = BeeState.Moving;
        }
    }
}