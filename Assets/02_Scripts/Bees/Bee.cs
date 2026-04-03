using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public abstract class Bee : MonoBehaviour
{
    public enum BeeState
    {
        Idle,
        Moving,
        Working,
        Returning,
        Dead
    }

    [Header("Base Stats")]
    public float maxHealth = 10f;
    protected float currentHealth;

    [Header("Movement")]
    public float moveSpeed = 2f;
    protected Vector2 moveDirection;
    protected Vector2 targetPosition;

    protected BeeState currentState;

    protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Ensure correct 2D physics setup
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        currentHealth = maxHealth;
        currentState = BeeState.Idle;

        PickRandomDirection();
    }

    protected virtual void Update()
    {
        if (currentState == BeeState.Dead)
            return;

        StateUpdate();
    }

    protected virtual void FixedUpdate()
    {
        if (currentState == BeeState.Moving)
        {
            rb.linearVelocity = moveDirection * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    protected virtual void StateUpdate()
    {
        switch (currentState)
        {
            case BeeState.Idle:
                IdleBehavior();
                break;

            case BeeState.Moving:
                MoveToTarget();
                break;

            case BeeState.Working:
                WorkBehavior();
                break;

            case BeeState.Returning:
                ReturnBehavior();
                break;
        }
    }

    protected void MoveToTarget()
    {
        Vector2 direction = (targetPosition - rb.position).normalized;
        moveDirection = direction;

        if (Vector2.Distance(rb.position, targetPosition) < 0.1f)
        {
            OnReachedTarget();
        }
    }

    protected void PickRandomDirection()
    {
        moveDirection = Random.insideUnitCircle.normalized;
    }

    protected virtual void OnReachedTarget()
    {
        currentState = BeeState.Idle;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == BeeState.Dead)
            return;

        if (collision.gameObject.CompareTag("Bee"))
        {
            Vector2 normal = collision.contacts[0].normal;

            moveDirection = Vector2.Reflect(moveDirection, normal);

            // Add small randomness so it feels organic
            moveDirection += Random.insideUnitCircle * 0.2f;
            moveDirection.Normalize();
        }
    }

    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        currentState = BeeState.Dead;
        rb.linearVelocity = Vector2.zero;
        Destroy(gameObject);
    }

    protected abstract void IdleBehavior();
    protected abstract void WorkBehavior();
    protected abstract void ReturnBehavior();
}