using UnityEngine;

public class HouseBeeZone : Zone
{
    private bool isActive = false;
    public bool IsActive => isActive;

    protected override void Awake()
    {
        base.Awake();
        zoneType = Bee.BeeType.House;
        isStorageZone = true; // Act as a valid drop-off for Foragers!
        depositRadius = 2f; // specific radius for house

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

    protected override void Start()
    {
        base.Start();
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddCapacity(10, 10);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.RemoveCapacity(10, 10);
    }

    public void SetInactive()
    {
        isActive = false;
    }

    public void ActivateZone()
    {
        isActive = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, depositRadius);
    }
}