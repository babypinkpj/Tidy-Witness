using UnityEngine;

public class NPCBillboard : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The SpriteRenderer to flip. Auto-detected if left blank.")]
    public SpriteRenderer spriteRenderer;

    [Header("Billboard Mode")]
    [Tooltip("Y-Axis only: sprite stays upright (recommended for top-down / 3-quarter view).\n" +
             "Full: sprite tilts to fully face the camera (use for pure 2D-in-3D worlds).")]
    public BillboardMode mode = BillboardMode.YAxisOnly;

    [Header("Flip Settings")]
    [Tooltip("Flip the sprite horizontally based on movement direction.")]
    public bool flipOnMove = true;

    [Tooltip("Flip when moving in the positive-X direction (disable if your sprite faces right by default).")]
    public bool defaultFacesRight = true;

    // ── Private ───────────────────────────────────────────────────────────────

    private Transform _cam;
    private Vector3   _lastPosition;

    // ── Enums ─────────────────────────────────────────────────────────────────

    public enum BillboardMode { YAxisOnly, Full }

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
            Debug.LogWarning("[NPCBillboard] No SpriteRenderer found on " + gameObject.name, this);
    }

    void Start()
    {
        if (Camera.main != null) _cam = Camera.main.transform;
        _lastPosition = transform.position;
    }

    void LateUpdate()   // LateUpdate keeps it in sync after physics/NavMesh moves the NPC
    {
        FaceCamera();

        if (flipOnMove)
            HandleFlip();

        _lastPosition = transform.position;
    }

    // ── Billboard ─────────────────────────────────────────────────────────────

    private void FaceCamera()
    {
        if (_cam == null)
        {
            if (Camera.main != null) _cam = Camera.main.transform;
            else return;
        }

        switch (mode)
        {
            case BillboardMode.YAxisOnly:
                // Rotate only around Y so the sprite stays upright
                Vector3 lookDir = _cam.position - transform.position;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(lookDir);
                break;

            case BillboardMode.Full:
                // Fully face the camera (works for isometric / overhead cameras)
                transform.rotation = _cam.rotation;
                break;
        }
    }

    // ── Sprite flip ───────────────────────────────────────────────────────────

    private void HandleFlip()
    {
        if (spriteRenderer == null) return;

        Vector3 movement = transform.position - _lastPosition;

        // Project movement onto the camera's right axis
        Vector3 camRight  = _cam != null ? _cam.right : Vector3.right;
        float   dot       = Vector3.Dot(movement.normalized, camRight);

        if (movement.sqrMagnitude < 0.00001f) return; // not moving – keep last flip

        // Flip when moving in the negative-camera-right direction
        spriteRenderer.flipX = defaultFacesRight ? (dot < 0f) : (dot > 0f);
    }
}
