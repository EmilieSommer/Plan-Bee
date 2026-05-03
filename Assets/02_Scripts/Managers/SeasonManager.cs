using UnityEngine;
using TMPro;
using System.Collections;

public class SeasonManager : MonoBehaviour
{
    public static SeasonManager Instance;

    [Header("Season Settings")]
    public int daysPerSeason = 5;

    [Header("UI")]
    public TextMeshProUGUI seasonText;
    public TextMeshProUGUI seasonDescriptionText;

    [Header("Season Popups")]
    public GameObject springPopup;
    public GameObject summerPopup;
    public GameObject autumnPopup;
    public GameObject winterPopup;

    public Season currentSeason = Season.Spring;
    private Season previousSeason;

    private bool showingPopup = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateSeason(force: true);
    }

    private void Update()
    {
        UpdateSeason();
    }

    // -----------------------------
    // SEASON LOGIC
    // -----------------------------
    void UpdateSeason(bool force = false)
    {
        int day = DayCycleManager.Instance.currentDay;

        int seasonIndex = (day - 1) / daysPerSeason;
        int seasonEnumIndex = seasonIndex % 4;

        currentSeason = (Season)seasonEnumIndex;

        if (force || currentSeason != previousSeason)
        {
            previousSeason = currentSeason;
            OnSeasonChanged();
        }

        UpdateUI();
    }

    // -----------------------------
    // SEASON CHANGE EVENT
    // -----------------------------
    void OnSeasonChanged()
    {
        StopAllCoroutines();
        StartCoroutine(ShowSeasonPopup());
    }

    IEnumerator ShowSeasonPopup()
    {
        showingPopup = true;

        HideAllPopups();

        GameObject popup = GetPopup(currentSeason);

        if (popup != null)
            popup.SetActive(true);

        // show for 3 seconds (no freezing gameplay!)
        yield return new WaitForSeconds(3f);

        if (popup != null)
            popup.SetActive(false);

        showingPopup = false;
    }

    GameObject GetPopup(Season season)
    {
        switch (season)
        {
            case Season.Spring: return springPopup;
            case Season.Summer: return summerPopup;
            case Season.Autumn: return autumnPopup;
            case Season.Winter: return winterPopup;
        }

        return null;
    }

    // -----------------------------
    // UI
    // -----------------------------
    void UpdateUI()
    {
        if (seasonText != null)
            seasonText.text = currentSeason.ToString();

        if (seasonDescriptionText != null)
            seasonDescriptionText.text = GetSeasonDescription(currentSeason);
    }

    string GetSeasonDescription(Season season)
    {
        switch (season)
        {
            case Season.Spring:
                return "A calm season. Bees are active and flowers bloom.";

            case Season.Summer:
                return "High activity season. More enemies appear.";

            case Season.Autumn:
                return "Resources are abundant but weather becomes unstable.";

            case Season.Winter:
                return "Harsh conditions. Snowstorms and reduced visibility.";
        }

        return "";
    }

    // -----------------------------
    // ENEMIES (FIXED FOR YOUR SPAWNER)
    // -----------------------------
    public EnemyType[] GetAllowedEnemies()
    {
        switch (currentSeason)
        {
            case Season.Spring:
                return new EnemyType[]
                {
                    EnemyType.VarroaMite,
                    EnemyType.RobberBee,
                    EnemyType.Skunk
                };

            case Season.Summer:
                return new EnemyType[]
                {
                    EnemyType.VarroaMite,
                    EnemyType.HiveBeetle,
                    EnemyType.Ant,
                    EnemyType.Wasp,
                    EnemyType.RobberBee
                };

            case Season.Autumn:
                return new EnemyType[]
                {
                    EnemyType.VarroaMite,
                    EnemyType.WaxMoth,
                    EnemyType.Bear,
                    EnemyType.RobberBee
                };

            case Season.Winter:
                return new EnemyType[]
                {
                    EnemyType.VarroaMite,
                    EnemyType.Mouse,
                    EnemyType.RobberBee
                };
        }

        return new EnemyType[] { EnemyType.VarroaMite };
    }

    void HideAllPopups()
    {
        if (springPopup) springPopup.SetActive(false);
        if (summerPopup) summerPopup.SetActive(false);
        if (autumnPopup) autumnPopup.SetActive(false);
        if (winterPopup) winterPopup.SetActive(false);
    }
}