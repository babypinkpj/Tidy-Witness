using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PianoUI : MonoBehaviour
{
    // Builds itself entirely in code — no prefab or scene setup required.
    // Assign this component to any GameObject (e.g. the PianoPuzzle object).

    private PianoPuzzle _puzzle;

    private Canvas    _canvas;
    private Image     _panel;
    private Text      _infoText;
    private Text      _titleText;

    // Direction key cells indexed by (int)PianoDirection (0-7)
    private Image[] _keyImages = new Image[8];
    private Image[] _keyBorder = new Image[8];

    // Sequence progress dots
    private List<Image> _dots = new List<Image>();

    // ── Colors ────────────────────────────────────────────────────────────────
    static readonly Color BgNormal  = new Color(0.04f, 0.04f, 0.12f, 0.96f);
    static readonly Color BgFail    = new Color(0.18f, 0.02f, 0.02f, 0.97f);
    static readonly Color BgSolve   = new Color(0.02f, 0.16f, 0.06f, 0.97f);
    static readonly Color KeyIdle   = new Color(0.13f, 0.13f, 0.26f, 1f);
    static readonly Color KeyBorder = new Color(0.25f, 0.25f, 0.50f, 1f);
    static readonly Color KeyHeld   = new Color(0.22f, 0.60f, 1.00f, 1f);
    static readonly Color KeyHeldBd = new Color(0.50f, 0.80f, 1.00f, 1f);
    static readonly Color DotEmpty  = new Color(0.20f, 0.20f, 0.40f, 1f);
    static readonly Color DotFilled = new Color(0.22f, 0.88f, 0.52f, 1f);
    static readonly Color DotFail   = new Color(0.92f, 0.22f, 0.22f, 1f);
    static readonly Color DotSolve  = new Color(0.40f, 1.00f, 0.70f, 1f);
    static readonly Color TextMain  = new Color(0.92f, 0.92f, 1.00f, 1f);
    static readonly Color TextSub   = new Color(0.58f, 0.58f, 0.82f, 1f);
    static readonly Color Accent    = new Color(0.52f, 0.76f, 1.00f, 1f);

    // Grid layout: [row, col] → PianoDirection  (None = center gap)
    //  ↖  ↑  ↗
    //  ←  ·  →
    //  ↙  ↓  ↘
    static readonly PianoDirection[,] GridDir =
    {
        { PianoDirection.UpLeft,   PianoDirection.Up,   PianoDirection.UpRight   },
        { PianoDirection.Left,     PianoDirection.None, PianoDirection.Right      },
        { PianoDirection.DownLeft, PianoDirection.Down, PianoDirection.DownRight  }
    };

    // ── Unity ─────────────────────────────────────────────────────────────────

    void Awake() => Build();

    // ── Public API ────────────────────────────────────────────────────────────

    public void Show(PianoPuzzle puzzle)
    {
        _puzzle = puzzle;
        _canvas.gameObject.SetActive(true);
        _panel.color   = BgNormal;
        _infoText.text = "Hold direction + press any key";
        RebuildDots(puzzle.SequenceLength);
        Refresh();
        ResetKeyColors();
    }

    public void Hide() => _canvas.gameObject.SetActive(false);

    public void Refresh()
    {
        if (_puzzle == null) return;
        UpdateDots(_puzzle.CurrentStep);
    }

    public void OnDirectionChanged(PianoDirection dir)
    {
        for (int i = 0; i < 8; i++)
        {
            bool held = (i == (int)dir);
            if (_keyImages[i]) _keyImages[i].color = held ? KeyHeld   : KeyIdle;
            if (_keyBorder[i]) _keyBorder[i].color = held ? KeyHeldBd : KeyBorder;
        }
    }

    public void ShowFail(bool active)
    {
        _panel.color   = active ? BgFail  : BgNormal;
        _infoText.text = active ? "✗  Wrong note! Resetting..." : "Hold direction + press any key";
        if (active) StartCoroutine(FlashDots(DotFail));
    }

    public void ShowSolve()
    {
        _panel.color   = BgSolve;
        _infoText.text = "✓  Puzzle Solved!";
        StartCoroutine(FlashDots(DotSolve));
    }

    // ── Build ─────────────────────────────────────────────────────────────────

    void Build()
    {
        // Canvas
        var cGo = new GameObject("PianoCanvas");
        cGo.transform.SetParent(transform, false);
        _canvas = cGo.AddComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 200;

        var scaler = cGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;
        cGo.AddComponent<GraphicRaycaster>();
        cGo.SetActive(false);

        // Dark background panel — centred
        var panelGo = MkImg(cGo.transform, "Panel", BgNormal,
            Vector2.zero, new Vector2(480f, 580f));
        _panel = panelGo.GetComponent<Image>();

        // Title
        _titleText = MkTxt(panelGo.transform, "Title", "PIANO PUZZLE",
            27, FontStyle.Bold, Accent,
            new Vector2(0f, 255f), new Vector2(460f, 40f));

        // Instruction line
        _infoText = MkTxt(panelGo.transform, "Info",
            "Hold direction + press any key",
            13, FontStyle.Normal, TextSub,
            new Vector2(0f, 218f), new Vector2(460f, 26f));

        // Thin divider
        MkImg(panelGo.transform, "Div1",
            new Color(0.3f, 0.3f, 0.6f, 0.4f),
            new Vector2(0f, 198f), new Vector2(420f, 1f));

        // Sequence dots (initial — rebuilt on Show)
        BuildDots(5, panelGo.transform);

        // 3×3 direction grid
        BuildGrid(panelGo.transform);

        // Thin divider below grid
        MkImg(panelGo.transform, "Div2",
            new Color(0.3f, 0.3f, 0.6f, 0.4f),
            new Vector2(0f, -195f), new Vector2(420f, 1f));

        // Escape hint
        MkTxt(panelGo.transform, "EscHint", "[ESC]  Close",
            11, FontStyle.Italic, new Color(0.40f, 0.40f, 0.62f, 1f),
            new Vector2(0f, -262f), new Vector2(300f, 24f));
    }

    void BuildGrid(Transform parent)
    {
        const float cell  = 84f;
        const float gap   = 7f;
        const float step  = cell + gap;
        const float offY  = -18f;

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                float cx = (col - 1) * step;
                float cy = (1 - row) * step + offY;

                PianoDirection dir = GridDir[row, col];

                if (dir == PianoDirection.None)
                {
                    // Centre pip
                    MkImg(parent, "Pip",
                        new Color(0.28f, 0.28f, 0.50f, 0.50f),
                        new Vector2(cx, cy),
                        new Vector2(cell * 0.22f, cell * 0.22f));
                    continue;
                }

                int idx = (int)dir;

                // Border (slightly larger)
                var border = MkImg(parent, "Brd_" + PianoPuzzle.Symbols[idx],
                    KeyBorder,
                    new Vector2(cx, cy),
                    new Vector2(cell + 4f, cell + 4f));
                _keyBorder[idx] = border.GetComponent<Image>();

                // Key face
                var face = MkImg(parent, "Key_" + PianoPuzzle.Symbols[idx],
                    KeyIdle,
                    new Vector2(cx, cy),
                    new Vector2(cell, cell));
                _keyImages[idx] = face.GetComponent<Image>();

                // Direction symbol (large)
                MkTxt(face.transform, "Sym", PianoPuzzle.Symbols[idx],
                    28, FontStyle.Bold, TextMain,
                    new Vector2(0f, 11f), new Vector2(cell, 36f));

                // Note name (small)
                MkTxt(face.transform, "Note", PianoPuzzle.NoteLabels[idx],
                    10, FontStyle.Normal, TextSub,
                    new Vector2(0f, -19f), new Vector2(cell, 20f));
            }
        }
    }

    // ── Sequence dots ──────────────────────────────────────────────────────────

    void BuildDots(int count, Transform parent)
    {
        _dots.Clear();
        const float dotSize = 18f;
        float spacing = 30f;
        float startX  = -(count - 1) * spacing * 0.5f;
        const float dotY = 168f;

        for (int i = 0; i < count; i++)
        {
            var go = MkImg(parent, "Dot_" + i, DotEmpty,
                new Vector2(startX + i * spacing, dotY),
                new Vector2(dotSize, dotSize));
            _dots.Add(go.GetComponent<Image>());
        }
    }

    void RebuildDots(int count)
    {
        foreach (var d in _dots) if (d) Destroy(d.gameObject);
        if (_panel) BuildDots(count, _panel.transform);
    }

    void UpdateDots(int filledCount)
    {
        for (int i = 0; i < _dots.Count; i++)
            if (_dots[i]) _dots[i].color = i < filledCount ? DotFilled : DotEmpty;
    }

    IEnumerator FlashDots(Color flashColor)
    {
        for (int f = 0; f < 5; f++)
        {
            foreach (var d in _dots) if (d) d.color = flashColor;
            yield return new WaitForSeconds(0.09f);
            foreach (var d in _dots) if (d) d.color = DotEmpty;
            yield return new WaitForSeconds(0.09f);
        }
    }

    void ResetKeyColors()
    {
        for (int i = 0; i < 8; i++)
        {
            if (_keyImages[i]) _keyImages[i].color = KeyIdle;
            if (_keyBorder[i]) _keyBorder[i].color = KeyBorder;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static Font _builtinFont;
    static Font GetFont()
    {
        if (_builtinFont == null)
        {
            _builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_builtinFont == null)
                _builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        return _builtinFont;
    }

    static GameObject MkImg(Transform parent, string name, Color col, Vector2 pos, Vector2 size)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = col;
        var rt  = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
        return go;
    }

    static Text MkTxt(Transform parent, string name, string content,
                      int fontSize, FontStyle style, Color col,
                      Vector2 pos, Vector2 size)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var t   = go.AddComponent<Text>();
        t.text      = content;
        t.fontSize  = fontSize;
        t.fontStyle = style;
        t.color     = col;
        t.alignment = TextAnchor.MiddleCenter;
        t.font      = GetFont();
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
        return t;
    }
}
