using UnityEngine;

public class CameraController2D : MonoBehaviour
{
    [Header("Zoom")]
    public float zoomSpeed = 3f;
    public float minZoom = 2f;
    public float maxZoom = 10f;
    public float zoomSmoothTime = 0.2f;

    [Header("Drag")]
    public float dragSpeed = 1f;
    public float dragSmoothTime = 0.08f;

    [Header("Drag Limits")]
    public float maxDragDistance = 10f;

    private Camera cam;

    private float targetZoom;
    private float zoomVelocity;

    private Vector3 dragOrigin;
    private Vector3 moveVelocity;

    private Vector3 startPosition;

    void Start()
    {
        cam = Camera.main;
        targetZoom = cam.orthographicSize;
        startPosition = transform.position;
    }

    void Update()
    {
        HandleZoom();
        HandleDrag();
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            targetZoom -= scroll * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Abs(cam.transform.position.z);

        Vector3 mouseBefore = cam.ScreenToWorldPoint(mouseScreen);

        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize,
            targetZoom,
            ref zoomVelocity,
            zoomSmoothTime
        );

        Vector3 mouseAfter = cam.ScreenToWorldPoint(mouseScreen);

        Vector3 offset = mouseBefore - mouseAfter;

        Vector3 targetPosition = transform.position + offset;

        transform.position = ClampToRange(targetPosition);
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 currentPos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 difference = dragOrigin - currentPos;

            Vector3 targetPosition = transform.position + difference * dragSpeed;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                ClampToRange(targetPosition),
                ref moveVelocity,
                dragSmoothTime
            );
        }
    }

    Vector3 ClampToRange(Vector3 target)
    {
        Vector3 offset = target - startPosition;

        if (offset.magnitude > maxDragDistance)
        {
            offset = offset.normalized * maxDragDistance;
        }

        return startPosition + offset;
    }
}