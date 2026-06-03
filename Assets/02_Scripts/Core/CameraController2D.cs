using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController2D : MonoBehaviour
{
    [Header("Zoom")]
    public float zoomSpeed = 3f;
    public float minZoom = 2f;
    public float maxZoom = 10f;
    public float zoomSmoothTime = 0.15f;

    [Header("Pan")]
    public float panSpeed = 0.01f;

    [Header("Bounds")]
    public float minX = -10f;
    public float maxX =  10f;
    public float minY = -10f;
    public float maxY =  10f;

    private Camera cam;

    private float targetZoom;
    private float zoomVelocity;

    private bool isPanning = false;
    private Vector3 panStartMouse;
    private Vector3 panStartPos;

    void Start()
    {
        cam = Camera.main;
        targetZoom = cam.orthographicSize;
    }

    void Update()
    {
        HandleZoom();
        HandlePan();
    }

    // ======================================================
    // ZOOM
    // ======================================================
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0f)
        {
            targetZoom -= scroll * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            ApplyZoomAtCursor();
        }

        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize,
            targetZoom,
            ref zoomVelocity,
            zoomSmoothTime
        );

        ClampPosition();
    }

    void ApplyZoomAtCursor()
    {
        Vector3 mouse = Input.mousePosition;
        mouse.z = Mathf.Abs(cam.transform.position.z);

        Vector3 before = cam.ScreenToWorldPoint(mouse);
        cam.orthographicSize = targetZoom;
        Vector3 after = cam.ScreenToWorldPoint(mouse);

        transform.position += before - after;
    }

    // ======================================================
    // PAN
    // ======================================================
    void HandlePan()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 world = cam.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(world, Vector2.zero);

            if (hit.collider == null)
            {
                isPanning = true;
                panStartMouse = Input.mousePosition;
                panStartPos = transform.position;
            }
        }

        if (Input.GetMouseButtonUp(0))
            isPanning = false;

        if (!isPanning)
            return;

        Vector3 delta = Input.mousePosition - panStartMouse;
        transform.position = panStartPos + new Vector3(-delta.x, -delta.y, 0f) * panSpeed;

        ClampPosition();
    }

    // ======================================================
    // CLAMP
    // ======================================================
    void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }
}