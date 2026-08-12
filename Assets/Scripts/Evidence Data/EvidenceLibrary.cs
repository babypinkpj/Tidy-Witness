using UnityEngine;
using System;
using System.Collections.Generic;
public class EvidenceLibrary : MonoBehaviour
{
    public static EvidenceLibrary Instance { get; private set; }
 
    [SerializeField] private EvidenceData[] _allEvidence; // drag ทั้ง 25 assets ใส่ที่นี่
 
    private Dictionary<string, EvidenceData> _lookup;
 
    private void Awake()
    {
        Instance = this;
        _lookup = new Dictionary<string, EvidenceData>();
 
        if (_allEvidence == null) return;
 
        foreach (var e in _allEvidence)
        {
            if (e == null) continue;
 
            if (_lookup.ContainsKey(e.id))
                Debug.LogWarning($"Duplicate evidence ID '{e.id}' — check {e.name} against the existing entry.");
 
            _lookup[e.id] = e;
        }
    }
 
    public static EvidenceData Get(string id)
        => Instance != null && Instance._lookup.TryGetValue(id, out var d) ? d : null;
 
    public static EvidenceData[] GetAll() => Instance._allEvidence;
 
    /// <summary>Test-only helper — registers a single piece of evidence without going through the Inspector array.</summary>
    public void Register(EvidenceData data)
    {
        _lookup ??= new Dictionary<string, EvidenceData>();
        if (data != null) _lookup[data.id] = data;
    }
}
