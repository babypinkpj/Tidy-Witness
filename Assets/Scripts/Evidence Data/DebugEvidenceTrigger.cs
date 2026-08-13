using UnityEngine;
using UnityEngine.InputSystem;

// สคริปต์ชั่วคราวสำหรับ Play Mode เท่านั้น — ลบทิ้งได้เมื่อ player system จริงเสร็จ
// แปะบน empty GameObject ในฉากทดสอบ กด 1-5 เพื่อ unlock evidence แล้วดูว่า
// ClueNotification เด้งไหม, กด Tab เปิด ClueLogUI แล้วมี entry โผล่ไหม
public class DebugEvidenceTrigger : MonoBehaviour
{
      [SerializeField]
    private string[] testIDs = { "CLUE_01", "CLUE_02", "CLUE_15", "CLUE_22", "CLUE_23" };
 
    private static readonly Key[] NumberKeys =
    {
        Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5,
        Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9
    };
 
    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return; // ไม่มีคีย์บอร์ดต่ออยู่ (เช่นตอน build บนแพลตฟอร์มที่ไม่มี keyboard)
 
        for (int i = 0; i < testIDs.Length && i < NumberKeys.Length; i++)
        {
            if (kb[NumberKeys[i]].wasPressedThisFrame)
            {
                Debug.Log($"[Debug] Unlocking {testIDs[i]}");
                EvidenceManager.Instance.Unlock(testIDs[i]);
            }
        }
    }
}
 
