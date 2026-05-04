using UnityEngine;

public class ForagerBee : Bee
{
    [Header("Foraging")]
    public float foragingTime = 5f;
    public int pollenPerTrip = 5;

    [Header("Prefabs")]
    public GameObject pollenPrefab;

    [Header("Weather Effects")]
    public float rainSlowMultiplier = 2f;

    private float timer;
    private float baseMoveSpeed;

    private bool isOutForaging = false;
    private bool shelteringFromSnow = false;

    private HouseBeeZone currentZone;

    protected override void Awake()
    {
        base.Awake();

        beeType = BeeType.Forager;
        baseMoveSpeed = moveSpeed;

        StartForaging();
    }

    void StartForaging()
    {
        isOutForaging = true;
        shelteringFromSnow = false;

        currentState = BeeState.Moving;

        timer = foragingTime;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        targetPosition = (Vector2)transform.position + randomDir * 20f;
    }

    protected override void Update()
    {
        if (currentState == BeeState.Dead)
            return;

        // ❄ SNOW BEHAVIOR
        if (WinterSystem.Instance != null && WinterSystem.Instance.isSnowing)
        {
            HandleSnow();

            // IMPORTANT: only move until they reach shelter
            if (currentState != BeeState.Working)
            {
                base.Update();
            }

            return;
        }

        // Resume after snow
        if (shelteringFromSnow)
        {
            StartForaging();
        }

        // ☔ RAIN SLOWDOWN
        float weatherSlow = 1f;

        if (RainSystem.Instance != null && IsRaining())
        {
            weatherSlow = rainSlowMultiplier;
        }

        moveSpeed = baseMoveSpeed / weatherSlow;

        if (isOutForaging)
        {
            timer -= Time.deltaTime / weatherSlow;

            if (timer <= 0f)
            {
                StartReturning();
            }

            return;
        }

        base.Update();
    }

    bool IsRaining()
    {
        return RainSystem.Instance != null &&
               RainSystem.Instance.GetCurrentEmission() > 0f;
    }

    // ❄ Snow: go to house and stop there
    void HandleSnow()
    {
        if (!shelteringFromSnow)
        {
            shelteringFromSnow = true;
            isOutForaging = false;

            currentZone = FindNearestHouseZone();

            if (currentZone != null)
            {
                targetPosition = currentZone.transform.position;
                currentState = BeeState.Returning;
            }
        }

        float dist = Vector2.Distance(transform.position, targetPosition);

        if (dist < 0.2f)
        {
            currentState = BeeState.Working; // fully stop here
        }
    }

    void StartReturning()
    {
        isOutForaging = false;

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
            if (WinterSystem.Instance != null && WinterSystem.Instance.isSnowing)
            {
                currentState = BeeState.Working;
                return;
            }

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
            if (!zone.IsActive)
                continue;

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
        base.Die();
    }

    protected override void WorkBehavior() { }
}