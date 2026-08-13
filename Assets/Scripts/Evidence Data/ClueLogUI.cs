using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
public class ClueLogUI : MonoBehaviour
{
   [SerializeField] GameObject _panel;
    [SerializeField] Transform  _listParent;
    [SerializeField] ClueLogEntry _entryPrefab; // prefab มี Text EN + TH + icon
 
    void Awake()
        => EvidenceManager.OnEvidenceUnlocked += AddEntry;
 
    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.tabKey.wasPressedThisFrame) Toggle();
    }
 
    void Toggle()
    {
        bool open = !_panel.activeSelf;
        _panel.SetActive(open);
        Time.timeScale = open ? 0f : 1f; // pause in-game time
    }
 
    void AddEntry(EvidenceData data)
    {
        var entry = Instantiate(_entryPrefab, _listParent);
        entry.Set(data); // fill TextMeshPro fields
        // notification pill: ClueNotification.Show(data)
    }
}
