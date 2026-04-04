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
            egg.SnapToSlot(transform);

            currentEgg = egg;
            isFilled = true;

            Egg worldEgg = EggSpawner.Instance.SpawnEgg(egg.eggType);
            currentWorldEgg = worldEgg;

            // ✅ subscribe to hatch event
            if (currentWorldEgg != null)
            {
                currentWorldEgg.OnHatched += ClearSlot;
            }
        }
    }

    private void Update()
    {
        // Optional safety check (kept from your original logic)
        if (isFilled && (currentEgg == null || !currentEgg.gameObject.activeInHierarchy))
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        // unsubscribe from world egg
        if (currentWorldEgg != null)
        {
            currentWorldEgg.OnHatched -= ClearSlot;
            currentWorldEgg = null;
        }

        // ✅ remove the UI egg visually
        if (currentEgg != null)
        {
            Destroy(currentEgg.gameObject); // or SetActive(false)
        }

        isFilled = false;
        currentEgg = null;
    }
}