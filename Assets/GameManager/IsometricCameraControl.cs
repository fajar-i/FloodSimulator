using UnityEngine;
using UnityEngine.InputSystem;

public class IsometricCameraControl : MonoBehaviour
{
    [Header("Pan Settings")]
    public float panSpeed = 1f;
    public float trackpadSensitivity = 10f;
    public float smoothing = 15f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 10f;
    public float minZoom = 2f;
    public float maxZoom = 30f;
    public float zoomSmoothing = 10f;

    private Vector3 targetPosition;
    private float targetZoom;
    private Vector2 lastMousePosition;
    private bool isDragging = false;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        targetPosition = transform.position;
        targetZoom = cam.orthographicSize;
    }

    void Update()
    {
        HandlePan();
        HandleZoom();

        // Smooth Movement
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothing);

        // Smooth Zoom
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSmoothing);
    }

    void HandleZoom()
    {
        var mouse = Mouse.current;
        var keyboard = Keyboard.current;
        if (mouse == null || keyboard == null) return;

        // Figma style: Zoom aktif jika CTRL ditekan + Scroll
        bool isCtrlPressed = keyboard.ctrlKey.isPressed || keyboard.leftAppleKey.isPressed;

        if (isCtrlPressed)
        {
            float scrollY = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scrollY) > 0)
            {
                // Kurangi targetZoom untuk mendekat, tambah untuk menjauh
                targetZoom -= (scrollY * zoomSpeed * 0.01f);
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }
        }
    }

    void HandlePan()
    {
        var mouse = Mouse.current;
        var keyboard = Keyboard.current;
        if (mouse == null || keyboard == null) return;

        bool isCtrlPressed = keyboard.ctrlKey.isPressed || keyboard.leftAppleKey.isPressed;

        // 1. KONTROL KLIK KANAN (Drag)
        if (mouse.rightButton.wasPressedThisFrame)
        {
            isDragging = true;
            lastMousePosition = mouse.position.ReadValue();
        }
        if (mouse.rightButton.wasReleasedThisFrame) isDragging = false;

        if (isDragging)
        {
            Vector2 currentMousePos = mouse.position.ReadValue();
            Vector2 delta = currentMousePos - lastMousePosition;
            // Kita gunakan kecepatan panSpeed biasa
            MoveTarget(delta, panSpeed);
            lastMousePosition = currentMousePos;
        }

        // 2. KONTROL TRACKPAD DIAGONAL
        if (!isCtrlPressed)
        {
            Vector2 scrollDelta = mouse.scroll.ReadValue();
            if (scrollDelta.sqrMagnitude > 0)
            {
                // scrollDelta.x dan scrollDelta.y diproses sekaligus untuk gerak diagonal
                MoveTarget(- scrollDelta, trackpadSensitivity);
            }
        }
    }

    void MoveTarget(Vector2 delta, float speed)
    {
        // Ambil arah kanan dan depan kamera
        Vector3 right = transform.right;
        Vector3 up = transform.up;

        // Kunci sumbu Y agar kamera tetap di ketinggian yang sama (opsional)
        right.y = 0;
        up.y = 0;

        // Kalkulasi pergerakan:
        // delta.x menggerakkan ke samping, delta.y menggerakkan maju/mundur
        Vector3 combinedDirection = (right.normalized * delta.x) + (up.normalized * delta.y);

        // Gunakan -= agar arahnya "Natural Scrolling" seperti Figma (menggeser kanvas)
        targetPosition -= 0.05f * speed * combinedDirection;
    }
}