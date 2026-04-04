using UnityEngine;
using System.Collections.Generic;

public class Egg : MonoBehaviour
{
    public static List<Egg> allEggs = new List<Egg>();

    public System.Action OnHatched;

    [Header("Egg Settings")]
    public float hatchTime = 10f;

    private float timer;

    public GameObject beePrefab;

    [Header("Tending")]
    public float tendSpeedMultiplier = 1f;

    // 🐝 Only ONE nurse allowed
    private NurseBee currentNurse = null;

    // 🔒 Reservation system (prevents multiple bees targeting same egg)
    private NurseBee reservedNurse = null;

    private void OnEnable()
    {
        allEggs.Add(this);
    }

    private void OnDisable()
    {
        allEggs.Remove(this);
    }

    private void Start()
    {
        timer = hatchTime;
    }

    private void Update()
    {
        if (currentNurse != null)
        {
            timer -= Time.deltaTime * tendSpeedMultiplier;

            if (timer <= 0f)
            {
                Hatch();
            }
        }
    }

    // ---------------------------
    // RESERVATION SYSTEM
    // ---------------------------

    public bool HasNurse()
    {
        return currentNurse != null;
    }

    public bool IsReserved()
    {
        return reservedNurse != null;
    }

    public bool TryReserve(NurseBee nurse)
    {
        if (reservedNurse != null)
            return false;

        reservedNurse = nurse;
        return true;
    }

    public void ClearReservation()
    {
        if (currentNurse == null)
        {
            reservedNurse = null;
        }
    }

    // ---------------------------
    // NURSE ASSIGNMENT
    // ---------------------------

    public bool TryAssignNurse(NurseBee nurse)
    {
        if (currentNurse != null)
            return false;

        currentNurse = nurse;
        return true;
    }

    public void RemoveNurse()
    {
        currentNurse = null;

        // Only clear reservation if no nurse is present
        if (reservedNurse != null)
        {
            reservedNurse = null;
        }
    }

    // ---------------------------
    // STATE
    // ---------------------------

    public bool IsHatched()
    {
        return timer <= 0f;
    }

    // ---------------------------
    // HATCH
    // ---------------------------

    void Hatch()
    {
        OnHatched?.Invoke();

        Instantiate(beePrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}