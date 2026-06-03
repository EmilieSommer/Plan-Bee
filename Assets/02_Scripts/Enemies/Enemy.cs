using UnityEngine;

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

    protected float attackTimer;
    protected Bee targetBee;

    // -------------------------
    // INIT
    // -------------------------
    protected virtual void Awake()
    {
        currentHealth = maxHealth;
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

    // -------------------------
    // MOVEMENT
    // -------------------------
    protected virtual void MoveTowardsBee()
    {
        if (targetBee == null) return;

        float dist = Vector2.Distance(transform.position, targetBee.transform.position);

        if (dist <= attackRange) return;

        Vector2 dir = (targetBee.transform.position - transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
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