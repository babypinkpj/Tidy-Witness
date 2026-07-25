using UnityEngine;

public class MopController : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("References")]
    [Tooltip("The player's inventory. Assign the same InventorySystem used by PlayerController.")]
    public InventorySystem inventory;

    [Header("Mopping Settings")]
    [Tooltip("Suspicion reduced per second while actively mopping.")]
    public float suspicionReductionRate = 20f;

    [Tooltip("Minimum time (seconds) between each reduction tick while button is held. " +
             "Lower = smoother but more frequent updates.")]
    public float tickInterval = 0.1f;

    [Tooltip("Slowdown multiplier applied to the player's max speed while mopping (0–1).")]
    [Range(0f, 1f)]
    public float moppingSpeedMultiplier = 0.4f;

    [Header("Feedback (optional)")]
    [Tooltip("Particle system played while mopping (e.g. a water-splash effect).")]
    public ParticleSystem mopParticles;

    [Tooltip("AudioSource used for mopping sounds.")]
    public AudioSource mopAudio;

    // ── Private state ─────────────────────────────────────────────────────────

    private bool  _isMopping;
    private float _tickTimer;

    // Cached reference so we can restore the player's speed after mopping.
    private PlayerController _playerController;

    // ── Unity ─────────────────────────────────────────────────────────────────

    void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (!_isMopping) return;

        // Only keep mopping if the player still holds a moppable item
        if (!IsHoldingMoppableItem())
        {
            StopMopping();
            return;
        }

        // Drain suspicion on each tick
        _tickTimer -= Time.deltaTime;
        if (_tickTimer <= 0f)
        {
            _tickTimer = tickInterval;

            float reduction = suspicionReductionRate * tickInterval;
            ReduceSuspicion(reduction);
        }
    }

    // ── Input binding (called from PlayerController.OnUse) ────────────────────

    public void SetMopping(bool active)
    {
        if (active)
        {
            if (!IsHoldingMoppableItem()) return;
            StartMopping();
        }
        else
        {
            StopMopping();
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool IsHoldingMoppableItem()
    {
        if (inventory == null) return false;
        Item held = inventory.slots[inventory.currentSlotIndex];
        return held != null && held.isMoppable;
    }

    private void StartMopping()
    {
        if (_isMopping) return;
        _isMopping = true;
        _tickTimer = 0f;   // fire first tick immediately

        // Slow the player down while mopping
        if (_playerController != null)
            _playerController.maxSpeed *= moppingSpeedMultiplier;

        // Visual / audio feedback
        if (mopParticles != null && !mopParticles.isPlaying)
            mopParticles.Play();

        if (mopAudio != null && !mopAudio.isPlaying)
            mopAudio.Play();

        Debug.Log("[MopController] Started mopping — suspicion will decrease.");
    }

    private void StopMopping()
    {
        if (!_isMopping) return;
        _isMopping = false;

        // Restore player speed
        if (_playerController != null && moppingSpeedMultiplier > 0f)
            _playerController.maxSpeed /= moppingSpeedMultiplier;

        // Stop feedback
        if (mopParticles != null && mopParticles.isPlaying)
            mopParticles.Stop();

        if (mopAudio != null && mopAudio.isPlaying)
            mopAudio.Stop();

        Debug.Log("[MopController] Stopped mopping.");
    }

    private void ReduceSuspicion(float amount)
    {
        if (SuspicionManager.Instance == null) return;

        // SuspicionManager only exposes Add; we subtract by directly clamping
        // through a helper we add, or we use the existing ResetSuspicion trick.
        // Since the API doesn't have a public Reduce, we replicate the logic via
        // reflection-free access: call AddSuspicion with a negative-equivalent.
        // The cleanest approach: add a ReduceSuspicion method to SuspicionManager,
        // OR cast the reduction as: current - amount (clamped to 0).
        // Here we call the new public method added to SuspicionManager.
        SuspicionManager.Instance.ReduceSuspicion(amount);

        Debug.Log($"[MopController] Reduced suspicion by {amount:F1}. " +
                  $"Current: {SuspicionManager.Instance.Current:F1}");
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
    void OnGUI()
    {
        if (!_isMopping) return;

        // Simple on-screen indicator while debugging in the editor
        GUI.color = new Color(0.2f, 0.8f, 1f, 0.85f);
        GUI.Label(new Rect(10, Screen.height - 40, 300, 30),
                  "🧹 Mopping... suspicion decreasing");
    }
#endif
}
