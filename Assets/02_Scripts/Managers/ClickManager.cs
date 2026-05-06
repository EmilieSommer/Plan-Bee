using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ClickManager : MonoBehaviour
{
    public LayerMask honeyLayer;
    public LayerMask zoneLayer;
    public LayerMask beeLayer;

    [Header("Bee Drag")]
    public float maxDragDistance = 1.5f;
    public float maxDragTime = 1.2f;

    private GameObject activeCanvas;
    private bool uiOpen = false;

    private GameObject draggedBee;
    private bool isDraggingBee = false;
    private Vector3 dragStartPosition;
    private float dragTimer;

    // 🐝 Click detection
    private GameObject clickedBee;
    private bool beeClickPending = false;
    private float clickTimer;
    private Vector3 clickStartPos;

    void Update()
    {
        HandleBeeDrag();
        HandleBeeClick();

        // 🧠 NEW: Close bee UI when clicking anywhere (except UI objects)
        if (Input.GetMouseButtonDown(0))
        {
            if (BeeInfoUI.Instance != null && BeeInfoUI.Instance.panel.activeSelf)
            {
                BeeInfoUI.Instance.Hide();
                uiOpen = false;
            }
        }

        if (!Input.GetMouseButtonDown(0))
            return;

        // UI has priority
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 🧠 Close UI if open (your original system kept intact)
        if (uiOpen)
        {
            BeeInfoUI.Instance.Hide();
            CloseUI();
            return;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 🐝 Bee detection (drag + click prep)
        RaycastHit2D beeHit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, beeLayer);

        if (beeHit.collider != null)
        {
            clickedBee = beeHit.collider.gameObject;
            beeClickPending = true;
            clickTimer = 0f;
            clickStartPos = Input.mousePosition;

            // Start drag system (unchanged behaviour)
            draggedBee = clickedBee;
            isDraggingBee = true;
            dragStartPosition = draggedBee.transform.position;
            dragTimer = 0f;

            Bee bee = draggedBee.GetComponent<Bee>();
            if (bee != null)
                bee.StartDragging();

            return;
        }

        // 🍯 Honey
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

        // 🧱 Zones
        RaycastHit2D zoneHit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, zoneLayer);

        if (zoneHit.collider != null)
        {
            BuildZone buildZone = zoneHit.collider.GetComponent<BuildZone>();

            if (buildZone != null)
            {
                buildZone.Open();
                activeCanvas = buildZone.buildCanvas;
                uiOpen = true;
                return;
            }

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

    void HandleBeeDrag()
    {
        if (!isDraggingBee || draggedBee == null)
            return;

        dragTimer += Time.deltaTime;

        if (dragTimer >= maxDragTime)
        {
            ReleaseBee();
            return;
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = draggedBee.transform.position.z;

        Vector3 offset = mousePos - dragStartPosition;

        if (offset.magnitude > maxDragDistance)
            offset = offset.normalized * maxDragDistance;

        Vector3 targetPos = dragStartPosition + offset;

        Rigidbody2D rb = draggedBee.GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.MovePosition(targetPos);
        else
            draggedBee.transform.position = targetPos;

        if (Input.GetMouseButtonUp(0))
        {
            ReleaseBee();
        }
    }

    void HandleBeeClick()
    {
        if (!beeClickPending || clickedBee == null)
            return;

        clickTimer += Time.deltaTime;

        if (Input.GetMouseButtonUp(0))
        {
            float moveDist = Vector3.Distance(clickStartPos, Input.mousePosition);

            bool isClick = moveDist < 10f && clickTimer < 0.25f;

            if (isClick)
            {
                Bee bee = clickedBee.GetComponent<Bee>();

                if (bee != null)
                {
                    BeeInfoUI.Instance.Show(bee);
                    uiOpen = true;
                }

                ReleaseBee();
                isDraggingBee = false;
            }

            beeClickPending = false;
            clickedBee = null;
        }
    }

    void ReleaseBee()
    {
        if (draggedBee != null)
        {
            Bee bee = draggedBee.GetComponent<Bee>();

            if (bee != null)
                bee.StopDragging();
        }

        isDraggingBee = false;
        draggedBee = null;
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