using UnityEngine;
using UnityEngine.EventSystems;

public class ClickManager : MonoBehaviour
{
    public LayerMask honeyLayer;
    public LayerMask zoneLayer;
    public LayerMask beeLayer;

    [Header("Drag Settings")]
    public float dragThreshold = 12f;

    [Header("Camera")]
    public float panSpeed = 0.01f;

    [Header("Bee Drag")]
    public float beeDragSmoothTime = 0.08f;
    public float maxBeeDragDistance = 1.5f;

    private Vector3 mouseStart;
    private bool isDragging;
    private bool dragStarted;

    private bool cameraDrag;
    private Vector3 cameraStartPos;

    private GameObject draggedBee;
    private Vector3 beeStartWorld;
    private Vector3 beeDragVelocity;

    private GameObject clickedBee;

    // Zone deferral
    private BuildZone pendingBuildZone;
    private NurseBeeZone pendingNurseZone;

    private GameObject activeCanvas;
    private bool uiOpen;

    public static ClickManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        HandleUIClose();
        HandleMouseDown();
        HandleMouseDrag();
        HandleMouseUp();
    }

    void HandleUIClose()
    {
        if (!uiOpen) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        BeeInfoUI.Instance.Close();
        CloseUI();
    }

    public void RegisterCanvas(GameObject canvas)
    {
        activeCanvas = canvas;
        uiOpen = true;
    }

   void HandleMouseDown()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        mouseStart = Input.mousePosition;
        dragStarted = false;
        isDragging = false;
        pendingBuildZone = null;
        pendingNurseZone = null;

        Vector2 world = Camera.main.ScreenToWorldPoint(mouseStart);

        // Bee hit
        RaycastHit2D beeHit = Physics2D.Raycast(world, Vector2.zero, Mathf.Infinity, beeLayer);
        if (beeHit.collider != null)
        {
            clickedBee = beeHit.collider.gameObject;
            draggedBee = clickedBee;
            beeStartWorld = draggedBee.transform.position;

            Bee bee = draggedBee.GetComponent<Bee>();
            if (bee != null) bee.StartDragging();

            return;
        }

        // Honey (checked before zones so honey on top of a zone is always collectable)
        RaycastHit2D honeyHit = Physics2D.Raycast(world, Vector2.zero, Mathf.Infinity, honeyLayer);
        if (honeyHit.collider != null)
        {
            Honey honey = honeyHit.collider.GetComponent<Honey>();
            if (honey != null) honey.Collect();
            return;
        }

        // Zone hit — store it, don't open yet
        RaycastHit2D zoneHit = Physics2D.Raycast(world, Vector2.zero, Mathf.Infinity, zoneLayer);
        if (zoneHit.collider != null)
        {
            clickedBee = null;
            pendingBuildZone = zoneHit.collider.GetComponent<BuildZone>();
            pendingNurseZone = zoneHit.collider.GetComponent<NurseBeeZone>();
            cameraDrag = true;
            cameraStartPos = Camera.main.transform.position;
            return;
        }

        // Empty space
        cameraDrag = true;
        cameraStartPos = Camera.main.transform.position;
    }
    void HandleMouseDrag()
    {
        if (!Input.GetMouseButton(0)) return;

        Vector3 delta = Input.mousePosition - mouseStart;

        if (!dragStarted && delta.magnitude > dragThreshold)
        {
            dragStarted = true;

            if (draggedBee != null)
            {
                cameraDrag = false;
                isDragging = true;
            }
        }

        // Bee drag
        if (isDragging && draggedBee != null)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = draggedBee.transform.position.z;

            Vector3 offset = mouseWorld - beeStartWorld;
            if (offset.magnitude > maxBeeDragDistance)
                offset = offset.normalized * maxBeeDragDistance;

            draggedBee.transform.position = Vector3.SmoothDamp(
                draggedBee.transform.position,
                beeStartWorld + offset,
                ref beeDragVelocity,
                beeDragSmoothTime
            );

            return;
        }

        // Camera drag
        if (cameraDrag && dragStarted && draggedBee == null)
        {
            Vector3 move = new Vector3(-delta.x, -delta.y, 0f) * panSpeed;
            Camera.main.transform.position = cameraStartPos + move;
        }
    }

    void HandleMouseUp()
    {
        if (!Input.GetMouseButtonUp(0)) return;

        Vector3 totalMove = Input.mousePosition - mouseStart;
        bool isClick = totalMove.magnitude < dragThreshold;

        // End bee drag
        if (draggedBee != null)
        {
            Bee bee = draggedBee.GetComponent<Bee>();
            if (bee != null) bee.StopDragging();

            if (isClick)
            {
                BeeInfoUI.Instance.Open(draggedBee.GetComponent<Bee>());
                uiOpen = true;
            }
        }

        // Open zone UI only if it was a clean click (not a drag)
        if (isClick)
        {
            if (pendingBuildZone != null)
            {
                pendingBuildZone.Open();
                activeCanvas = pendingBuildZone.buildCanvas;
                uiOpen = true;
            }
        }

        draggedBee = null;
        isDragging = false;
        cameraDrag = false;
        dragStarted = false;
        clickedBee = null;
        pendingBuildZone = null;
        pendingNurseZone = null;
    }

    void CloseUI()
    {
        if (activeCanvas != null)
        {
            activeCanvas.SetActive(false);
            activeCanvas = null;
        }
        uiOpen = false;
    }
}