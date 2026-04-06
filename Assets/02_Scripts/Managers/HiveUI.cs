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

        totalText.text = "Total Bees: " + HiveManager.Instance.GetTotalBees();

        queenText.text = "Queen: " + GetCount(Bee.BeeType.Queen);
        houseText.text = "House: " + GetCount(Bee.BeeType.House);
        nurseText.text = "Nurse: " + GetCount(Bee.BeeType.Nurse);
        foragerText.text = "Forager: " + GetCount(Bee.BeeType.Forager);
        droneText.text = "Drone: " + GetCount(Bee.BeeType.Drone);
        builderText.text = "Builder: " + GetCount(Bee.BeeType.Builder);
    }

    int GetCount(Bee.BeeType type)
    {
        return HiveManager.Instance.GetBeeCount(type);
    }
}