using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }

    private readonly Queue<ConstructionSite> queue = new Queue<ConstructionSite>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddToQueue(ConstructionSite site)
    {
        if (site == null) return;
        queue.Enqueue(site);
    }
}
