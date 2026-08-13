using UnityEngine;
//ตรวจว่ามีหลักฐาานพอจะฟ้องใครได้หรือไม่ และจะนำไปสู่ ending ไหน
using System;
using System.Collections.Generic;
public class AccusationValidator : MonoBehaviour
{
     // Critical IDs ที่ต้องมีเพื่อ full confrontation
    static readonly string[] CriticalIDs = {
        "CLUE_15", // murder weapon + Silas McNeil docs
        "CLUE_16", // family photograph
        "CLUE_22", // Clara names Elliot
        "CLUE_23", // schedule in Clara's handwriting
    };

    public static EndingType EvaluateEnding()
    {
        var mgr = EvidenceManager.Instance;
        int total = mgr.UnlockedIDs.Count;
        bool allCritical = System.Array.TrueForAll(
            CriticalIDs, id => mgr.Has(id));

        if (allCritical && total >= 12) return EndingType.A_FullResolution;
        if (total >= 8)                  return EndingType.B_GoodResolution;
        if (total >= 5)                  return EndingType.C_Partial;
        return EndingType.D_Bad;
    }

    public static bool CanConfrontElliot()
        => EvidenceManager.Instance.Has("CLUE_15")
        && EvidenceManager.Instance.Has("CLUE_22");
}
public enum EndingType { A_FullResolution, B_GoodResolution, C_Partial, D_Bad }