using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    private Queue<ConstructionSite> queue = new Queue<ConstructionSite>();

    public ConstructionSite activeSite;

    private void Awake()
    {
        Instance = this;
    }

    public void AddToQueue(ConstructionSite site)
    {
        queue.Enqueue(site);

        if (activeSite == null)
        {
            StartNext();
        }
    }

    void StartNext()
    {
        if (queue.Count == 0)
        {
            activeSite = null;
            return;
        }

        activeSite = queue.Dequeue();
    }

    public void FinishCurrent()
    {
        StartNext();
    }
}