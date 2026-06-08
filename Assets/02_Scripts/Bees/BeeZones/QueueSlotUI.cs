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
            UIMessagePopup.Instance.ShowMessage("Select a Nurse Zone first!");
            return;
        }

        // 🧠 PREREQUISITE CHECK: Only require a zone for Drones
        Bee.BeeType requiredType = EggToBeeType(egg.eggType);
        if (requiredType == Bee.BeeType.Drone && ZoneManager.Instance != null && !ZoneManager.Instance.HasZone(requiredType))
        {
            egg.ResetToStartPosition();
            UIMessagePopup.Instance.ShowMessage("You must build a Drone Post first!");
            return;
        }

        int cost = egg.honeyCost;

        if (!CurrencyManager.Instance.UseHoney(cost))
        {
            egg.ResetToStartPosition();
            UIMessagePopup.Instance.ShowMessage("Not enough honey!");
            return;
        }

        // ❗ IMPORTANT: capacity check includes queued eggs now
        if (!HiveManager.Instance.CanSpawnBee())
        {
            CurrencyManager.Instance.AddHoney(cost);
            egg.ResetToStartPosition();
            UIMessagePopup.Instance.ShowMessage("Hive is full!");
            return;
        }

        // 🧠 RESERVE CAPACITY IMMEDIATELY
        HiveManager.Instance.RegisterQueuedEgg();

        egg.SnapToSlot(transform);

        currentEgg = egg;
        isFilled = true;

        Egg worldEgg = EggSpawner.Instance.SpawnEgg(egg.eggType, zone);
        currentWorldEgg = worldEgg;

        if (currentWorldEgg != null)
        {
            currentWorldEgg.OnHatched += ClearSlot;
            EggNamePopup.Instance.Open(currentWorldEgg);
        }
        else
        {
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

            // 🧠 release reserved slot
            HiveManager.Instance.UnregisterQueuedEgg();

            currentWorldEgg = null;
        }

        if (currentEgg != null)
        {
            Destroy(currentEgg.gameObject);
        }

        isFilled = false;
        currentEgg = null;
    }

    private Bee.BeeType EggToBeeType(EggType eggType)
    {
        switch (eggType)
        {
            case EggType.Builder: return Bee.BeeType.Builder;
            case EggType.Nurse: return Bee.BeeType.Nurse;
            case EggType.House: return Bee.BeeType.House;
            case EggType.Forager: return Bee.BeeType.Forager;
            case EggType.Drone: return Bee.BeeType.Drone;
            default: return Bee.BeeType.House;
        }
    }
}