using UnityEngine;
using TMPro;

public class EggNamePopup : MonoBehaviour
{
    public static EggNamePopup Instance;

    // 🧠 global lock flag
    public static bool IsOpen { get; private set; }

    public GameObject panel;
    public TMP_InputField nameInput;

    private Egg currentEgg;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
        IsOpen = false;
    }

    public void Open(Egg egg)
    {
        Debug.Log("OPEN CALLED");

        currentEgg = egg;

        panel.SetActive(true);
        nameInput.text = "";
        nameInput.ActivateInputField();

        IsOpen = true;

        Debug.Log("PANEL OPENED: " + panel.name);
    }

    public void ConfirmName()
    {
        Debug.Log("CONFIRM CALLED");

        if (currentEgg == null)
        {
            Debug.LogWarning("No current egg!");
            return;
        }

        string eggName = nameInput.text.Trim();

        // 🚫 block empty input
        if (string.IsNullOrEmpty(eggName))
        {
            Debug.Log("Name cannot be empty!");
            return;
        }

        currentEgg.SetName(eggName);

        Close();

        Debug.Log("CONFIRM COMPLETE");
    }

    private void Close()
    {
        Debug.Log("CLOSING PANEL: " + panel.name);

        panel.SetActive(false);

        Debug.Log("PANEL ACTIVE AFTER CLOSE: " + panel.activeSelf);

        currentEgg = null;
        IsOpen = false;
    }

    private void OnDisable()
    {
        Debug.Log("EggNamePopup DISABLED");
        IsOpen = false;
    }
}   