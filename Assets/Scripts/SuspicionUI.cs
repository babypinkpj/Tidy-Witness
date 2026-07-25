using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SuspicionUI : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Text References (assign in Inspector)")]
    [Tooltip("Main suspicion label, e.g. 'SUSPICION'.")]
    public TMP_Text labelText;

    [Tooltip("Shows the numeric value and bar, e.g. '████░░░░  50 / 100'.")]
    public TMP_Text valueText;

    [Header("Display Settings")]
    [Tooltip("Label shown above the meter.")]
    public string labelString = "SUSPICION";

    [Tooltip("Number of filled characters used as the bar fill.")]
    public int barLength = 10;

    [Tooltip("Character used for the filled portion of the bar.")]
    public string filledChar = "█";

    [Tooltip("Character used for the empty portion of the bar.")]
    public string emptyChar  = "░";

    [Header("Colours (applied to valueText)")]
    public Color colourLow    = new Color(0.2f, 0.9f, 0.2f);   // green
    public Color colourMedium = new Color(1.0f, 0.8f, 0.0f);   // yellow
    public Color colourHigh   = new Color(1.0f, 0.3f, 0.1f);   // red

    [Header("Visibility")]
    [Tooltip("Hide the UI completely when suspicion is 0.")]
    public bool hideWhenZero = true;

    [Tooltip("Parent CanvasGroup used for fading. Auto-created if missing.")]
    public CanvasGroup canvasGroup;

    [Tooltip("Fade speed when showing / hiding.")]
    public float fadeSpeed = 4f;

    // ── Private ───────────────────────────────────────────────────────────────

    private float _current;
    private float _max;
    private float _targetAlpha;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        // Auto-create CanvasGroup for fade support
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        if (SuspicionManager.Instance != null)
        {
            _max     = SuspicionManager.Instance.Max;
            _current = SuspicionManager.Instance.Current;

            SuspicionManager.Instance.OnSuspicionChanged += HandleSuspicionChanged;
        }
        else
        {
            Debug.LogWarning("[SuspicionUI] No SuspicionManager found in scene.", this);
        }

        if (labelText != null)
            labelText.text = labelString;

        Refresh();
    }

    void OnDestroy()
    {
        if (SuspicionManager.Instance != null)
            SuspicionManager.Instance.OnSuspicionChanged -= HandleSuspicionChanged;
    }

    void Update()
    {
        // Smooth fade
        if (canvasGroup != null)
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, _targetAlpha, fadeSpeed * Time.deltaTime);
    }

    // ── Event handler ─────────────────────────────────────────────────────────

    private void HandleSuspicionChanged(float current, float max)
    {
        _current = current;
        _max     = max;
        Refresh();
    }

    // ── Display ───────────────────────────────────────────────────────────────

    private void Refresh()
    {
        float ratio = _max > 0f ? _current / _max : 0f;

        // Visibility
        _targetAlpha = (hideWhenZero && _current <= 0f) ? 0f : 1f;

        if (valueText == null) return;

        // Build text bar  ████████░░  75 / 100
        int filled = Mathf.RoundToInt(ratio * barLength);
        int empty  = barLength - filled;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < filled; i++) sb.Append(filledChar);
        for (int i = 0; i < empty;   i++) sb.Append(emptyChar);
        sb.Append($"  {Mathf.RoundToInt(_current)} / {Mathf.RoundToInt(_max)}");

        valueText.text = sb.ToString();

        // Colour
        if (ratio < 0.4f)
            valueText.color = colourLow;
        else if (ratio < 0.75f)
            valueText.color = Color.Lerp(colourLow, colourMedium, (ratio - 0.4f) / 0.35f);
        else
            valueText.color = Color.Lerp(colourMedium, colourHigh, (ratio - 0.75f) / 0.25f);
    }
}
