using UnityEngine;
using System.Collections;

public class RainSystem : MonoBehaviour
{
    public static RainSystem Instance;

    [Header("Prefab")]
    public GameObject rainPrefab;

    private GameObject activeRain;
    private ParticleSystem ps;
    private ParticleSystem.EmissionModule emission;

    [Header("Settings")]
    public float maxEmission = 1200f;
    public float fadeDuration = 2f;

    private Coroutine fadeRoutine;
    public bool IsRaining { get; private set; } = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        activeRain = Instantiate(rainPrefab, new Vector3(0, 22, 0), Quaternion.identity);

        ps = activeRain.GetComponentInChildren<ParticleSystem>();
        emission = ps.emission;

        ps.Play();
        emission.rateOverTime = 0f;
    }

    public void StartRain()
    {
        IsRaining = true;
        FadeTo(maxEmission);
        BeeDeathPopup.Instance.ShowMessage("Rainy weather — Forager bees move slower!", 4f);
    }

    public void StopRain()
    {
        if (!IsRaining) return;

        IsRaining = false;
        FadeTo(0f);
        StartCoroutine(ShowStopMessageAfterFade());
    }

    IEnumerator ShowStopMessageAfterFade()
    {
        yield return new WaitForSeconds(fadeDuration);
        BeeDeathPopup.Instance.ShowMessage("Rain has stopped — Forager bees back to normal speed!", 4f);
    }

    void FadeTo(float target)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeEmission(target));
    }

    IEnumerator FadeEmission(float target)
    {
        float start = emission.rateOverTime.constant;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float lerp = t / fadeDuration;
            emission.rateOverTime = Mathf.Lerp(start, target, lerp);
            yield return null;
        }

        emission.rateOverTime = target;
    }

    public float GetCurrentEmission()
    {
        return emission.rateOverTime.constant;
    }
}