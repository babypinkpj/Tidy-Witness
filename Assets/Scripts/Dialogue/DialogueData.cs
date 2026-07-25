using UnityEngine;

/// <summary>
/// ScriptableObject asset that holds a full dialogue sequence for one NPC conversation.
/// Create via: Right-click in Project → Create → Dialogue / Dialogue Data
/// </summary>
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Tooltip("All lines in order. The manager will play them one by one.")]
    public DialogueLine[] lines;

    [Tooltip("If true the dialogue will loop back to the first line after the last one.")]
    public bool loop = false;
}
