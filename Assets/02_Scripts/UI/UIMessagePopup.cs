using UnityEngine;
using TMPro;
using System.Collections;

public class UIMessagePopup : MonoBehaviour
{
    public static UIMessagePopup Instance;

    public GameObject panel;
    public TextMeshProUGUI messageText;

    private Coroutine hideRoutine;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void ShowMessage(string message, float duration = 2f)
    {
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        panel.SetActive(true);
        messageText.text = message;

        hideRoutine = StartCoroutine(HideAfter(duration));
    }

    private IEnumerator HideAfter(float t)
    {
        yield return new WaitForSeconds(t);
        panel.SetActive(false);
    }
}