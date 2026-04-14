using UnityEngine;
using UnityEngine.EventSystems;

public class QueueSlotUI : MonoBehaviour, IDropHandler
{
    public bool isFilled = false;

    private DraggableEggUI currentEgg; // UI reference
    private Egg currentWorldEgg;       // world egg reference

    public void OnDrop(PointerEventData eventData)
    {
        if (isFilled) return;

        DraggableEggUI egg = eventData.pointerDrag.GetComponent<DraggableEggUI>();

        if (egg != null)
        {
            // ✅ Make sure a zone is selected
            if (NurseBeeZone.currentZone == null)
            {
                Debug.LogWarning("No Nurse Zone selected!");
                return;
            }

            egg.SnapToSlot(transform);

            currentEgg = egg;
            isFilled = true;

            // ✅ FIX: pass the selected zone
            Egg worldEgg = EggSpawner.Instance.SpawnEgg(
                egg.eggType,
                NurseBeeZone.currentZone
            );

            currentWorldEgg = worldEgg;

            // subscribe to hatch event
            if (currentWorldEgg != null)
            {
                currentWorldEgg.OnHatched += ClearSlot;
            }
        }
    }

    private void Update()
    {
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