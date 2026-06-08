using UnityEngine;
using System.Collections.Generic;
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 10f;
    public float moveSpeed = 2f;

    protected float currentHealth;

    [Header("Combat")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1f;

    [Header("Identity")]
    public EnemyType enemyType;

    [Header("Drone Targeting")]
    public int currentDroneAttackers = 0;
    public int maxDroneAttackers = 2;

    [Header("Queen Targeting")]
    [SerializeField] protected bool prefersQueen = false;

    protected float attackTimer;
    protected Bee targetBee;

    private Vector3 baseScale;
    private Vector3 lastPos;

    // -------------------------
    // INIT
    // -------------------------
    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        baseScale = transform.localScale;
        lastPos = transform.position;
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    // -------------------------
    // UPDATE
    // -------------------------
    protected virtual void Update()
    {
        FindNearestBee();
        MoveTowardsBee();
        HandleAttack();
        UpdateAnimator();
    }

    private float findTargetTimer = 0f;

    protected virtual void UpdateAnimator()
    {
        if (baseScale == Vector3.zero) baseScale = transform.localScale;

        bool isMoving = Vector3.Distance(transform.position, lastPos) > 0.001f;
        lastPos = transform.position;

        if (enemyType == EnemyType.VarroaMite)
        {
            if (isMoving)
            {
                // Fast bounce and slight forward lean
                float bounce = Mathf.Abs(Mathf.Sin(Time.time * 30f)) * 0.15f;
                transform.localScale = baseScale + new Vector3(-bounce * 0.5f, bounce, 0f);
                transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 20f) * 15f);
            }
            else
            {
                // Idle bounce
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime * 10f);
                float bounce = Mathf.Abs(Mathf.Sin(Time.time * 15f)) * 0.08f;
                transform.localScale = baseScale + new Vector3(-bounce * 0.5f, bounce, 0f);
            }
        }
        else
        {
            // Generic enemy animation
            if (isMoving)
            {
                Vector2 velocity = (transform.position - lastPos).normalized;
                float targetAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
                
                float waddle = Mathf.Sin(Time.time * 20f) * 10f;
                transform.localRotation = Quaternion.Euler(0, 0, targetAngle + waddle);
                
                float bob = Mathf.Abs(Mathf.Sin(Time.time * 20f)) * 0.05f;
                transform.localScale = baseScale + new Vector3(bob, bob, 0f);
            }
            else
            {
                // Just remove waddle but keep facing direction
                Vector3 euler = transform.localRotation.eulerAngles;
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 0, euler.z), Time.deltaTime * 10f);
                transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * 10f);
            }
        }
    }

    // -------------------------
    // DAMAGE
    // -------------------------
    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
            Die();
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    // -------------------------
    // TARGETING
    // -------------------------
    protected virtual void FindNearestBee()
    {
        findTargetTimer -= Time.deltaTime;
        if (findTargetTimer > 0f) return;
        findTargetTimer = 0.5f;

        if (prefersQueen)
        {
            var queen = FindObjectOfType<QueenBee>();
            if (queen != null && IsReachable(queen.transform.position))
            {
                targetBee = queen;
                return;
            }
        }

        Bee[] bees = FindObjectsOfType<Bee>();

        Bee closest = null;
        float closestDist = Mathf.Infinity;

        foreach (Bee bee in bees)
        {
            if (bee == null) continue;

            float dist = Vector2.Distance(transform.position, bee.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = bee;
            }
        }

        targetBee = closest;
    }

    protected bool IsReachable(Vector3 target)
    {
        if (HiveGrid.Instance == null) return true;

        Vector3 from = transform.position;
        float dist = Vector3.Distance(from, target);
        int samples = Mathf.Max(2, Mathf.CeilToInt(dist * 2f));

        for (int i = 1; i <= samples; i++)
        {
            Vector3 p = Vector3.Lerp(from, target, i / (float)samples);
            if (HiveGrid.Instance.IsBlockingAt(p)) return false;
        }
        return true;
    }

    // -------------------------
    // MOVEMENT
    // -------------------------
    private List<Vector2> currentPath;
    private int currentPathIndex;
    private float pathUpdateTimer;

    protected virtual void MoveTowardsBee()
    {
        if (targetBee == null) return;

        float dist = Vector2.Distance(transform.position, targetBee.transform.position);
        if (dist <= attackRange) return;

        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer <= 0f || currentPath == null || currentPath.Count == 0)
        {
            currentPath = AStar.FindPathWorld(transform.position, targetBee.transform.position);
            currentPathIndex = 0;
            pathUpdateTimer = 0.5f; // Re-path every 0.5s
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
                // If blocked by a wall unexpectedly, just recalculate path next frame
                pathUpdateTimer = 0f;
            }
            else
            {
                transform.position = nextPos;
            }
        }
    }

    // -------------------------
    // ATTACK
    // -------------------------
    protected virtual void HandleAttack()
    {
        if (targetBee == null) return;

        float dist = Vector2.Distance(transform.position, targetBee.transform.position);

        if (dist > attackRange) return;

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            Attack();
            attackTimer = attackCooldown;
        }
    }

    protected virtual void Attack()
    {
        if (targetBee == null) return;

        targetBee.TakeDamage(1f, this);
    }
    // -------------------------
    // 🧠 THREAT SYSTEM
    // -------------------------
    public virtual int GetThreatLevel()
    {
        switch (enemyType)
        {
            case EnemyType.Bear: return 100;
            case EnemyType.Wasp: return 80;
            case EnemyType.RobberBee: return 70;
            case EnemyType.Mouse: return 60;
            case EnemyType.HiveBeetle: return 50;
            case EnemyType.Ant: return 40;
            case EnemyType.VarroaMite: return 30;
            case EnemyType.WaxMoth: return 25;
            default: return 10;
        }
    }

    // -------------------------
    // 🐝 DRONE SYSTEM
    // -------------------------
    public bool CanBeTargetedByDrone()
    {
        return currentDroneAttackers < maxDroneAttackers;
    }

    public void RegisterDrone()
    {
        currentDroneAttackers++;
    }

    public void UnregisterDrone()
    {
        currentDroneAttackers = Mathf.Max(0, currentDroneAttackers - 1);
    }
}