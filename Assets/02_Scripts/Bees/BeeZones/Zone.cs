using UnityEngine;
using System.Collections.Generic;

public class Zone : MonoBehaviour
{
    public Bee.BeeType zoneType;

    [System.Serializable]
    public class BeeTypeLimit
    {
        public Bee.BeeType type;
        public int capacity;
        public int current;
    }

    [Header("Capacity")]
    public List<BeeTypeLimit> limits = new List<BeeTypeLimit>();

    private Dictionary<Bee.BeeType, BeeTypeLimit> lookup;

    private void Awake()
    {
        lookup = new Dictionary<Bee.BeeType, BeeTypeLimit>();

        foreach (var limit in limits)
            lookup[limit.type] = limit;
    }

    protected virtual void Start()
    {
        if (ZoneManager.Instance != null)
            ZoneManager.Instance.RegisterZone(this);
    }

    protected virtual void OnDestroy()
    {
        if (ZoneManager.Instance != null)
            ZoneManager.Instance.UnregisterZone(this);
    }

    public bool CanAccept(Bee.BeeType type)
    {
        if (!lookup.ContainsKey(type)) return false;

        BeeTypeLimit limit = lookup[type];
        return limit.current < limit.capacity;
    }

    public void RegisterBee(Bee bee)
    {
        if (!lookup.ContainsKey(bee.beeType)) return;

        BeeTypeLimit limit = lookup[bee.beeType];

        if (limit.current >= limit.capacity)
            return;

        limit.current++;
    }

    public void UnregisterBee(Bee bee)
    {
        if (!lookup.ContainsKey(bee.beeType)) return;

        BeeTypeLimit limit = lookup[bee.beeType];
        limit.current = Mathf.Max(0, limit.current - 1);
    }

    public float GetFillRatio(Bee.BeeType type)
    {
        if (!lookup.ContainsKey(type)) return 0f;

        BeeTypeLimit limit = lookup[type];

        if (limit.capacity == 0)
            return 1f;

        return (float)limit.current / limit.capacity;
    }
}