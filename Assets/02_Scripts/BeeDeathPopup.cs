using UnityEngine;
using TMPro;
using System.Collections;

public class BeeDeathPopup : MonoBehaviour
{
    public static BeeDeathPopup Instance;

    public GameObject panel;
    public TextMeshProUGUI messageText;

    private Coroutine hideRoutine;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void ShowDeath(string beeType, string beeName, string killedBy, float duration = 3f)
    {
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        panel.SetActive(true);
        messageText.text = $"{beeType} \"{beeName}\" was killed by {killedBy}";

        hideRoutine = StartCoroutine(HideAfter(duration));
    }

    private IEnumerator HideAfter(float t)
    {
        yield return new WaitForSeconds(t);
        panel.SetActive(false);
    }
}