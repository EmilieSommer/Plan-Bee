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

    private float stateTimer;

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
        ApplyScenery(); // Add this
        InitializeWeather();
    }

    // -----------------------------
    // WEATHER INIT
    // -----------------------------
    void InitializeWeather()
    {
        weatherState = WeatherState.Dry;

        RainSystem.Instance?.StopRain();
        WinterSystem.Instance?.StopWinter();

        SetNextWeatherState();
    }

    // -----------------------------
    // WEATHER LOOP
    // -----------------------------
    void UpdateWeatherLoop()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            SetNextWeatherState();
        }
    }

    // -----------------------------
    // WEATHER STATE MACHINE
    // -----------------------------
    void SetNextWeatherState()
{
    WeatherSettings settings = GetWeatherSettings(currentSeason);

    // ❄️ WINTER OVERRIDE
    if (currentSeason == Season.Winter)
    {
        WinterSystem.Instance?.StartWinter();
        RainSystem.Instance?.StopRain();

        stateTimer = Random.Range(settings.dryMinTime, settings.dryMaxTime);
        return;
    }

    if (weatherState == WeatherState.Dry)
    {
        bool shouldRain = Random.value < settings.rainChance;

        if (shouldRain)
        {
            weatherState = WeatherState.Rain;
            RainSystem.Instance?.StartRain();

            // 🌧️ FIXED: now uses per-season settings
            stateTimer = Random.Range(settings.rainMinTime, settings.rainMaxTime);
        }
        else
        {
            RainSystem.Instance?.StopRain();
            stateTimer = Random.Range(settings.dryMinTime, settings.dryMaxTime);
        }
    }
    else
    {
        weatherState = WeatherState.Dry;
        RainSystem.Instance?.StopRain();

        stateTimer = Random.Range(settings.dryMinTime, settings.dryMaxTime);
    }
}

    // -----------------------------
    // WEATHER SETTINGS
    // -----------------------------
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
    // VISUAL WEATHER APPLY
    // -----------------------------
    void ApplyWeather()
    {
        RainSystem.Instance?.StopRain();
        WinterSystem.Instance?.StopWinter();

        if (currentSeason == Season.Winter)
        {
            WinterSystem.Instance?.StartWinter();
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

    void ApplyScenery()
    {
        StopCoroutine(nameof(FadeScenery));
        StartCoroutine(FadeScenery());
    }

    IEnumerator FadeScenery()
    {
        SeasonProfile[] profiles = { spring, summer, autumn, winter };
        SeasonProfile current = GetCurrentProfile();

        float duration = 2f;
        float timer = 0f;

        // Get all renderers
        SpriteRenderer[] currentRenderers = current.scenery.GetComponentsInChildren<SpriteRenderer>(true);

        // Enable current scenery + make transparent
        current.scenery.SetActive(true);

        foreach (var sr in currentRenderers)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }

        // Fade out all others instantly
        foreach (var profile in profiles)
        {
            if (profile != current && profile.scenery != null)
                profile.scenery.SetActive(false);
        }

        // Fade in
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = timer / duration;

            foreach (var sr in currentRenderers)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }

            yield return null;
        }

        // Ensure fully visible
        foreach (var sr in currentRenderers)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
    }
    
}