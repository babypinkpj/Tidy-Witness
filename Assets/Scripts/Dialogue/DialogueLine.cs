using System;
using UnityEngine;

/// <summary>
/// A single line of dialogue.  Fill these in on the DialogueData ScriptableObject.
/// </summary>
[Serializable]
public class DialogueLine
{
    [Tooltip("Name shown in the name-plate. Leave empty to hide the name-plate.")]
    public string speakerName = "NPC";

    [Tooltip("The text displayed in the dialogue box.")]
    [TextArea(2, 6)]
    public string text;

    [Tooltip("Optional portrait sprite shown next to the text.")]
    public Sprite portrait;
}
