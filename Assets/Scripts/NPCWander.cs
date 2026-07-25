using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCWander : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Wander Settings")]
    [Tooltip("How far from the NPC's starting position it may roam.")]
    public float wanderRadius = 8f;

    [Tooltip("Minimum seconds the NPC stands still before picking a new destination.")]
    public float minWaitTime = 1.5f;

    [Tooltip("Maximum seconds the NPC stands still before picking a new destination.")]
    public float maxWaitTime = 4f;

    [Tooltip("Walking speed while wandering.")]
    public float moveSpeed = 2f;

    [Tooltip("How quickly the NPC turns toward its next waypoint.")]
    public float angularSpeed = 180f;

    [Tooltip("How quickly the NPC accelerates.")]
    public float acceleration = 8f;

    [Header("Animation (optional – 3D only)")]
    [Tooltip("3D Animator on the NPC model. Leave blank for 2D sprite NPCs (use NPCSpriteAnimator instead).")]
    public Animator animator;

    [Tooltip("Name of the float parameter that controls walk blend (0 = idle, 1 = walk). Only used when Animator is assigned.")]
    public string speedParamName = "Speed";

    // ── Private ───────────────────────────────────────────────────────────────

    private NavMeshAgent _agent;
    private Vector3 _origin;        // home position – wander stays within radius of this
    private bool _isWaiting;
    private bool _dialogueWasOpen;  // tracks dialogue state to avoid repeated calls

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        _origin = transform.position;

        // Configure the agent
        _agent.speed           = moveSpeed;
        _agent.angularSpeed    = angularSpeed;
        _agent.acceleration    = acceleration;
        _agent.stoppingDistance = 0.5f;

        // Keep NavMesh rotation OFF so we can drive root rotation manually.
        // This lets NPCSight read transform.forward correctly (it follows movement)
        // while NPCBillboard on the sprite child still overrides that child's
        // world rotation to face the camera independently.
        _agent.updateRotation = false;
        _agent.updateUpAxis   = false;

        // Try to auto-find the Animator if one wasn't assigned (3D NPCs only)
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        StartCoroutine(WanderRoutine());
    }

    void Update()
    {
        HandleDialoguePause();
        UpdateAnimation();
        RotateRootTowardsVelocity();
    }

    private void RotateRootTowardsVelocity()
    {
        if (!_agent.enabled) return;

        Vector3 vel = _agent.velocity;
        vel.y = 0f;

        if (vel.sqrMagnitude < 0.01f) return;   // standing still – keep last facing

        Quaternion targetRot = Quaternion.LookRotation(vel.normalized);
        // angularSpeed is in deg/s; convert to a smooth step fraction
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRot, angularSpeed * Time.deltaTime);
    }

    // ── Wander logic ──────────────────────────────────────────────────────────

    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            // Don't pick a new destination while dialogue is open or agent is disabled
            yield return new WaitUntil(() => _agent.enabled && !IsDialogueOpen());

            // Pick a random reachable point
            Vector3 destination;
            if (TryGetRandomNavMeshPoint(_origin, wanderRadius, out destination))
            {
                _agent.SetDestination(destination);
                _isWaiting = false;

                // Wait until the NPC reaches the destination (or dialogue interrupts)
                yield return new WaitUntil(() =>
                    !_agent.pathPending &&
                    (_agent.remainingDistance <= _agent.stoppingDistance ||
                     IsDialogueOpen() ||
                     !_agent.enabled));
            }

            // Idle pause before the next destination
            _isWaiting = true;
            float waitDuration = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitDuration);
            _isWaiting = false;
        }
    }

    private bool TryGetRandomNavMeshPoint(Vector3 origin, float radius, out Vector3 result)
    {
        const int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPoint = origin + Random.insideUnitSphere * radius;
            randomPoint.y = origin.y; // keep sampling at the same height level

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, radius, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = origin;
        return false;
    }

    // ── Dialogue pause ────────────────────────────────────────────────────────

    private void HandleDialoguePause()
    {
        bool dialogueOpen = IsDialogueOpen();

        if (dialogueOpen && !_dialogueWasOpen)
        {
            // Dialogue just opened – stop the NPC
            _agent.isStopped = true;
            _agent.velocity   = Vector3.zero;
            _dialogueWasOpen  = true;
        }
        else if (!dialogueOpen && _dialogueWasOpen)
        {
            // Dialogue just closed – let the agent move again
            _agent.isStopped = false;
            _dialogueWasOpen = false;
        }
    }

    private bool IsDialogueOpen()
    {
        return DialogueManager.Instance != null && DialogueManager.Instance.IsOpen;
    }

    // ── Animation ─────────────────────────────────────────────────────────────

    private void UpdateAnimation()
    {
        if (animator == null) return;

        // Map current agent speed (0 → stopped, 1 → moving) to the animator parameter
        float normalizedSpeed = _agent.enabled
            ? _agent.velocity.magnitude / Mathf.Max(moveSpeed, 0.001f)
            : 0f;

        animator.SetFloat(speedParamName, normalizedSpeed, 0.1f, Time.deltaTime);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Show the wander radius in the Scene view
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.25f);
        Gizmos.DrawSphere(Application.isPlaying ? _origin : transform.position, wanderRadius);
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.8f);
        UnityEditor.Handles.color = new Color(0f, 1f, 0.5f, 0.8f);
        UnityEditor.Handles.DrawWireDisc(Application.isPlaying ? _origin : transform.position,
                                          Vector3.up, wanderRadius);
    }
#endif
}
