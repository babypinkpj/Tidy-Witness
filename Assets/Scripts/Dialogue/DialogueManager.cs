using System.Collections;
using UnityEngine;

/// <summary>
/// Singleton that drives the dialogue flow.
/// Place one DialogueManager prefab in your scene and wire up the UI references.
/// The manager raises events so other systems (quest, audio, etc.) can react.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────────
    public static DialogueManager Instance { get; private set; }

    // ── Inspector ──────────────────────────────────────────────────────────────
    [Header("UI Reference")]
    [Tooltip("The DialogueUI component that controls the canvas.")]
    public DialogueUI dialogueUI;

    [Header("Typing Effect")]
    [Tooltip("Characters per second for the typewriter effect. Set to 0 to disable.")]
    public float typeSpeed = 40f;

    // ── State ──────────────────────────────────────────────────────────────────
    private DialogueData _currentData;
    private int          _currentLineIndex;
    private bool         _isTyping;
    private Coroutine    _typeCoroutine;

    public bool IsOpen { get; private set; }

    // ── Events (subscribe from other systems) ─────────────────────────────────
    public System.Action<DialogueData>    OnDialogueStart;
    public System.Action<DialogueLine>    OnLineStart;
    public System.Action<DialogueData>    OnDialogueEnd;

    // ── Unity ──────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Begin a dialogue sequence from a DialogueData asset.</summary>
    public void StartDialogue(DialogueData data)
    {
        if (data == null || data.lines == null || data.lines.Length == 0)
        {
            Debug.LogWarning("[DialogueManager] Tried to start dialogue with no lines.");
            return;
        }

        if (IsOpen) EndDialogue();

        _currentData      = data;
        _currentLineIndex = 0;
        IsOpen            = true;

        dialogueUI.Show();
        OnDialogueStart?.Invoke(_currentData);
        ShowCurrentLine();
    }

    /// <summary>
    /// Advance to the next line. Call this from a UI button or the player's
    /// Interact / Confirm input.
    /// </summary>
    public void Advance()
    {
        if (!IsOpen) return;

        // If still typing → skip to full text immediately
        if (_isTyping)
        {
            SkipTyping();
            return;
        }

        _currentLineIndex++;

        if (_currentLineIndex >= _currentData.lines.Length)
        {
            if (_currentData.loop)
                _currentLineIndex = 0;
            else
            {
                EndDialogue();
                return;
            }
        }

        ShowCurrentLine();
    }

    /// <summary>Close the dialogue box immediately.</summary>
    public void EndDialogue()
    {
        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
        _isTyping = false;
        IsOpen    = false;
        dialogueUI.Hide();
        OnDialogueEnd?.Invoke(_currentData);
        _currentData = null;
    }

    // ── Private ────────────────────────────────────────────────────────────────

    void ShowCurrentLine()
    {
        DialogueLine line = _currentData.lines[_currentLineIndex];
        dialogueUI.SetSpeaker(line.speakerName, line.portrait);
        OnLineStart?.Invoke(line);

        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);

        if (typeSpeed > 0)
            _typeCoroutine = StartCoroutine(TypeLine(line.text));
        else
            dialogueUI.SetBodyText(line.text);
    }

    IEnumerator TypeLine(string fullText)
    {
        _isTyping = true;
        dialogueUI.SetBodyText(string.Empty);
        int charIndex = 0;

        while (charIndex <= fullText.Length)
        {
            dialogueUI.SetBodyText(fullText.Substring(0, charIndex));
            charIndex++;
            yield return new WaitForSeconds(1f / typeSpeed);
        }

        _isTyping = false;
    }

    void SkipTyping()
    {
        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
        _isTyping = false;
        dialogueUI.SetBodyText(_currentData.lines[_currentLineIndex].text);
    }
}
