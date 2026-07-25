using UnityEngine;

public class NPCSight : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Vision")]
    [Tooltip("Maximum distance the NPC can see.")]
    public float sightRange = 10f;

    [Tooltip("Total angle of the vision cone in degrees (e.g. 90 = 45° either side).")]
    [Range(10f, 360f)]
    public float fieldOfView = 90f;

    [Tooltip("Layer mask for obstacles that block NPC vision (walls, shelves, etc.).")]
    public LayerMask obstacleLayers;

    [Header("Eye Position")]
    [Tooltip("Where the NPC 'looks from'. Leave null to use this transform's position + eyeHeightOffset.")]
    public Transform eyePoint;

    [Tooltip("Height offset above the NPC pivot used when Eye Point is not set.")]
    public float eyeHeightOffset = 1.5f;

    // ── Private ───────────────────────────────────────────────────────────────

    private Transform _playerTransform;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Start()
    {
        // Subscribe to the player pickup event
        PlayerController.OnItemPickedUp += HandlePlayerPickup;

        // Cache player transform
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _playerTransform = player.transform;
        else
            Debug.LogWarning("[NPCSight] No GameObject tagged 'Player' found.", this);
    }

    void OnDestroy()
    {
        // Always unsubscribe to avoid memory leaks
        PlayerController.OnItemPickedUp -= HandlePlayerPickup;
    }

    // ── Vision logic ──────────────────────────────────────────────────────────

    private void HandlePlayerPickup()
    {
        if (_playerTransform == null) return;
        if (SuspicionManager.Instance == null) return;

        if (CanSeePlayer())
        {
            Debug.Log($"[NPCSight] '{gameObject.name}' witnessed a pickup! Suspicion increases.");
            SuspicionManager.Instance.AddSuspicion();
        }
    }

    public bool CanSeePlayer()
    {
        if (_playerTransform == null) return false;

        Vector3 origin   = GetEyePosition();
        Vector3 toPlayer = _playerTransform.position - origin;

        // ── Distance check ────────────────────────────────────────────────────
        if (toPlayer.magnitude > sightRange) return false;

        // ── Angle check on XZ plane only ──────────────────────────────────────
        // Flattening both vectors removes height bias so a player standing
        // directly in front is never rejected by vertical tilt.
        Vector3 flatToPlayer = new Vector3(toPlayer.x, 0f, toPlayer.z);
        Vector3 flatForward  = new Vector3(transform.forward.x, 0f, transform.forward.z);

        if (flatToPlayer.sqrMagnitude < 0.001f) return true;   // player is directly on top of NPC

        float angle = Vector3.Angle(flatForward, flatToPlayer);
        if (angle > fieldOfView * 0.5f) return false;

        // ── Line-of-sight check (raycast) ─────────────────────────────────────
        if (obstacleLayers != 0)
        {
            if (Physics.Raycast(origin, toPlayer.normalized, toPlayer.magnitude, obstacleLayers))
                return false;   // something is blocking the view
        }

        return true;
    }

    private Vector3 GetEyePosition()
    {
        if (eyePoint != null) return eyePoint.position;
        return transform.position + Vector3.up * eyeHeightOffset;
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector3 origin = GetEyePosition();

        // Sight range circle
        UnityEditor.Handles.color = new Color(1f, 0.85f, 0f, 0.15f);
        UnityEditor.Handles.DrawSolidArc(origin, Vector3.up, 
            Quaternion.Euler(0, -fieldOfView * 0.5f, 0) * transform.forward,
            fieldOfView, sightRange);

        // Cone outline
        UnityEditor.Handles.color = new Color(1f, 0.85f, 0f, 0.9f);
        UnityEditor.Handles.DrawWireArc(origin, Vector3.up,
            Quaternion.Euler(0, -fieldOfView * 0.5f, 0) * transform.forward,
            fieldOfView, sightRange);

        // Edge rays
        Gizmos.color = new Color(1f, 0.85f, 0f, 0.9f);
        Gizmos.DrawRay(origin, Quaternion.Euler(0, -fieldOfView * 0.5f, 0) * transform.forward * sightRange);
        Gizmos.DrawRay(origin, Quaternion.Euler(0,  fieldOfView * 0.5f, 0) * transform.forward * sightRange);
    }
#endif
}
