using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(SpriteRenderer))]
public class NPCSpriteAnimator : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Sprite Frames")]
    [Tooltip("Sprites played when the NPC is standing still.")]
    public Sprite[] idleFrames;

    [Tooltip("Sprites played when the NPC is walking.")]
    public Sprite[] walkFrames;

    [Header("Playback")]
    [Tooltip("Frames per second for the idle animation.")]
    public float idleFPS = 6f;

    [Tooltip("Frames per second for the walk animation.")]
    public float walkFPS = 10f;

    [Tooltip("Agent velocity threshold to switch from idle to walk.")]
    public float moveThreshold = 0.1f;

    // ── Private ───────────────────────────────────────────────────────────────

    private SpriteRenderer _sr;
    private NavMeshAgent   _agent;
    private Sprite[]       _currentSet;
    private int            _frameIndex;
    private float          _timer;
    private float          _currentFPS;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        _sr    = GetComponent<SpriteRenderer>();
        _agent = GetComponentInParent<NavMeshAgent>();   // agent may be on the parent root

        if (_agent == null)
            _agent = GetComponent<NavMeshAgent>();

        if (_agent == null)
            Debug.LogWarning("[NPCSpriteAnimator] No NavMeshAgent found. Animation won't switch states.", this);
    }

    void Start()
    {
        SwitchAnimation(idleFrames, idleFPS);
    }

    void Update()
    {
        ChooseAnimation();
        AdvanceFrame();
    }

    // ── Animation state ───────────────────────────────────────────────────────

    private void ChooseAnimation()
    {
        bool isMoving = _agent != null && _agent.enabled &&
                        _agent.velocity.magnitude > moveThreshold;

        Sprite[] desired = isMoving ? walkFrames : idleFrames;
        float    fps     = isMoving ? walkFPS    : idleFPS;

        if (desired != _currentSet)
            SwitchAnimation(desired, fps);
    }

    private void SwitchAnimation(Sprite[] frames, float fps)
    {
        _currentSet  = frames;
        _currentFPS  = Mathf.Max(fps, 0.01f);
        _frameIndex  = 0;
        _timer       = 0f;

        if (_currentSet != null && _currentSet.Length > 0)
            _sr.sprite = _currentSet[0];
    }

    private void AdvanceFrame()
    {
        if (_currentSet == null || _currentSet.Length == 0) return;

        _timer += Time.deltaTime;
        float frameDuration = 1f / _currentFPS;

        if (_timer >= frameDuration)
        {
            _timer     -= frameDuration;
            _frameIndex = (_frameIndex + 1) % _currentSet.Length;
            _sr.sprite  = _currentSet[_frameIndex];
        }
    }
}
