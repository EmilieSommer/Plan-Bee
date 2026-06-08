using UnityEngine;
using TMPro;

public class HiveUI : MonoBehaviour
{
    [Header("Drag dine objekter ind her!")]
    public GameObject totalTextObj;
    public GameObject queenTextObj;
    public GameObject houseTextObj;
    public GameObject nurseTextObj;
    public GameObject foragerTextObj;
    public GameObject droneTextObj;
    public GameObject builderTextObj;

    void Update()
    {
        if (HiveManager.Instance == null) return;

        UpdateText(totalTextObj, HiveManager.Instance.GetTotalBees() + " / " + HiveManager.Instance.GetHiveCapacity());

        UpdateText(queenTextObj, GetCount(Bee.BeeType.Queen).ToString());
        UpdateText(houseTextObj, GetCount(Bee.BeeType.House).ToString());
        UpdateText(nurseTextObj, GetCount(Bee.BeeType.Nurse).ToString());
        UpdateText(foragerTextObj, GetCount(Bee.BeeType.Forager).ToString());
        UpdateText(droneTextObj, GetCount(Bee.BeeType.Drone).ToString());
        UpdateText(builderTextObj, GetCount(Bee.BeeType.Builder).ToString());
    }

    void UpdateText(GameObject obj, string value)
    {
        if (obj == null) return;
        
        var tmp = obj.GetComponentInChildren<TMP_Text>();
        if (tmp != null) 
        {
            tmp.text = value;
            return;
        }

        var txt = obj.GetComponentInChildren<UnityEngine.UI.Text>();
        if (txt != null)
        {
            txt.text = value;
        }
    }

    int GetCount(Bee.BeeType type)
    {
        return HiveManager.Instance.GetBeeCount(type);
    }
}