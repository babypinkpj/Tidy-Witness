using UnityEngine;
//เอาไว้ใส่ EvidenceData ของแต่ละ clue / testimony / deduction
[CreateAssetMenu(fileName = "EvidenceData", menuName = "Scriptable Objects/EvidenceData")]
public class EvidenceData : ScriptableObject
{
      public string id;              // "CLUE_01", "CLUE_12" etc.
    public string titleEN;
    public string titleTH;
    [TextArea] public string descEN;
    [TextArea] public string descTH;
    public EvidenceType type;      // Testimony / Physical / Deduction
    public EvidenceTier tier;      // Normal / Major / Critical
    public Sprite icon;            // optional — ใส่ทีหลังได้
    public bool isRedHerring;
    public string[] requiredClueIDs;  // unlock conditions
    public string[] unlocksFlashbackIf; // ถ้าต้องการ trigger flashback
}
public enum EvidenceType  { Testimony, Physical, Deduction }
public enum EvidenceTier  { Normal, Major, Critical }
