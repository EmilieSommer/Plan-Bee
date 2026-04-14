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

        // IMPORTANT:
        // If the zone already exists in the scene, it's a "default zone"
        // so it should already be active
        isActive = true;
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
        Vector2 randomPoint = (Vector2)transform.position + Random.insideUnitCircle * depositRadius;
        return randomPoint;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, depositRadius);
    }
}