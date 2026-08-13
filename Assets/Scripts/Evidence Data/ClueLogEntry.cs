using UnityEngine;
using UnityEngine.UI;
using TMPro;



// ติดบน prefab ของแต่ละแถวใน Clue Log
// ต้องมี field ครบ 4 ตัวลาก reference ใน Inspector: TMP_Text ×2 (EN/TH), Image ×2 (tier strip / icon)
public class ClueLogEntry : MonoBehaviour
{
   [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private Image _tierStrip;
    [SerializeField] private Image _icon;
 
    private static readonly Color NormalColor   = new Color(0.55f, 0.55f, 0.55f);
    private static readonly Color MajorColor    = new Color(0.85f, 0.55f, 0.05f);
    private static readonly Color CriticalColor = new Color(0.72f, 0.10f, 0.10f);
 
    public void Set(EvidenceData data)
    {
        if (data == null) return;
 
        // _titleText = หัวข้อภาษาอังกฤษ, _descriptionText = หัวข้อภาษาไทย
        // (ถ้าอยากโชว์ desc เต็มแทน titleTH ให้เปลี่ยนบรรทัดล่างเป็น data.descTH)
        if (_titleText != null) _titleText.text = data.titleEN;
        if (_descriptionText != null) _descriptionText.text = data.titleTH;
 
        if (_tierStrip != null)
        {
            _tierStrip.color = data.tier switch
            {
                EvidenceTier.Critical => CriticalColor,
                EvidenceTier.Major    => MajorColor,
                _                     => NormalColor
            };
        }
 
        if (_icon != null)
        {
            _icon.sprite = data.icon;
            _icon.enabled = data.icon != null;
        }
    }
}
