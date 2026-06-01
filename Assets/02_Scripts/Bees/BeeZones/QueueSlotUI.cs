using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class QueueSlotUI : MonoBehaviour, IDropHandler
{
    public bool isFilled = false;

    private DraggableEggUI currentEgg;
    private Egg currentWorldEgg;

    public TextMeshProUGUI timerText;

    private NurseBeeZone zone;

    public void SetZone(NurseBeeZone newZone)
    {
        zone = newZone;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (EggNamePopup.IsOpen) return;
        if (isFilled) return;

        DraggableEggUI egg = eventData.pointerDrag?.GetComponent<DraggableEggUI>();
        if (egg == null) return;

        if (zone == null)
        {
            Debug.LogError("Slot has no zone assigned!");
            return;
        }

        int cost = egg.honeyCost;

        if (!CurrencyManager.Instance.UseHoney(cost))
        {
            egg.ResetToStartPosition();
            UIMessagePopup.Instance.ShowMessage("Not enough honey!");
            return;
        }

        if (!HiveManager.Instance.CanSpawnBee())
        {
            CurrencyManager.Instance.AddHoney(cost);
            egg.ResetToStartPosition();
            UIMessagePopup.Instance.ShowMessage("Hive is full!");
            return;
        }

        egg.SnapToSlot(transform);

        currentEgg = egg;
        isFilled = true;

        Egg worldEgg = EggSpawner.Instance.SpawnEgg(egg.eggType, zone);
        currentWorldEgg = worldEgg;

        // 🔥 SAFE POPUP CALL (NO GLOBAL STATE DEPENDENCY)
        if (currentWorldEgg != null)
        {
            currentWorldEgg.OnHatched += ClearSlot;

            if (EggNamePopup.Instance != null)
            {
                EggNamePopup.Instance.Open(currentWorldEgg);
            }
            else
            {
                Debug.LogError("EggNamePopup instance missing!");
            }
        }
        else
        {
            Debug.LogError("SpawnEgg returned NULL!");
            ClearSlot();
        }
    }

    private void Update()
    {
        if (isFilled && currentWorldEgg != null)
        {
            float timeLeft = currentWorldEgg.GetTimeRemaining();

            timerText.text = timeLeft <= 0
                ? "Hatching..."
                : timeLeft.ToString("F1") + "s";
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