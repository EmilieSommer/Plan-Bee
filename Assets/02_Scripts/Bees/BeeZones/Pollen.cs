using UnityEngine;
using System.Collections.Generic;

public class Pollen : MonoBehaviour
{
    public static List<Pollen> allPollen = new List<Pollen>();

    public bool isClaimed = false; // ✅ IMPORTANT

    private void OnEnable()
    {
        allPollen.Add(this);
    }

    private void OnDisable()
    {
        allPollen.Remove(this);
    }
}