using UnityEngine;
using System.Collections.Generic;

public class Pollen : MonoBehaviour
{
    public static List<Pollen> allPollen = new List<Pollen>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        allPollen.Clear();
    }

    public bool isClaimed = false; // ✅ IMPORTANT

    private void OnEnable()
    {
        allPollen.Add(this);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 1000;
    }

    private void OnDisable()
    {
        allPollen.Remove(this);
    }
}