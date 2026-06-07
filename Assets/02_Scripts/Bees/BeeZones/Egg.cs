using UnityEngine;
using System.Collections.Generic;

public class Egg : MonoBehaviour
{
    public static List<Egg> allEggs = new List<Egg>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        allEggs.Clear();
    }

    public System.Action OnHatched;

    [Header("Egg Settings")]
    public float hatchTime = 10f;

    private float timer;

    public GameObject beePrefab;

    [Header("Tending")]
    public float tendSpeedMultiplier = 1f;

    // 🐝 Only ONE nurse working
    private NurseBee currentNurse = null;

    // 🔒 Only ONE nurse targeting
    private NurseBee reservedNurse = null;

    public string eggName;

    private bool hasName = false;

    public void SetName(string newName)
    {
        eggName = newName;
        hasName = true;

        Debug.Log("Egg named: " + eggName);
    }

    public float GetTimeRemaining()
    {
        return Mathf.Max(timer, 0f);
    }

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
        // ❌ cannot hatch if not named
        if (!hasName)
            return;

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

    public void ClearReservation(NurseBee nurse)
    {
        if (reservedNurse == nurse)
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

        // ✅ Only the reserving nurse can assign
        if (reservedNurse != nurse)
            return false;

        currentNurse = nurse;

        // reservation no longer needed
        reservedNurse = null;

        return true;
    }

    public void RemoveNurse(NurseBee nurse)
    {
        if (currentNurse == nurse)
        {
            currentNurse = null;
        }

        // also clear reservation if same nurse
        if (reservedNurse == nurse)
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
        if (!hasName)
            return;

        // 🧠 release reserved capacity
        HiveManager.Instance.UnregisterQueuedEgg();

        OnHatched?.Invoke();

        GameObject beeObj = Instantiate(beePrefab, transform.position, Quaternion.identity);

        Bee bee = beeObj.GetComponent<Bee>();
        if (bee != null)
        {
            bee.SetName(eggName);
        }

        Destroy(gameObject);
    }
}