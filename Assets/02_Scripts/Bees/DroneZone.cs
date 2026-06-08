using UnityEngine;

public class DroneZone : Zone
{

    protected override void Awake()
    {
        base.Awake();
        zoneType = Bee.BeeType.Drone;
        
        // Ensure it has Drone capacity!
        bool hasDroneLimit = false;
        foreach (var limit in limits)
        {
            if (limit.type == Bee.BeeType.Drone)
            {
                hasDroneLimit = true;
                break;
            }
        }

        if (!hasDroneLimit)
        {
            limits.Add(new BeeTypeLimit
            {
                type = Bee.BeeType.Drone,
                capacity = 1,
                current = 0
            });
        }
    }


}