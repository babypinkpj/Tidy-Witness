using UnityEngine;
using System;
// เรียกจาก dialogue system เมื่อ player เลือก option
public class DialogueEvidenceUnlocker : MonoBehaviour
{
     // Call this when dialogue choice is selected
    public static void UnlockOnChoice(string[] evidenceIDs)
    {
        foreach (var id in evidenceIDs)
            EvidenceManager.Instance.Unlock(id);
    }
}

// Dialogue node data structure (ทำ ScriptableObject ไว้รอ)
[Serializable]
public class DialogueChoice
{
    public string labelEN;
    public string labelTH;
    public string[] unlocksEvidenceIDs;  // กรอกได้เลยจาก doc
    public int suspicionDelta;           // +1, -1, 0
    public string nextNodeID;
}
