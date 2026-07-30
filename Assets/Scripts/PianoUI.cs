using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PianoUI : MonoBehaviour
{
    [Header("Custom UI References")]
    [Tooltip("The main Canvas or parent GameObject for the UI")]
    public GameObject uiCanvas;
    
    [Tooltip("Text component to show tooltips/info")]
    public TMP_Text tooltipText;
    
    [Tooltip("Assign 8 arrow images in order: Up, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft")]
    public Image[] arrowImages = new Image[8];

    [Header("Settings")]
    public Color arrowIdleColor = new Color(1f, 1f, 1f, 0.5f);
    public Color arrowHeldColor = new Color(0.2f, 0.8f, 0.2f, 1f);

    private void Awake()
    {
        Hide();
    }

    public void Show(PianoPuzzle puzzle)
    {
        if (uiCanvas) uiCanvas.SetActive(true);
        if (tooltipText) tooltipText.text = "Hold direction + press Z";
        ResetArrows();
    }

    public void Hide()
    {
        if (uiCanvas) uiCanvas.SetActive(false);
    }

    public void Refresh()
    {
        // Called when the puzzle state updates (e.g. sequence progresses)
    }

    public void OnDirectionChanged(PianoDirection dir)
    {
        for (int i = 0; i < arrowImages.Length; i++)
        {
            if (arrowImages[i])
            {
                arrowImages[i].color = (i == (int)dir) ? arrowHeldColor : arrowIdleColor;
            }
        }
    }

    public void ShowFail(bool active)
    {
        if (tooltipText)
        {
            tooltipText.text = active ? "Wrong note!" : "Hold direction + press Z";
        }
    }

    public void ShowSolve()
    {
        if (tooltipText)
        {
            tooltipText.text = "Puzzle Solved!";
        }
    }

    private void ResetArrows()
    {
        for (int i = 0; i < arrowImages.Length; i++)
        {
            if (arrowImages[i])
            {
                arrowImages[i].color = arrowIdleColor;
            }
        }
    }
}
