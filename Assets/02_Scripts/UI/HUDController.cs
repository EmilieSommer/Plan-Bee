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
        if (queenCountText == null)   queenCountText = FindTextRobust("queen");
        if (nurseCountText == null)   nurseCountText = FindTextRobust("nurse");
        if (builderCountText == null) builderCountText = FindTextRobust("builder");
        if (houseCountText == null)   houseCountText = FindTextRobust("house");
        if (foragerCountText == null) foragerCountText = FindTextRobust("forager");
        if (droneCountText == null)   droneCountText = FindTextRobust("drone");
        if (totalCountText == null)   totalCountText = FindTextRobust("total", "capacity", "hive");
    }

    private TMP_Text FindTextRobust(params string[] keywords)
    {
        TMP_Text[] allTexts = GetComponentsInChildren<TMP_Text>(true);
        foreach(var t in allTexts)
        {
            string n = t.gameObject.name.ToLower();
            foreach(string kw in keywords)
            {
                if (n.Contains(kw) && (n.Contains("count") || n.Contains("text") || n.Contains("num")))
                {
                    return t;
                }
            }
        }
        return null;
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
