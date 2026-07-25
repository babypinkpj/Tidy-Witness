using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the dialogue canvas.  Wire up all UI references in the Inspector.
/// This script only handles display — DialogueManager handles the logic.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Panel")]
    [Tooltip("Root dialogue panel (the entire box that slides in/out).")]
    public GameObject dialoguePanel;

    [Header("Text Fields")]
    [Tooltip("The speaker name text (e.g. 'Old Guard').")]
    public TMP_Text speakerNameText;

    [Tooltip("The main body text where the dialogue line is shown.")]
    public TMP_Text bodyText;

    [Header("Portrait")]
    [Tooltip("Image component used to show the speaker's portrait. Set alpha=0 to hide when empty.")]
    public Image portraitImage;

    [Header("Continue Indicator")]
    [Tooltip("A blinking arrow / icon shown when the player can press to advance. Optional.")]
    public GameObject continueIndicator;

    // ── Animation (optional) ───────────────────────────────────────────────────
    private Animator _animator;
    private static readonly int ShowHash = Animator.StringToHash("Show");

    void Awake()
    {
        _animator = dialoguePanel != null ? dialoguePanel.GetComponent<Animator>() : null;
        Hide();
    }

    // ── Called by DialogueManager ─────────────────────────────────────────────

    public void Show()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (_animator != null) _animator.SetBool(ShowHash, true);
        SetContinueIndicator(false);
    }

    public void Hide()
    {
        if (_animator != null)
            _animator.SetBool(ShowHash, false);
        else if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        SetContinueIndicator(false);
    }

    public void SetSpeaker(string speakerName, Sprite portrait)
    {
        // Name plate
        if (speakerNameText != null)
        {
            speakerNameText.text = speakerName;
            speakerNameText.gameObject.SetActive(!string.IsNullOrEmpty(speakerName));
        }

        // Portrait
        if (portraitImage != null)
        {
            bool hasPortrait = portrait != null;
            portraitImage.gameObject.SetActive(hasPortrait);
            if (hasPortrait) portraitImage.sprite = portrait;
        }

        SetContinueIndicator(false);
    }

    public void SetBodyText(string text)
    {
        if (bodyText != null) bodyText.text = text;
    }

    /// <summary>Show/hide the "press to continue" indicator.</summary>
    public void SetContinueIndicator(bool show)
    {
        if (continueIndicator != null) continueIndicator.SetActive(show);
    }
}
