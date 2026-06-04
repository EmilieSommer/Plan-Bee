using UnityEngine;
using TMPro;

public class HiveUI : MonoBehaviour
{
    public TextMeshProUGUI totalText;
    public TextMeshProUGUI queenText;
    public TextMeshProUGUI houseText;
    public TextMeshProUGUI nurseText;
    public TextMeshProUGUI foragerText;
    public TextMeshProUGUI droneText;
    public TextMeshProUGUI builderText;

    void Update()
    {
        if (HiveManager.Instance == null) return;

        // TOTAL: Bees / Capacity
        totalText.text =
            HiveManager.Instance.GetTotalBees() +
            " / " +
            HiveManager.Instance.GetHiveCapacity();

        if (queenText != null) queenText.text = GetCount(Bee.BeeType.Queen).ToString();
        if (houseText != null) houseText.text = GetCount(Bee.BeeType.House).ToString();
        if (nurseText != null) nurseText.text = GetCount(Bee.BeeType.Nurse).ToString();
        if (foragerText != null) foragerText.text = GetCount(Bee.BeeType.Forager).ToString();
        if (droneText != null) droneText.text = GetCount(Bee.BeeType.Drone).ToString();
        if (builderText != null) builderText.text = GetCount(Bee.BeeType.Builder).ToString();
    }

    int GetCount(Bee.BeeType type)
    {
        return HiveManager.Instance.GetBeeCount(type);
    }
}