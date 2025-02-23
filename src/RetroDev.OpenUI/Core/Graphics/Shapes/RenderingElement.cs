﻿using RetroDev.OpenUI.Core.Graphics.Coordinates;
using RetroDev.OpenUI.Core.Windowing;
using RetroDev.OpenUI.Core.Windowing.Events;

namespace RetroDev.OpenUI.Core.Graphics.Shapes;

/// <summary>
/// Defines a generic element to render.
/// This class enables retained mode rendering, so it is meant to be stored in a <see cref="IRenderingEngine"/> and
/// modify its values whenever somethign changes, instead of creating a new shape for each frame.
/// </summary>
/// <param name="dispatcher">Manages the UI thread and dispatches UI operations from other thread to the UI thread.</param>
public abstract class RenderingElement(ThreadDispatcher dispatcher)
{
    private readonly ThreadDispatcher _dispatcher = dispatcher;

    private Area _renderingArea = Area.Empty;
    private Area _clipArea = Area.Empty;
    private Color _backgroundColor = Color.Transparent;
    private bool _visible = true;

    /// <summary>
    /// Whether the shape has changed and therefore needs re-drawing.
    /// </summary>
    public TypeSafeEventHandler<RenderingElement, EventArgs>? ShapeChanged;

    /// <summary>
    /// The area where to renderer <see langword="this" /> shape.
    /// Coordinates are in pixels and relative tho the rendering viewport .
    /// </summary>
    public Area RenderingArea
    {
        get => _renderingArea;
        set => SetValue(ref _renderingArea, value);
    }

    /// <summary>
    /// The portion of the rendering viewport where <see langword="this" /> will be displayed.
    /// If <see cref="RenderingArea"/> is bigger than <see cref="ClipArea"/>, part of the shape will be clipped.
    /// </summary>
    public Area ClipArea
    {
        get => _clipArea;
        set => SetValue(ref _clipArea, value);
    }

    /// <summary>
    /// The shape background color.
    /// </summary>
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set => SetValue(ref _backgroundColor, value);
    }

    /// <summary>
    /// Whether the component is visible.
    /// </summary>
    public bool Visible
    {
        get => _visible;
        set => SetValue(ref _visible, value);
    }

    /// <summary>
    /// Sets a <paramref name="field"/> value.
    /// </summary>
    /// <typeparam name="TValue">The type of the propery.</typeparam>
    /// <param name="field">The field to set.</param>
    /// <param name="value">The value to set.</param>
    protected void SetValue<TValue>(ref TValue field, TValue value)
    {
        _dispatcher.ThrowIfNotOnUIThread();
        if (!EqualityComparer<TValue>.Default.Equals(field, value))
        {
            ShapeChanged?.Invoke(this, EventArgs.Empty);
            field = value;
        }
    }
}
