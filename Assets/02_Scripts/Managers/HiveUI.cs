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

        queenText.text = GetCount(Bee.BeeType.Queen).ToString();
        houseText.text = GetCount(Bee.BeeType.House).ToString();
        nurseText.text = GetCount(Bee.BeeType.Nurse).ToString();
        foragerText.text = GetCount(Bee.BeeType.Forager).ToString();
        droneText.text = GetCount(Bee.BeeType.Drone).ToString();
        builderText.text = GetCount(Bee.BeeType.Builder).ToString();
    }

    int GetCount(Bee.BeeType type)
    {
        return HiveManager.Instance.GetBeeCount(type);
    }
}