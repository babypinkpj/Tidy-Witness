using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum PianoDirection
{
    None      = -1,
    Up        = 0,
    UpRight   = 1,
    Right     = 2,
    DownRight = 3,
    Down      = 4,
    DownLeft  = 5,
    Left      = 6,
    UpLeft    = 7
}

public class PianoPuzzle : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [Tooltip("How many notes the random sequence contains.")]
    public int sequenceLength = 5;

    [Header("Audio")]
    [Range(0.2f, 2f)] public float noteDuration = 0.7f;
    [Range(0f,  1f)]  public float noteVolume   = 0.8f;
    [Tooltip("Assign 8 custom notes. If empty, uses procedural sine waves.")]
    public AudioClip[] customNotes = new AudioClip[8];
    [Tooltip("Distorted sound for failure.")]
    public AudioClip distortedFailSound;
    [Tooltip("Sound for puzzle solved.")]
    public AudioClip solveSound;
    
    private AudioSource _sfxSource;

    [Header("Events")]
    public UnityEvent OnSolved;
    public UnityEvent OnFailed;
    public UnityEvent OnNoteCorrect;

    [Header("References")]
    [Tooltip("Disable player movement while the puzzle is open.")]
    public PlayerController playerController;
    [Tooltip("Assign scripts here to disable during the puzzle (like your Camera Look script or PlayerInput).")]
    public Behaviour[] componentsToDisable;
    [Tooltip("Assign a PianoUI component to show the on-screen overlay.")]
    public PianoUI ui;

    // ── Public read-only state ────────────────────────────────────────────────
    public bool           IsActive      { get; private set; }
    public bool           IsEvaluating  { get; private set; }
    public PianoDirection HeldDirection { get; private set; } = PianoDirection.None;
    public int            CurrentStep   => _input.Count;
    public int            SequenceLength => sequenceLength;
    public PianoDirection[] Sequence    => _sequence;

    // ── Private ───────────────────────────────────────────────────────────────
    private PianoDirection[]     _sequence;
    private List<PianoDirection> _input = new List<PianoDirection>();
    private AudioSource[]        _src   = new AudioSource[8];

    // 8 notes: C4 D4 E4 F4 G4 A4 B4 C5  (one full C-major octave)
    private static readonly float[] Frequencies =
        { 261.63f, 293.66f, 329.63f, 349.23f, 392.00f, 440.00f, 493.88f, 523.25f };

    public static readonly string[] Symbols    = { "↑","↗","→","↘","↓","↙","←","↖" };
    public static readonly string[] NoteLabels = { "C4","D4","E4","F4","G4","A4","B4","C5" };

    // ── Unity ─────────────────────────────────────────────────────────────────

    void Awake()
    {
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.spatialBlend = 0f;

        for (int i = 0; i < 8; i++)
        {
            AudioClip clip = (customNotes != null && i < customNotes.Length && customNotes[i] != null) 
                                ? customNotes[i] 
                                : BakeClip(Frequencies[i], noteDuration);
            var go   = new GameObject("PianoNote_" + NoteLabels[i]);
            go.transform.SetParent(transform);
            var src          = go.AddComponent<AudioSource>();
            src.clip         = clip;
            src.playOnAwake  = false;
            src.spatialBlend = 0f;
            _src[i] = src;
        }
    }

    void Start() => GenerateSequence();

    void Update()
    {
        if (!IsActive || IsEvaluating) return;

        HeldDirection = ReadHeld();
        ui?.OnDirectionChanged(HeldDirection);

        var kb = UnityEngine.InputSystem.Keyboard.current;

        if (HeldDirection != PianoDirection.None && AnyActionKeyDown())
            RegisterNote(HeldDirection);

        if (kb != null && kb.escapeKey.wasPressedThisFrame)
            Deactivate();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Activate()
    {
        IsActive = true;
        GenerateSequence();

        if (playerController) playerController.enabled = false;
        if (componentsToDisable != null)
        {
            foreach (var comp in componentsToDisable)
                if (comp) comp.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        ui?.Show(this);
    }

    public void Deactivate()
    {
        IsActive = false;
        StopAllCoroutines();

        if (playerController) playerController.enabled = true;
        if (componentsToDisable != null)
        {
            foreach (var comp in componentsToDisable)
                if (comp) comp.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        ui?.Hide();
    }

    // ── Puzzle logic ──────────────────────────────────────────────────────────

    void GenerateSequence()
    {
        _sequence = new PianoDirection[sequenceLength];
        for (int i = 0; i < sequenceLength; i++)
            _sequence[i] = (PianoDirection)Random.Range(0, 8);

        _input.Clear();
        ui?.Refresh();

        string dbg = "[PianoPuzzle] New sequence: ";
        foreach (var d in _sequence) dbg += Symbols[(int)d] + " ";
        Debug.Log(dbg);
    }

    void RegisterNote(PianoDirection dir)
    {
        PlayNote((int)dir, 1f, noteVolume);
        _input.Add(dir);
        ui?.Refresh();

        int step = _input.Count - 1;

        if (dir != _sequence[step])
        {
            OnFailed?.Invoke();
            StartCoroutine(FailRoutine());
        }
        else if (_input.Count == sequenceLength)
        {
            StartCoroutine(SolveRoutine());
        }
        else
        {
            OnNoteCorrect?.Invoke();
        }
    }

    IEnumerator FailRoutine()
    {
        IsEvaluating = true;
        ui?.ShowFail(true);

        yield return new WaitForSeconds(0.2f);

        if (distortedFailSound != null)
        {
            _sfxSource.PlayOneShot(distortedFailSound, noteVolume);
            yield return new WaitForSeconds(distortedFailSound.length + 0.1f);
        }
        else
        {
            // Sweep through all 8 notes quickly
            for (int i = 0; i < 8; i++)
            {
                PlayNote(i, 1f, noteVolume * 0.5f);
                yield return new WaitForSeconds(0.05f);
            }

            yield return new WaitForSeconds(0.08f);

            // Distorted chaos: all 8 at random pitches simultaneously
            for (int i = 0; i < 8; i++)
                PlayNote(i, Random.Range(0.30f, 2.00f), noteVolume);

            yield return new WaitForSeconds(noteDuration + 0.4f);
        }

        GenerateSequence();
        IsEvaluating = false;
        ui?.ShowFail(false);
    }

    IEnumerator SolveRoutine()
    {
        IsEvaluating = true;
        ui?.ShowSolve();

        yield return new WaitForSeconds(0.3f);

        if (solveSound != null)
        {
            _sfxSource.PlayOneShot(solveSound, noteVolume);
            yield return new WaitForSeconds(solveSound.length + 0.1f);
        }
        else
        {
            // Ascending arpeggio to celebrate
            for (int i = 0; i < 8; i++)
            {
                PlayNote(i, 1f, noteVolume);
                yield return new WaitForSeconds(0.08f);
            }

            yield return new WaitForSeconds(0.5f);
        }

        OnSolved?.Invoke();
        Debug.Log("[PianoPuzzle] Puzzle Solved!");
        IsEvaluating = false;
        Deactivate();
    }

    // ── Audio ─────────────────────────────────────────────────────────────────

    void PlayNote(int idx, float pitch, float vol)
    {
        if (idx < 0 || idx > 7) return;
        _src[idx].pitch  = pitch;
        _src[idx].volume = vol;
        _src[idx].Stop();
        _src[idx].Play();
    }

    AudioClip BakeClip(float freq, float duration)
    {
        const int rate    = 44100;
        int       samples = Mathf.CeilToInt(rate * duration);
        float[]   data    = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t   = (float)i / rate;
            float env = Mathf.Clamp01(t / 0.02f) * Mathf.Clamp01((duration - t) / 0.2f);

            // Fundamental + 3 harmonics for a richer piano-like timbre
            data[i] = (Mathf.Sin(Mathf.PI * 2f * freq       * t) * 0.65f
                     + Mathf.Sin(Mathf.PI * 2f * freq * 2f  * t) * 0.20f
                     + Mathf.Sin(Mathf.PI * 2f * freq * 3f  * t) * 0.10f
                     + Mathf.Sin(Mathf.PI * 2f * freq * 4f  * t) * 0.05f) * env;
        }

        var clip = AudioClip.Create("Tone_" + (int)freq, samples, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    PianoDirection ReadHeld()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return PianoDirection.None;

        bool u = kb.wKey.isPressed || kb.upArrowKey.isPressed;
        bool d = kb.sKey.isPressed || kb.downArrowKey.isPressed;
        bool l = kb.aKey.isPressed || kb.leftArrowKey.isPressed;
        bool r = kb.dKey.isPressed || kb.rightArrowKey.isPressed;

        if (u && r) return PianoDirection.UpRight;
        if (u && l) return PianoDirection.UpLeft;
        if (d && r) return PianoDirection.DownRight;
        if (d && l) return PianoDirection.DownLeft;
        if (u)      return PianoDirection.Up;
        if (d)      return PianoDirection.Down;
        if (l)      return PianoDirection.Left;
        if (r)      return PianoDirection.Right;

        return PianoDirection.None;
    }

    bool AnyActionKeyDown()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return false;

        return kb.zKey.wasPressedThisFrame;
    }
}
