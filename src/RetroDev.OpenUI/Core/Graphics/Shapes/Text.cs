﻿using RetroDev.OpenUI.Core.Graphics.Fonts;
using RetroDev.OpenUI.Core.Windowing;

namespace RetroDev.OpenUI.Core.Graphics.Shapes;

/// <summary>
/// Text to render.
/// </summary>
/// <param name="dispatcher">Manages the UI thread and dispatches UI operations from other thread to the UI thread.</param>
/// <param name="font">The initial font.</param>
public class Text(ThreadDispatcher dispatcher, Font font) : RenderingElement(dispatcher)
{
    private Color _foregroundColor = Color.Transparent;
    private string _value = string.Empty;
    private Font _font = font;

    /// <summary>
    /// The text color.
    /// </summary>
    public Color ForegroundColor
    {
        get => _foregroundColor;
        set => SetValue(ref _foregroundColor, value);
    }

    /// <summary>
    /// The text string.
    /// </summary>
    public string Value
    {
        get => _value;
        set => SetValue(ref _value, value);
    }

    /// <summary>
    /// The text font.
    /// </summary>
    public Font Font
    {
        get => _font;
        set => SetValue(ref _font, value);
    }
}
