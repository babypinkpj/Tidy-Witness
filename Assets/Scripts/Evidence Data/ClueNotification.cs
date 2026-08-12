using UnityEngine;
using System.Collections;
using TMPro;
public class ClueNotification : MonoBehaviour
{
    public static ClueNotification Instance;
    [SerializeField] CanvasGroup _group;
    [SerializeField] TMP_Text    _label;
    [SerializeField] AudioClip   _sfx;

    void Awake()
    {
        Instance = this;
        _group.alpha = 0;
        EvidenceManager.OnEvidenceUnlocked += Show;
    }

    public void Show(EvidenceData data)
    {
        _label.text = "🔍 " + data.titleEN;
        StopAllCoroutines();
        StartCoroutine(FadeRoutine());
        AudioSource.PlayClipAtPoint(_sfx, Camera.main.transform.position);
    }

    IEnumerator FadeRoutine()
    {
        _group.alpha = 1f;
        yield return new WaitForSecondsRealtime(1.5f);
        float t = 0;
        while (t < 0.5f)
        {
            _group.alpha = 1f - (t / 0.5f);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        _group.alpha = 0;
    }
}