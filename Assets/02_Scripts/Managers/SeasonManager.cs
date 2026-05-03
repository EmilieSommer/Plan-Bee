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

    [Header("Season Profiles")]
    public SeasonProfile spring;
    public SeasonProfile summer;
    public SeasonProfile autumn;
    public SeasonProfile winter;

    public Season currentSeason = Season.Spring;
    private Season previousSeason;

    private GameObject activeWeather;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateSeason(true);
    }

    private void Update()
    {
        UpdateSeason();
    }

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

    void OnSeasonChanged()
    {
        StopAllCoroutines();
        StartCoroutine(ShowSeasonPopup());

        ApplyWeather();
        UpdateSeasonWeather(); // IMPORTANT
    }

    void UpdateSeasonWeather()
    {
        // reset weather first
        RainSystem.Instance?.StopRain();
        WinterSystem.Instance?.StopWinter();

        switch (currentSeason)
        {
            case Season.Spring:
                if (Random.value < 0.3f)
                    RainSystem.Instance?.StartRain();
                break;

            case Season.Summer:
                if (Random.value < 0.15f)
                    RainSystem.Instance?.StartRain();
                break;

            case Season.Autumn:
                if (Random.value < 0.5f)
                    RainSystem.Instance?.StartRain();
                break;

            case Season.Winter:
                WinterSystem.Instance?.StartWinter();
                break;
        }
    }

    IEnumerator ShowSeasonPopup()
    {
        GameObject popup = GetPopup(currentSeason);

        if (popup != null)
            popup.SetActive(true);

        yield return new WaitForSeconds(3f);

        if (popup != null)
            popup.SetActive(false);
    }

    void ApplyWeather()
    {
        if (activeWeather != null)
            Destroy(activeWeather);

        SeasonProfile profile = GetCurrentProfile();

        if (profile != null && profile.weatherParticles != null)
        {
            activeWeather = Instantiate(profile.weatherParticles);
        }
    }

    GameObject GetPopup(Season season)
    {
        return season switch
        {
            Season.Spring => springPopup,
            Season.Summer => summerPopup,
            Season.Autumn => autumnPopup,
            Season.Winter => winterPopup,
            _ => null
        };
    }

    void UpdateUI()
    {
        if (seasonText != null)
            seasonText.text = currentSeason.ToString();

        if (seasonDescriptionText != null)
            seasonDescriptionText.text = GetSeasonDescription(currentSeason);
    }

    string GetSeasonDescription(Season season)
    {
        return season switch
        {
            Season.Spring => "A calm season. Bees expand and flowers bloom.",
            Season.Summer => "High activity season. Heavy enemy pressure.",
            Season.Autumn => "Resources fluctuate. Prepare for collapse.",
            Season.Winter => "Survival mode. Snow and starvation risk.",
            _ => ""
        };
    }

    public SeasonProfile GetCurrentProfile()
    {
        return currentSeason switch
        {
            Season.Spring => spring,
            Season.Summer => summer,
            Season.Autumn => autumn,
            Season.Winter => winter,
            _ => spring
        };
    }
}