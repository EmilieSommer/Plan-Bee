using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip musicClip;
    public AudioClip clickClip;
    public AudioClip notificationClip;
    public AudioClip honeyCollectClip;
    public AudioClip buildClip;

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoInit()
    {
        if (Instance != null) return;

        GameObject go = new GameObject("AudioManager");
        var audioManager = go.AddComponent<AudioManager>();
        
        // Auto-assign clips via AssetDatabase
        audioManager.musicClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/07_Audio/SFX/background music.mp3");
        audioManager.clickClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/07_Audio/SFX/click.mp3"); 
        audioManager.notificationClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/07_Audio/SFX/notification.wav");
        audioManager.honeyCollectClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/07_Audio/SFX/honey_pickup.wav");
        audioManager.buildClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/07_Audio/SFX/build_sound.aiff");

        DontDestroyOnLoad(go);
    }
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (transform.parent == null) DontDestroyOnLoad(gameObject);

        // Setup Audio Sources
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = 0.5f; // Serene volume

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = 0.8f;
    }

    private void Start()
    {
        if (musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.Play();
        }

        // Auto-bind all UI buttons that exist in the scene currently
        BindAllButtons();
    }

    public void BindAllButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);
        foreach (Button btn in buttons)
        {
            // Remove first to avoid double binding if called multiple times
            btn.onClick.RemoveListener(PlayClick);
            btn.onClick.AddListener(PlayClick);
        }
    }

    public void PlayClick()
    {
        if (clickClip != null)
            sfxSource.PlayOneShot(clickClip, 0.4f);
    }

    public void PlayNotification()
    {
        if (notificationClip != null)
            sfxSource.PlayOneShot(notificationClip, 0.3f); // Not too loud!
    }

    public void PlayHoneyCollect()
    {
        if (honeyCollectClip != null)
            sfxSource.PlayOneShot(honeyCollectClip, 0.5f);
    }

    public void PlayBuild()
    {
        if (buildClip != null)
            sfxSource.PlayOneShot(buildClip, 0.6f);
    }
}
