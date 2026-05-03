using UnityEngine;

[System.Serializable]
public class WeatherSettings
{
    [Range(0f, 1f)]
    public float rainChance = 0.3f;

    [Header("Rain Duration")]
    public float rainMinTime = 5f;
    public float rainMaxTime = 12f;

    [Header("Dry Duration")]
    public float dryMinTime = 8f;
    public float dryMaxTime = 20f;
}