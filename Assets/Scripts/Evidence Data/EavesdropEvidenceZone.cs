using UnityEngine;
// Attach บน empty GameObject ที่มี SphereCollider (radius=4m) trigger
//ต้องมี PlayerState.IsCleaningMode
public class EavesdropEvidenceZone : MonoBehaviour
{
    [SerializeField] string[] evidenceIDs; // กรอกจาก doc
    [SerializeField] float radius = 4f;
    bool _playerInside;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) _playerInside = true;
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) _playerInside = false;
    }

    // เรียกจาก NPC เมื่อ conversation เริ่ม หรือ poll ใน Update
    public void TryUnlock()
    {
        if (!_playerInside) return;
        if (!PlayerState.Instance.IsCleaningMode) return; // stub interface
        foreach (var id in evidenceIDs)
            EvidenceManager.Instance.Unlock(id);
    }
}

// PlayerState stub — เพื่อนเติม implementation ทีหลัง
public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; private set; }
    public bool IsCleaningMode { get; set; } // เพื่อน set ค่านี้
    void Awake() => Instance = this;
}
