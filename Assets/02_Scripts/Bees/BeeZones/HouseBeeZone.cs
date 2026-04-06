using UnityEngine;

public class HouseBeeZone : Zone
{
    [Header("Zone Settings")]
    public float depositRadius = 2f;

    private void Awake()
    {
        zoneType = Bee.BeeType.House;
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