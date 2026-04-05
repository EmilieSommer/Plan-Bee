using UnityEngine;
using UnityEngine.EventSystems;

public class ClickManager : MonoBehaviour
{
    public LayerMask honeyLayer;
    public LayerMask zoneLayer;

    private GameObject activeCanvas;
    private bool uiOpen = false;

    void Update()
    {
        // 🖱️ Click / touch (mouse works for both in most cases)
        if (!Input.GetMouseButtonDown(0))
            return;

        // 🚫 If UI is open → ONLY allow closing it
        if (uiOpen)
        {
            // If clicking UI → ignore
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            // Click outside UI → close it
            if (activeCanvas != null)
            {
                activeCanvas.SetActive(false);
                activeCanvas = null;
            }

            uiOpen = false;
            return;
        }

        // 🚫 Ignore clicks on UI (buttons etc.)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 🥇 HONEY
        RaycastHit2D honeyHit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, honeyLayer);
        if (honeyHit.collider != null)
        {
            Honey honey = honeyHit.collider.GetComponent<Honey>();
            if (honey != null)
            {
                honey.Collect();
                return;
            }
        }

        // 🧱 ZONES
        RaycastHit2D zoneHit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, zoneLayer);
        if (zoneHit.collider != null)
        {
            // Build Zone
            BuildZone buildZone = zoneHit.collider.GetComponent<BuildZone>();
            if (buildZone != null)
            {
                buildZone.Open();
                activeCanvas = buildZone.buildCanvas;
                uiOpen = true;
                return;
            }

            // Nurse Zone
            NurseBeeZone nurseZone = zoneHit.collider.GetComponent<NurseBeeZone>();
            if (nurseZone != null)
            {
                nurseZone.Open();
                activeCanvas = nurseZone.nurseCanvas;
                uiOpen = true;
                return;
            }
        }
    }
}