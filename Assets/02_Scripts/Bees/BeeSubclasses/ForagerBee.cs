using UnityEngine;

public class ForagerBee : Bee
{
    [Header("Foraging")]
    public float foragingTime = 5f;
    public int pollenPerTrip = 5;

    [Header("Prefabs")]
    public GameObject pollenPrefab;

    private float timer;

    private bool isOutForaging = false;
    private bool isReturning = false;

    private Vector2 offscreenTarget;

    private HouseBeeZone currentZone;

    protected override void Awake()
    {
        base.Awake();

        beeType = BeeType.Forager;

        StartForaging();
    }

    void StartForaging()
    {
        isOutForaging = true;
        isReturning = false;

        currentState = BeeState.Moving;

        timer = foragingTime;

        // Pick a random direction to leave the screen
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        offscreenTarget = (Vector2)transform.position + randomDir * 20f;

        targetPosition = offscreenTarget;
    }

    protected override void Update()
    {
        if (currentState == BeeState.Dead)
            return;

        if (isOutForaging)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                StartReturning();
            }

            return;
        }

        base.Update();
    }

    void StartReturning()
    {
        isOutForaging = false;
        isReturning = true;

        currentState = BeeState.Returning;

        currentZone = FindNearestHouseZone();

        if (currentZone != null)
        {
            targetPosition = currentZone.transform.position;
        }
        else
        {
            targetPosition = homePosition;
        }
    }

    protected override void ReturnBehavior()
    {
        float dist = Vector2.Distance(transform.position, targetPosition);

        if (dist < 0.2f)
        {
            DepositPollen();
        }
    }

    void DepositPollen()
    {
        if (pollenPrefab != null && currentZone != null)
        {
            Instantiate(
                pollenPrefab,
                currentZone.GetDepositPoint(),
                Quaternion.identity
            );
        }

        CurrencyManager.Instance.pollen += pollenPerTrip;

        StartForaging();
    }

    HouseBeeZone FindNearestHouseZone()
    {
        HouseBeeZone[] zones = FindObjectsOfType<HouseBeeZone>();

        float closestDist = Mathf.Infinity;
        HouseBeeZone closest = null;

        foreach (var zone in zones)
        {
            float dist = Vector2.Distance(transform.position, zone.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = zone;
            }
        }

        return closest;
    }

    protected override void Die()
    {
        // custom behavior (e.g. particles)

        base.Die(); // VERY IMPORTANT
    }

    // REQUIRED BY BASE CLASS
    protected override void WorkBehavior() { }
}   