using TMPro;
using UnityEngine;

public class EggTimerUI : MonoBehaviour
{
    public Egg egg;
    public TextMeshProUGUI text;

    void Update()
    {
        if (egg == null)
        {
            Destroy(gameObject);
            return;
        }

        float timeLeft = egg.GetTimeRemaining();

        if (timeLeft <= 0)
        {
            text.text = "Hatching...";
        }
        else
        {
            text.text = timeLeft.ToString("F1") + "s";
        }
    }
}