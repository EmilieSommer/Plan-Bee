using UnityEngine;

/// <summary>
/// Top-down camera. Supports zoom via scroll wheel and pan via middle-mouse drag.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 3f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 15f;

    [Header("Pan")]
    [SerializeField] private float panSpeed = 10f;

    private Camera cam;
    private Vector3 dragOrigin;
    private bool isDragging;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        HandleZoom();
        HandlePan();
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f) return;

        cam.orthographicSize = Mathf.Clamp(
            cam.orthographicSize - scroll * zoomSpeed,
            minZoom,
            maxZoom
        );
    }

    private void HandlePan()
    {
        // Middle mouse button pan
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(2))
            isDragging = false;

        if (!isDragging) return;

        Vector3 delta = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
        transform.position += delta;
    }
}
