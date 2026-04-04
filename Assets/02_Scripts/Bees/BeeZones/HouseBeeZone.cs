using UnityEngine;

public class HouseBeeZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public float depositRadius = 2f;

    public Vector2 GetDepositPoint()
    {
        // Return a random point inside the circle
        Vector2 randomPoint = (Vector2)transform.position + Random.insideUnitCircle * depositRadius;
        return randomPoint;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, depositRadius);
    }
}