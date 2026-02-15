using Unity.Cinemachine;
using UnityEngine;

public class CamController : MonoBehaviour
{
    public static CamController Instance { get; private set; }

    [Header("Pan Settings")]
    public float panSpeed = 10f;
    public bool invertPan = false;

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 10f;
    public float zoomSmoothing = 5f;

    [Header("Camera Bounds")]
    public bool enableBounds = true;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private CinemachineCamera cinemachineCamera;
    private CinemachinePositionComposer positionComposer;
    private Camera cam;
    private Vector3 targetPosition;
    private float targetZoom;

    // Optional: Visualize bounds in editor
    public bool showBoundsGizmo = true;
    public Color boundsColor = Color.green;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        // Get the CinemachineCamera component
        cinemachineCamera = GetComponent<CinemachineCamera>();
        if (cinemachineCamera == null)
        {
            return;
        }

        // Get the camera component
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;

        // Get or add PositionComposer for zoom control
        positionComposer = GetComponent<CinemachinePositionComposer>();
        if (positionComposer == null)
        {
            // If no PositionComposer exists, we'll control the camera directly
            Debug.Log("No CinemachinePositionComposer found. Using direct camera control for zoom.");
        }

        // Initialize target values
        targetPosition = transform.position;

        // Set target zoom based on what's available
        if (positionComposer != null)
            targetZoom = positionComposer.CameraDistance;
        else
            targetZoom = cam.orthographicSize;

        // Validate bounds
        ValidateBounds();
    }

    void Update()
    {
        if (cinemachineCamera == null) return;

        HandlePan();
        HandleZoom();

        // Apply camera position with bounds
        ApplyPositionWithBounds();

        // Apply zoom with smoothing
        ApplyZoomWithSmoothing();
    }

    public float GetCurrentOrthoSize()
    {
        if (cinemachineCamera != null)
        {
            return cinemachineCamera.Lens.OrthographicSize;
        }
        return 0f;
    }

    void HandlePan()
    {
        // Right mouse button pan
        if (Input.GetMouseButton(1))
        {
            // Get current zoom level for scaling
            float currentZoom = GetCurrentCameraSize();

            // Calculate zoom scale factor (normalized between min and max zoom)
            float zoomRange = maxZoom - minZoom;
            float zoomProgress = (currentZoom - minZoom) / zoomRange;

            // Option 1: Linear scaling - faster when zoomed out, slower when zoomed in
            float zoomScaleFactor = Mathf.Lerp(0.5f, 2f, zoomProgress);

            // Option 2: Inverse scaling - slower when zoomed out, faster when zoomed in
            // float zoomScaleFactor = Mathf.Lerp(2f, 0.5f, zoomProgress);

            // Option 3: Exponential scaling for more dramatic effect
            // float zoomScaleFactor = Mathf.Pow(zoomProgress, 2) * 2f + 0.5f;

            // Apply zoom scaling to pan speed
            float scaledPanSpeed = panSpeed * zoomScaleFactor;

            float panX = Input.GetAxis("Mouse X") * scaledPanSpeed * (invertPan ? 1 : -1);
            float panY = Input.GetAxis("Mouse Y") * scaledPanSpeed * (invertPan ? 1 : -1);

            Vector3 panDirection = new Vector3(panX, panY, 0);
            targetPosition += panDirection;
        }
    }

    void HandleZoom()
    {
        // Scroll wheel zoom
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            targetZoom -= scrollInput * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
    }

    void ApplyPositionWithBounds()
    {
        if (!enableBounds)
        {
            transform.position = targetPosition;
            return;
        }

        // Get current camera size for bounds calculation
        float currentCameraSize = GetCurrentCameraSize();
        float cameraHeight = 2f * currentCameraSize;
        float cameraWidth = cameraHeight * cam.aspect;

        // Calculate clamped position considering camera size
        Vector3 clampedPosition = targetPosition;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x,
            minBounds.x + cameraWidth * 0.5f,
            maxBounds.x - cameraWidth * 0.5f);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y,
            minBounds.y + cameraHeight * 0.5f,
            maxBounds.y - cameraHeight * 0.5f);

        // Apply clamped position
        transform.position = clampedPosition;

        // Update target position to match clamped position
        targetPosition = clampedPosition;
    }

    void ApplyZoomWithSmoothing()
    {
        float currentZoom = GetCurrentCameraSize();
        float newZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomSmoothing);

        // Apply zoom based on what component we're using
        if (positionComposer != null)
        {
            // If using PositionComposer, set its CameraDistance
            positionComposer.CameraDistance = newZoom;
        }
        else
        {
            // Direct camera control
            if (cinemachineCamera.Lens.Orthographic)
            {
                // For orthographic cameras
                var lens = cinemachineCamera.Lens;
                lens.OrthographicSize = newZoom;
                cinemachineCamera.Lens = lens;
            }
            else
            {
                // For perspective cameras (if needed)
                var lens = cinemachineCamera.Lens;
                lens.FieldOfView = newZoom;
                cinemachineCamera.Lens = lens;
            }
        }
    }

    float GetCurrentCameraSize()
    {
        if (positionComposer != null)
        {
            return positionComposer.CameraDistance;
        }
        else if (cinemachineCamera.Lens.Orthographic)
        {
            return cinemachineCamera.Lens.OrthographicSize;
        }
        else
        {
            return cinemachineCamera.Lens.FieldOfView;
        }
    }

    void ValidateBounds()
    {
        if (minBounds.x > maxBounds.x)
        {
            float temp = minBounds.x;
            minBounds.x = maxBounds.x;
            maxBounds.x = temp;
            Debug.LogWarning("Min X was greater than Max X. Bounds have been swapped.");
        }

        if (minBounds.y > maxBounds.y)
        {
            float temp = minBounds.y;
            minBounds.y = maxBounds.y;
            maxBounds.y = temp;
            Debug.LogWarning("Min Y was greater than Max Y. Bounds have been swapped.");
        }
    }

    // Optional: Visualize bounds in the Scene view
    void OnDrawGizmosSelected()
    {
        if (!showBoundsGizmo || !enableBounds) return;

        Gizmos.color = boundsColor;

        // Draw rectangle representing bounds
        Vector3 center = new Vector3(
            (minBounds.x + maxBounds.x) * 0.5f,
            (minBounds.y + maxBounds.y) * 0.5f,
            0
        );

        Vector3 size = new Vector3(
            maxBounds.x - minBounds.x,
            maxBounds.y - minBounds.y,
            0
        );

        Gizmos.DrawWireCube(center, size);
    }
}
