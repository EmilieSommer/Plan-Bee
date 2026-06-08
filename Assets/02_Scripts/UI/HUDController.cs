using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Colony UI References (Accepts any GameObject with TMP_Text or Legacy Text)")]
    public GameObject queenCountText;
    public GameObject nurseCountText;
    public GameObject builderCountText;
    public GameObject houseCountText;
    public GameObject foragerCountText;
    public GameObject droneCountText;
    public GameObject totalCountText;

    [Header("Controls")]
    public GameObject speedLabel;

    private readonly float[] speedOptions = { 1f, 2f, 3f };
    private int currentSpeedIdx = 0;

    private void Update()
    {
        if (HiveManager.Instance == null) return;

        SetCount(queenCountText,   "Queen",   Bee.BeeType.Queen);
        SetCount(nurseCountText,   "Nurse",   Bee.BeeType.Nurse);
        SetCount(builderCountText, "Builder", Bee.BeeType.Builder);
        SetCount(houseCountText,   "House",   Bee.BeeType.House);
        SetCount(foragerCountText, "Forager", Bee.BeeType.Forager);
        SetCount(droneCountText,   "Drone",   Bee.BeeType.Drone);

        if (totalCountText != null)
        {
            int total = HiveManager.Instance.GetTotalBees();
            int cap   = HiveManager.Instance.GetHiveCapacity();
            int eggs  = HiveManager.Instance.QueuedEggs;
            if (eggs > 0)
                SetTextValue(totalCountText, $"{total} (+{eggs} Eggs) / {cap}");
            else
                SetTextValue(totalCountText, $"{total} / {cap}");
        }
    }

    private void SetCount(GameObject obj, string label, Bee.BeeType type)
    {
        if (obj == null) return;
        int count = HiveManager.Instance.GetBeeCount(type);
        SetTextValue(obj, $"{label,-8}  {count}");
    }

    private void SetTextValue(GameObject obj, string value)
    {
        if (obj == null) return;

        // Try TextMeshPro first
        var tmp = obj.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            tmp.text = value;
            return;
        }

        // Try Legacy Text
        var legacyText = obj.GetComponentInChildren<UnityEngine.UI.Text>(true);
        if (legacyText != null)
        {
            legacyText.text = value;
            return;
        }
    }

    // Called by Speed button OnClick
    public void CycleSpeed()
    {
        currentSpeedIdx = (currentSpeedIdx + 1) % speedOptions.Length;
        Time.timeScale = speedOptions[currentSpeedIdx];
        if (speedLabel != null)
        {
            SetTextValue(speedLabel, $"{(int)speedOptions[currentSpeedIdx]}x");
        }
    }
}
