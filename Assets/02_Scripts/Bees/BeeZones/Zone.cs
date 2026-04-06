using UnityEngine;
using System.Collections.Generic;

public class Zone : MonoBehaviour
{
    public Bee.BeeType zoneType;

    [Header("Capacity")]
    public int capacity = 3;
    private List<Bee> beesInZone = new List<Bee>();


    protected virtual void Start()
    {
        if (ZoneManager.Instance != null)
        {
            ZoneManager.Instance.RegisterZone(this);
        }
    }

    protected virtual void OnDestroy()
    {
        if (ZoneManager.Instance != null)
        {
            ZoneManager.Instance.UnregisterZone(this);
        }
    }

    public bool IsFull()
    {
        return beesInZone.Count >= capacity;
    }

    public void RegisterBee(Bee bee)
    {
        if (!beesInZone.Contains(bee))
            beesInZone.Add(bee);
    }

    public void UnregisterBee(Bee bee)
    {
        beesInZone.Remove(bee);
    }
    public float GetFillRatio()
    {
        return (float)beesInZone.Count / capacity;
    }
}