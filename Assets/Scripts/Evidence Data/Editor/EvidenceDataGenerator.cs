
using UnityEngine;
using UnityEditor;


// Editor-only tool — ต้องอยู่ในโฟลเดอร์ชื่อ "Editor" เท่านั้น (เช่น
// Assets/Scripts/Evidence Data/Editor/) ไม่งั้นจะโดนรวมเข้า build จริง
public class EvidenceDataGenerator
{
     private struct Row
    {
        public string id, titleEN;
        public EvidenceType type;
        public EvidenceTier tier;
        public bool redHerring;
 
        public Row(string id, string titleEN, EvidenceType type, EvidenceTier tier, bool redHerring)
        {
            this.id = id;
            this.titleEN = titleEN;
            this.type = type;
            this.tier = tier;
            this.redHerring = redHerring;
        }
    }
 
    private static readonly Row[] Rows =
    {
        new("CLUE_01", "Elliot never saw the painting — excluded by Vincenzo", EvidenceType.Testimony, EvidenceTier.Normal, false),
        new("CLUE_02", "Elliot has a personal timeline — needs done before 9pm", EvidenceType.Testimony, EvidenceTier.Normal, false),
        new("CLUE_03", "Elliot knows the Gallery layout exactly, as well as the owner does", EvidenceType.Testimony, EvidenceTier.Normal, false),
        new("CLUE_04", "Elliot and Vincenzo have CONFLICTING orders about the Music Room", EvidenceType.Testimony, EvidenceTier.Major, false),
        new("CLUE_05", "Elliot controls Clara — ordered her to lie to Vincenzo", EvidenceType.Testimony, EvidenceTier.Critical, false),
        new("CLUE_06", "Borislav ties himself to \"loss\" — emotional weight, possible motive", EvidenceType.Testimony, EvidenceTier.Normal, false),
        new("CLUE_07", "Vincenzo changed the piano request without explanation", EvidenceType.Testimony, EvidenceTier.Normal, false),
        new("CLUE_08", "Vincenzo was anxious — not simply excited — on his own exhibition night", EvidenceType.Testimony, EvidenceTier.Normal, false),
        new("CLUE_09", "Katherine possesses proof of Vincenzo's document forgery", EvidenceType.Physical, EvidenceTier.Normal, true),
        new("CLUE_10", "Katherine lied — she was heading toward Study Room, not the bathroom", EvidenceType.Testimony, EvidenceTier.Normal, true),
        new("CLUE_11", "Benedetto has a \"copy\" of something Vincenzo has — plans to retrieve it tonight", EvidenceType.Testimony, EvidenceTier.Major, true),
        new("CLUE_12", "Palm smear on the painting frame — pressed downward, post-cleaning", EvidenceType.Physical, EvidenceTier.Normal, false),
        new("CLUE_13", "Carpet pile bent TOWARD the Gallery — consistent with a body being dragged", EvidenceType.Physical, EvidenceTier.Normal, false),
        new("CLUE_14", "Cleaning rag missing from the cart — the murderer took it", EvidenceType.Physical, EvidenceTier.Major, false),
        new("CLUE_15", "Murder weapon (wire) + forged documents naming Silas McNeil", EvidenceType.Physical, EvidenceTier.Critical, false),
        new("CLUE_16", "Family photograph — Elliot IS Silas, the portrait is his grandmother", EvidenceType.Physical, EvidenceTier.Critical, false),
        new("CLUE_17", "McNeil family ledger — legal record of the stolen inheritance", EvidenceType.Physical, EvidenceTier.Critical, false),
        new("CLUE_18", "Photograph: Vincenzo Sr. receiving the stolen painting", EvidenceType.Physical, EvidenceTier.Major, false),
        new("CLUE_19", "French letter — the original theft documented by a witness", EvidenceType.Physical, EvidenceTier.Normal, false),
        new("CLUE_20", "Borislav confirms: Elliot, not Vincenzo, changed the performance schedule", EvidenceType.Testimony, EvidenceTier.Critical, false),
        new("CLUE_21", "Vincenzo personally confirmed the original setlist to Borislav", EvidenceType.Testimony, EvidenceTier.Major, false),
        new("CLUE_22", "Clara directly names Elliot: \"Elliot asked me. Not Mr. Vincenzo.\"", EvidenceType.Testimony, EvidenceTier.Critical, false),
        new("CLUE_23", "The performance schedule is in Clara's handwriting — physical proof", EvidenceType.Physical, EvidenceTier.Critical, false),
        new("CLUE_24", "Benedetto confirms the Study Room theft — his crime is theft, not murder", EvidenceType.Testimony, EvidenceTier.Normal, true),
        new("CLUE_25", "Katherine confirms she tried the Study Room — her motive is fraud, not murder", EvidenceType.Testimony, EvidenceTier.Normal, true),
    };
 
    [MenuItem("TidyWitness/Generate Placeholder Evidence Assets")]
    private static void Generate()
    {
        const string folder = "Assets/Data/Evidence";
 
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/Data", "Evidence");
 
        int created = 0, skipped = 0;
 
        foreach (var row in Rows)
        {
            string path = $"{folder}/{row.id}.asset";
 
            if (AssetDatabase.LoadAssetAtPath<EvidenceData>(path) != null)
            {
                skipped++;
                continue;
            }
 
            var data = ScriptableObject.CreateInstance<EvidenceData>();
            data.id = row.id;
            data.titleEN = row.titleEN;
            data.titleTH = "(ยังไม่ได้แปล)";
            data.descEN = "TODO: เขียน description เต็ม";
            data.descTH = "TODO: เขียน description ภาษาไทย";
            data.type = row.type;
            data.tier = row.tier;
            data.isRedHerring = row.redHerring;
 
            AssetDatabase.CreateAsset(data, path);
            created++;
        }
 
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
 
        Debug.Log($"[EvidenceDataGenerator] Created {created} new assets, skipped {skipped} that already existed. Check {folder}/");
    }
}
