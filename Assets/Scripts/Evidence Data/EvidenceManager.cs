using UnityEngine;
using System;
using System.Collections.Generic;
public class EvidenceManager : MonoBehaviour
{
 public static EvidenceManager Instance { get; private set; }
    public static event Action<EvidenceData> OnEvidenceUnlocked;

    private HashSet<string> _unlockedIDs = new();
    public IReadOnlyCollection<string> UnlockedIDs => _unlockedIDs;

    void Awake() { Instance = this; }

    public void Unlock(string evidenceID)
    {
        if (_unlockedIDs.Contains(evidenceID)) return;
        // find asset
        var data = EvidenceLibrary.Get(evidenceID);
        if (data == null) return;
        _unlockedIDs.Add(evidenceID);
        OnEvidenceUnlocked?.Invoke(data);
        CheckFlashbackTrigger();
    }

    public bool Has(string id) => _unlockedIDs.Contains(id);

    void CheckFlashbackTrigger()
    {
        // Flashback triggers when CLUE_15 + CLUE_17 + CLUE_23 all present
        //อันนี้อย่าลืมว่าต้องดูดี ๆ นะจ๊ะ
        if (Has("CLUE_15") && Has("CLUE_17") && Has("CLUE_23"))
            GameStateManager.Instance.SetState(GameState.FlashbackReady);
    }
}
