using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class QueueSlotUI : MonoBehaviour, IDropHandler
{
    public bool isFilled = false;

    private DraggableEggUI currentEgg; // UI reference
    private Egg currentWorldEgg;       // world egg reference
    
    public TextMeshProUGUI timerText;
    public void OnDrop(PointerEventData eventData)
    {
        if (EggNamePopup.IsOpen) return; // 🚫 block during naming

        if (isFilled) return;

        DraggableEggUI egg = eventData.pointerDrag?.GetComponent<DraggableEggUI>();
        if (egg == null) return;

        if (NurseBeeZone.currentZone == null)
        {
            Debug.LogWarning("No Nurse Zone selected!");
            return;
        }

        int cost = egg.honeyCost;

        // ❌ Check BEFORE anything happens
        if (!CurrencyManager.Instance.UseHoney(cost))
        {
            Debug.Log("Not enough honey!");

            egg.ResetToStartPosition();
            return;
        }

        // ✅ accept egg
        egg.SnapToSlot(transform);

        currentEgg = egg;
        isFilled = true;

        Egg worldEgg = EggSpawner.Instance.SpawnEgg(
            egg.eggType,
            NurseBeeZone.currentZone
        );

        currentWorldEgg = worldEgg;

        if (currentWorldEgg != null)
        {
            EggNamePopup.Instance.Open(currentWorldEgg);
            currentWorldEgg.OnHatched += ClearSlot;
        }
    }

    private void Update()
    {
        if (isFilled && currentWorldEgg != null)
        {
            float timeLeft = currentWorldEgg.GetTimeRemaining();

            if (timeLeft <= 0)
            {
                timerText.text = "Hatching...";
            }
            else
            {
                timerText.text = timeLeft.ToString("F1") + "s";
            }
        }
        else
        {
            timerText.text = "";
        }

        if (isFilled && (currentEgg == null || !currentEgg.gameObject.activeInHierarchy))
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        if (currentWorldEgg != null)
        {
            currentWorldEgg.OnHatched -= ClearSlot;
            currentWorldEgg = null;
        }

        if (currentEgg != null)
        {
            Destroy(currentEgg.gameObject);
        }

        isFilled = false;
        currentEgg = null;
    }
}