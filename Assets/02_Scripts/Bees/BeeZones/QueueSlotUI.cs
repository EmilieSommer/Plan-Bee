using UnityEngine;
using UnityEngine.EventSystems;

public class QueueSlotUI : MonoBehaviour, IDropHandler
{
    public bool isFilled = false;

    private DraggableEggUI currentEgg; // ✅ keep reference

    public void OnDrop(PointerEventData eventData)
    {
        if (isFilled) return;

        DraggableEggUI egg = eventData.pointerDrag.GetComponent<DraggableEggUI>();

        if (egg != null)
        {
            egg.SnapToSlot(transform);

            currentEgg = egg; // ✅ store egg
            isFilled = true;

            EggSpawner.Instance.SpawnEgg(egg.eggType);
        }
    }

    private void Update()
    {
        // ✅ check if egg is gone or disabled
        if (isFilled && (currentEgg == null || !currentEgg.gameObject.activeInHierarchy))
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        isFilled = false;
        currentEgg = null;
    }
}