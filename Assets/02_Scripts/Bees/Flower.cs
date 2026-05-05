using UnityEngine;

public class Flower : MonoBehaviour
{
    public bool IsAvailable = true;

    public void Reserve()
    {
        IsAvailable = false;
    }

    public void Release()
    {
        IsAvailable = true;
    }
}