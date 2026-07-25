using UnityEngine;

/// <summary>
/// Attach this to any NPC GameObject.
/// Drag a DialogueData asset into the 'dialogueData' field — that's all you need.
/// </summary>
public class NPCDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Asset")]
    [Tooltip("The ScriptableObject that holds this NPC's dialogue lines.")]
    public DialogueData dialogueData;

    [Header("Interaction Settings")]
    [Tooltip("How close the player must be to start a conversation.")]
    public float interactRange = 3f;

    [Tooltip("Show a floating indicator above the NPC when the player is in range.")]
    public GameObject interactIndicator;

    // ── called by PlayerController when the player presses Interact ──────────
    public void TryStartDialogue(Transform playerTransform)
    {
        if (dialogueData == null)
        {
            Debug.LogWarning($"[NPCDialogueTrigger] '{gameObject.name}' has no DialogueData assigned!", this);
            return;
        }

        float dist = Vector3.Distance(playerTransform.position, transform.position);
        if (dist > interactRange)
        {
            Debug.Log($"[NPCDialogueTrigger] Player is too far from '{gameObject.name}' ({dist:F1} > {interactRange}).");
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogueData);
    }

    // ── optional indicator driven by proximity ────────────────────────────────
    private Transform _playerTransform;

    void Start()
    {
        // Try to find the player automatically (works if the player is tagged "Player")
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;

        SetIndicator(false);
    }

    void Update()
    {
        if (interactIndicator == null || _playerTransform == null) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) 
        { 
            SetIndicator(false); 
            return; 
        }

        float dist = Vector3.Distance(_playerTransform.position, transform.position);
        SetIndicator(dist <= interactRange);
    }

    void SetIndicator(bool show)
    {
        if (interactIndicator != null) interactIndicator.SetActive(show);
    }
}
