using System.Collections.Generic;
using UnityEngine;

public class HiveManager : MonoBehaviour
{
    public static HiveManager Instance;

    private Dictionary<Bee.BeeType, int> beeCounts = new Dictionary<Bee.BeeType, int>();

    private int totalBees = 0;

    private void Awake()
    {
        Instance = this;

        // Initialize all bee types
        foreach (Bee.BeeType type in System.Enum.GetValues(typeof(Bee.BeeType)))
        {
            beeCounts[type] = 0;
        }
    }

    // ------------------------
    // REGISTER
    // ------------------------

    public void RegisterBee(Bee bee)
    {
        totalBees++;
        beeCounts[bee.beeType]++;
    }

    public void UnregisterBee(Bee bee)
    {
        totalBees--;
        beeCounts[bee.beeType]--;
    }

    // ------------------------
    // GETTERS
    // ------------------------

    public int GetTotalBees()
    {
        return totalBees;
    }

    public int GetBeeCount(Bee.BeeType type)
    {
        return beeCounts[type];
    }
}