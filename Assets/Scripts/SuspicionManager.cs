using UnityEngine;

public class SuspicionManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static SuspicionManager Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Suspicion Settings")]
    [Tooltip("Maximum suspicion value before the player is 'caught'.")]
    public float maxSuspicion = 100f;

    [Tooltip("Suspicion added each time an NPC sees the player pick up an item.")]
    public float suspicionPerPickup = 25f;

    [Tooltip("Seconds of delay before suspicion starts decaying after the last increase.")]
    public float decayDelay = 5f;

    [Tooltip("Suspicion lost per second during decay.")]
    public float decayRate = 5f;

    // ── Events ────────────────────────────────────────────────────────────────
    public System.Action<float, float> OnSuspicionChanged;

    public System.Action OnMaxSuspicion;

    // ── State ─────────────────────────────────────────────────────────────────
    private float _current;
    private float _decayTimer;   // counts down from decayDelay before decay starts
    private bool  _maxFired;     // prevents OnMaxSuspicion firing multiple times

    public float Current => _current;
    public float Max     => maxSuspicion;

    // ── Unity ─────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (_current <= 0f) return;

        // Count down the delay window before decay starts
        if (_decayTimer > 0f)
        {
            _decayTimer -= Time.deltaTime;
            return;
        }

        // Decay
        _current = Mathf.Max(0f, _current - decayRate * Time.deltaTime);
        OnSuspicionChanged?.Invoke(_current, maxSuspicion);

        if (_current <= 0f)
            _maxFired = false;  // reset so max can fire again next cycle
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void AddSuspicion()
    {
        AddSuspicion(suspicionPerPickup);
    }

    public void AddSuspicion(float amount)
    {
        if (amount <= 0f) return;

        _current    = Mathf.Min(_current + amount, maxSuspicion);
        _decayTimer = decayDelay;   // reset decay countdown

        OnSuspicionChanged?.Invoke(_current, maxSuspicion);

        if (!_maxFired && _current >= maxSuspicion)
        {
            _maxFired = true;
            OnMaxSuspicion?.Invoke();
            Debug.Log("[SuspicionManager] Max suspicion reached! Player caught.");
        }
    }

    public void ResetSuspicion()
    {
        _current    = 0f;
        _decayTimer = 0f;
        _maxFired   = false;
        OnSuspicionChanged?.Invoke(_current, maxSuspicion);
    }

    public void ReduceSuspicion(float amount)
    {
        if (amount <= 0f) return;

        _current = Mathf.Max(0f, _current - amount);

        // If suspicion is fully cleared, allow max-suspicion event to fire again next cycle
        if (_current <= 0f)
            _maxFired = false;

        OnSuspicionChanged?.Invoke(_current, maxSuspicion);
    }
}
