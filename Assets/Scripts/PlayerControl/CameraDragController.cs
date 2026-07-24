using UnityEngine;
using UnityEngine.EventSystems;

public class CameraDragController : MonoBehaviour
{
    [Header("Setup")]
    public Transform cameraTarget;   // Virtual Camera sẽ Follow object này

    [Header("Drag")]
    public float dragThresholdPixels = 10f;

    // BuildManager đọc biến này để phân biệt click và drag (giữ nguyên như cũ)
    public static bool IsDragging { get; private set; }

    private Camera cam;
    private Vector3 dragOriginWorld;
    private Vector3 mouseDownScreenPos;
    private bool mouseHeld;

    void Awake()
    {
        cam = Camera.main;   // vẫn là Main Camera (có CinemachineBrain)
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            mouseHeld = true;
            IsDragging = false;
            mouseDownScreenPos = Input.mousePosition;
            dragOriginWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (mouseHeld && Input.GetMouseButton(0))
        {
            if (!IsDragging &&
                Vector3.Distance(Input.mousePosition, mouseDownScreenPos) > dragThresholdPixels)
            {
                IsDragging = true;
            }

            if (IsDragging)
            {
                // Di chuyển TARGET, không đụng vào camera
                Vector3 diff = dragOriginWorld - cam.ScreenToWorldPoint(Input.mousePosition);
                diff.z = 0f;
                cameraTarget.position += diff;
                // Không cần clamp — Confiner2D sẽ tự chặn camera trong map
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            mouseHeld = false;
        }
    }

    void LateUpdate()
    {
        if (!Input.GetMouseButton(0))
            IsDragging = false;
    }
}