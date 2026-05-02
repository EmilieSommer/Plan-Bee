using UnityEngine;

public class HouseBeeZone : Zone
{
    [Header("Zone Settings")]
    public float depositRadius = 2f;

    private bool isActive = false;
    public bool IsActive => isActive;

    private void Awake()
    {
        zoneType = Bee.BeeType.House;

        // Ensure this zone accepts BOTH House + Forager bees
        SetupCapacities();

        isActive = true;
    }

    void SetupCapacities()
    {
        bool hasHouse = false;
        bool hasForager = false;

        foreach (var limit in limits)
        {
            if (limit.type == Bee.BeeType.House)
                hasHouse = true;

            if (limit.type == Bee.BeeType.Forager)
                hasForager = true;
        }

        if (!hasHouse)
        {
            limits.Add(new BeeTypeLimit
            {
                type = Bee.BeeType.House,
                capacity = 5,
                current = 0
            });
        }

        if (!hasForager)
        {
            limits.Add(new BeeTypeLimit
            {
                type = Bee.BeeType.Forager,
                capacity = 5,
                current = 0
            });
        }
    }

    public void SetInactive()
    {
        isActive = false;
    }

    public void ActivateZone()
    {
        isActive = true;
    }

    public Vector2 GetDepositPoint()
    {
        return (Vector2)transform.position + Random.insideUnitCircle * depositRadius;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, depositRadius);
    }
}