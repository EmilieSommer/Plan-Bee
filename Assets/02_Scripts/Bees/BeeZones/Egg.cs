using UnityEngine;
using System.Collections.Generic;

public class Egg : MonoBehaviour
{
    public static List<Egg> allEggs = new List<Egg>();

    public System.Action OnHatched; // ✅ event

    private void OnEnable()
    {
        allEggs.Add(this);
    }

    private void OnDisable()
    {
        allEggs.Remove(this);
    }

    [Header("Egg Settings")]
    public float hatchTime = 10f;

    private float timer;

    public GameObject beePrefab;

    [Header("Tending")]
    public int nursesTending = 0;
    public float tendSpeedMultiplier = 1f;

    private void Start()
    {
        timer = hatchTime;
    }

    private void Update()
    {
        if (nursesTending > 0)
        {
            float speed = nursesTending * tendSpeedMultiplier;
            timer -= Time.deltaTime * speed;

            if (timer <= 0f)
            {
                Hatch();
            }
        }
    }

    public void AddNurse()
    {
        nursesTending++;
    }

    public void RemoveNurse()
    {
        nursesTending = Mathf.Max(0, nursesTending - 1);
    }

    public bool IsHatched()
    {
        return timer <= 0f;
    }

    void Hatch()
    {
        OnHatched?.Invoke(); // ✅ notify UI before destruction

        Instantiate(beePrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}