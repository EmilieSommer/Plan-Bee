using UnityEngine;

public class QueenBee : Bee
{
    public GameObject gameOverCanvas;

    protected override void Awake()
    {
        base.Awake();
        beeType = BeeType.Queen;
    }

    protected override void Start()
    {
        AssignZone();
    }

    protected override void WorkBehavior()
    {
        currentState = BeeState.Idle;
    }

    protected override void ReturnBehavior()
    {
        currentState = BeeState.Idle;
    }

    protected override void Die()
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);
        }

        Time.timeScale = 0f;

        currentState = BeeState.Dead;
        Destroy(gameObject);
    }

    protected override void AssignZone()
    {
        QueenZone queenZone = FindObjectOfType<QueenZone>();

        if (queenZone == null)
            return;

        assignedZone = queenZone;
        homePosition = queenZone.transform.position;

        queenZone.RegisterBee(this);

        // OPTIONAL (recommended if your system uses it)
        if (ZoneManager.Instance != null)
        {
            ZoneManager.Instance.RegisterBee(this);
        }
    }
}