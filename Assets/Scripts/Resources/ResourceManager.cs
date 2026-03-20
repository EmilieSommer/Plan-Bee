using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Starting Amounts")]
    [SerializeField] private float startingHoney = 100f;
    [SerializeField] private float startingPollen = 50f;
    [SerializeField] private float startingBeeswax = 30f;

    [Header("Caps")]
    [SerializeField] private float maxHoney = 500f;
    [SerializeField] private float maxPollen = 300f;
    [SerializeField] private float maxBeeswax = 200f;

    public float Honey { get; private set; }
    public float Pollen { get; private set; }
    public float Beeswax { get; private set; }

    public float MaxHoney => maxHoney;
    public float MaxPollen => maxPollen;
    public float MaxBeeswax => maxBeeswax;

    // UI subscribes to these to update displays
    public event System.Action<float, float> OnHoneyChanged;
    public event System.Action<float, float> OnPollenChanged;
    public event System.Action<float, float> OnBeeswaxChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Honey = startingHoney;
        Pollen = startingPollen;
        Beeswax = startingBeeswax;
    }

    // --- Honey ---

    public bool SpendHoney(float amount)
    {
        if (Honey < amount) return false;
        Honey = Mathf.Max(0f, Honey - amount);
        OnHoneyChanged?.Invoke(Honey, maxHoney);
        return true;
    }

    public void AddHoney(float amount)
    {
        Honey = Mathf.Min(maxHoney, Honey + amount);
        OnHoneyChanged?.Invoke(Honey, maxHoney);
    }

    // --- Pollen ---

    public bool SpendPollen(float amount)
    {
        if (Pollen < amount) return false;
        Pollen = Mathf.Max(0f, Pollen - amount);
        OnPollenChanged?.Invoke(Pollen, maxPollen);
        return true;
    }

    public void AddPollen(float amount)
    {
        Pollen = Mathf.Min(maxPollen, Pollen + amount);
        OnPollenChanged?.Invoke(Pollen, maxPollen);
    }

    // --- Beeswax ---

    public bool SpendBeeswax(float amount)
    {
        if (Beeswax < amount) return false;
        Beeswax = Mathf.Max(0f, Beeswax - amount);
        OnBeeswaxChanged?.Invoke(Beeswax, maxBeeswax);
        return true;
    }

    public void AddBeeswax(float amount)
    {
        Beeswax = Mathf.Min(maxBeeswax, Beeswax + amount);
        OnBeeswaxChanged?.Invoke(Beeswax, maxBeeswax);
    }

    public void SetMaxHoney(float newMax) { maxHoney = newMax; OnHoneyChanged?.Invoke(Honey, maxHoney); }
    public void SetMaxPollen(float newMax) { maxPollen = newMax; OnPollenChanged?.Invoke(Pollen, maxPollen); }
    public void SetMaxBeeswax(float newMax) { maxBeeswax = newMax; OnBeeswaxChanged?.Invoke(Beeswax, maxBeeswax); }
}
