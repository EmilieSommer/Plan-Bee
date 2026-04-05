using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 10f;
    public float moveSpeed = 2f;

    private float currentHealth;

    [Header("Combat")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1f;

    private float attackTimer;
    private Bee targetBee;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        FindNearestBee();
        MoveTowardsBee();
        HandleAttack();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            Destroy(gameObject);
        }
    }

    void FindNearestBee()
    {
        Bee[] bees = FindObjectsOfType<Bee>();

        Bee closest = null;
        float closestDist = Mathf.Infinity;

        foreach (Bee bee in bees)
        {
            float dist = Vector2.Distance(transform.position, bee.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = bee;
            }
        }

        targetBee = closest;
    }

    void MoveTowardsBee()
    {
        if (targetBee == null) return;

        float dist = Vector2.Distance(transform.position, targetBee.transform.position);

        if (dist <= attackRange) return;

        Vector2 direction = (targetBee.transform.position - transform.position).normalized;

        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
    }

    void HandleAttack()
    {
        if (targetBee == null) return;

        float dist = Vector2.Distance(transform.position, targetBee.transform.position);

        if (dist > attackRange)
            return;

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            Attack();
            attackTimer = attackCooldown;
        }
    }

    void Attack()
    {
        if (targetBee == null) return;

        targetBee.TakeDamage(1f);
    }
}