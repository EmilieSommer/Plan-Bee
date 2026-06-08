using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Colony — assigned by HUDBuilder")]
    public TMP_Text queenCountText;
    public TMP_Text nurseCountText;
    public TMP_Text builderCountText;
    public TMP_Text houseCountText;
    public TMP_Text foragerCountText;
    public TMP_Text droneCountText;
    public TMP_Text totalCountText;

    [Header("Controls")]
    public TMP_Text speedLabel;

    private readonly float[] speedOptions = { 1f, 2f, 3f };
    private int currentSpeedIdx = 0;

    private void Start()
    {
        // Auto-find references if the user didn't rebuild the UI
        if (queenCountText == null)   queenCountText = FindText("QueenCount");
        if (nurseCountText == null)   nurseCountText = FindText("NurseCount");
        if (builderCountText == null) builderCountText = FindText("BuilderCount");
        if (houseCountText == null)   houseCountText = FindText("HouseCount");
        if (foragerCountText == null) foragerCountText = FindText("ForagerCount");
        if (droneCountText == null)   droneCountText = FindText("DroneCount");
        if (totalCountText == null)   totalCountText = FindText("TotalCount");
    }

    private TMP_Text FindText(string n)
    {
        GameObject go = GameObject.Find(n);
        return go != null ? go.GetComponent<TMP_Text>() : null;
    }

    private void Update()
    {
        if (HiveManager.Instance == null) return;
        int total = 0;
        total += SetCount(queenCountText,   Bee.BeeType.Queen);
        total += SetCount(nurseCountText,   Bee.BeeType.Nurse);
        total += SetCount(builderCountText, Bee.BeeType.Builder);
        total += SetCount(houseCountText,   Bee.BeeType.House);
        total += SetCount(foragerCountText, Bee.BeeType.Forager);
        total += SetCount(droneCountText,   Bee.BeeType.Drone);
        if (totalCountText != null) 
        {
            totalCountText.text = $"TOTAL: {total} / {HiveManager.Instance.GetHiveCapacity()}";
        }
    }

    private int SetCount(TMP_Text txt, Bee.BeeType type)
    {
        int c = HiveManager.Instance.GetBeeCount(type);
        if (txt != null) txt.text = c.ToString();
        return c;
    }

    // Called by Speed button OnClick
    public void CycleSpeed()
    {
        currentSpeedIdx = (currentSpeedIdx + 1) % speedOptions.Length;
        Time.timeScale = speedOptions[currentSpeedIdx];
        if (speedLabel != null) speedLabel.text = $"{(int)speedOptions[currentSpeedIdx]}x";
    }
}
