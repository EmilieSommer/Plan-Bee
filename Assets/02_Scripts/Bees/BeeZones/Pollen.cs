using UnityEngine;
using System.Collections.Generic;

public class Pollen : MonoBehaviour
{
    public static List<Pollen> allPollen = new List<Pollen>();

    public int amount = 5;

    private void OnEnable()
    {
        allPollen.Add(this);
    }

    private void OnDisable()
    {
        allPollen.Remove(this);
    }
}