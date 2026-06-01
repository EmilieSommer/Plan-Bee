using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class QueueSlotUI : MonoBehaviour, IDropHandler
{
    public bool isFilled = false;

    private DraggableEggUI currentEgg;
    private Egg currentWorldEgg;

    public TextMeshProUGUI timerText;

    public void OnDrop(PointerEventData eventData)
    {
        if (EggNamePopup.IsOpen) return;
        if (isFilled) return;

        DraggableEggUI egg = eventData.pointerDrag?.GetComponent<DraggableEggUI>();
        if (egg == null) return;

        if (NurseBeeZone.currentZone == null)
        {
            Debug.LogWarning("No Nurse Zone selected!");
            UIMessagePopup.Instance.ShowMessage("Select a Nurse Zone first!");
            return;
        }

        int cost = egg.honeyCost;

        if (!CurrencyManager.Instance.UseHoney(cost))
        {
            Debug.Log("Not enough honey!");
            egg.ResetToStartPosition();

            UIMessagePopup.Instance.ShowMessage("Not enough honey!");
            return;
        }

        // 🧠 Hive capacity check
        if (!HiveManager.Instance.CanSpawnBee())
        {
            Debug.Log("Hive is full (SleepZones limit reached)");

            CurrencyManager.Instance.AddHoney(cost); // refund
            egg.ResetToStartPosition();

            UIMessagePopup.Instance.ShowMessage("Hive is full! Build more sleep zones.");
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