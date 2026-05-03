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

    [Header("Weather Settings Per Season")]
    public WeatherSettings springWeather;
    public WeatherSettings summerWeather;
    public WeatherSettings autumnWeather;
    public WeatherSettings winterWeather;

    public Season currentSeason = Season.Spring;
    private Season previousSeason;

    private enum WeatherState
    {
        Dry,
        Rain
    }

    private WeatherState weatherState;
    private float weatherTimer;

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
        UpdateWeatherLoop();
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

    void OnSeasonChanged()
    {
        StopAllCoroutines();
        StartCoroutine(ShowSeasonPopup());

        ApplyWeather();
        InitializeWeather();
    }

    void InitializeWeather()
    {
        weatherState = WeatherState.Dry;
        weatherTimer = 0f;

        RainSystem.Instance?.StopRain();
        WinterSystem.Instance?.StopWinter();

        SetNextWeatherState();
    }

    // -----------------------------
    // WEATHER LOOP
    // -----------------------------
    void UpdateWeatherLoop()
    {
        weatherTimer -= Time.deltaTime;

        if (weatherTimer <= 0f)
        {
            SetNextWeatherState();
        }
    }

    void SetNextWeatherState()
    {
        WeatherSettings settings = GetWeatherSettings(currentSeason);

        // ❄️ WINTER OVERRIDE
        if (currentSeason == Season.Winter)
        {
            WinterSystem.Instance?.StartWinter();
            RainSystem.Instance?.StopRain();

            weatherTimer = Random.Range(
                settings.rainMinTime,
                settings.rainMaxTime
            );

            return;
        }

        // 🌧️ RAIN STATE
        if (weatherState == WeatherState.Dry)
        {
            bool shouldRain = Random.value < settings.rainChance;

            if (shouldRain)
            {
                weatherState = WeatherState.Rain;
                RainSystem.Instance?.StartRain();

                weatherTimer = Random.Range(
                    settings.rainMinTime,
                    settings.rainMaxTime
                );
            }
            else
            {
                weatherState = WeatherState.Dry;
                RainSystem.Instance?.StopRain();

                weatherTimer = Random.Range(
                    settings.dryMinTime,
                    settings.dryMaxTime
                );
            }
        }
        else
        {
            weatherState = WeatherState.Dry;
            RainSystem.Instance?.StopRain();

            weatherTimer = Random.Range(
                settings.dryMinTime,
                settings.dryMaxTime
            );
        }
    }

    WeatherSettings GetWeatherSettings(Season season)
    {
        return season switch
        {
            Season.Spring => springWeather,
            Season.Summer => summerWeather,
            Season.Autumn => autumnWeather,
            Season.Winter => winterWeather,
            _ => springWeather
        };
    }

    // -----------------------------
    // WEATHER APPLICATION (visual preset)
    // -----------------------------
    void ApplyWeather()
    {
        RainSystem.Instance?.StopRain();
        WinterSystem.Instance?.StopWinter();

        switch (currentSeason)
        {
            case Season.Winter:
                WinterSystem.Instance?.StartWinter();
                break;
        }
    }

    // -----------------------------
    // POPUPS
    // -----------------------------
    IEnumerator ShowSeasonPopup()
    {
        GameObject popup = GetPopup(currentSeason);

        if (popup != null)
            popup.SetActive(true);

        yield return new WaitForSeconds(3f);

        if (popup != null)
            popup.SetActive(false);
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