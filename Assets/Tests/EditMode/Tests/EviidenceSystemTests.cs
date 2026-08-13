using UnityEngine;
using NUnit.Framework;

public class EviidenceSystemTests : MonoBehaviour
{
   private GameObject _libGo;
    private GameObject _mgrGo;
    private EvidenceManager _mgr;
 
    [SetUp]
    public void SetUp()
    {
        _libGo = new GameObject("TestLibrary");
        var lib = _libGo.AddComponent<EvidenceLibrary>();
 
        var clue = ScriptableObject.CreateInstance<EvidenceData>();
        clue.id = "CLUE_01";
        clue.titleEN = "Test clue";
        lib.Register(clue);
 
        _mgrGo = new GameObject("TestManager");
        _mgr = _mgrGo.AddComponent<EvidenceManager>();
    }
 
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_mgrGo);
        Object.DestroyImmediate(_libGo);
    }
 
    [Test]
    public void Unlock_SameID_OnlyFiresOnce()
    {
        int count = 0;
        EvidenceManager.OnEvidenceUnlocked += _ => count++;
 
        _mgr.Unlock("CLUE_01");
        _mgr.Unlock("CLUE_01"); // duplicate
 
        Assert.AreEqual(1, count);
    }
 
    [Test]
    public void Unlock_UnknownID_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _mgr.Unlock("CLUE_DOES_NOT_EXIST"));
    }
 
    [Test]
    public void Has_ReturnsFalse_BeforeUnlock()
    {
        Assert.IsFalse(_mgr.Has("CLUE_01"));
    }
 
    [Test]
    public void Has_ReturnsTrue_AfterUnlock()
    {
        _mgr.Unlock("CLUE_01");
        Assert.IsTrue(_mgr.Has("CLUE_01"));
    }
 
    // TODO: เพิ่ม test สำหรับ AccusationValidator.EvaluateEnding() เมื่อพร้อม
    // seed evidence ครบ 12 ชิ้น + 4 critical IDs (CLUE_15, 16, 22, 23) ใน SetUp
}
