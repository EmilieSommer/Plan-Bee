using UnityEngine;

public class Wasp : Enemy
{
    [Header("Wasp Settings")]
    public float aggroRange = 6f;

    protected override void Awake()
    {
        base.Awake();
        moveSpeed = 3.5f; // faster than default enemies
    }

    protected override void Update()
    {
        base.Update(); // uses Enemy AI (move + attack bees)
    }

    protected override void FindNearestBee()
    {
        // optional override if you want special targeting later
        base.FindNearestBee();
    }

    protected override void Attack()
    {
        base.Attack(); // or replace later with "sting effect"
    }
}