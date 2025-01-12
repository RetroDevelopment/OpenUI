﻿using RetroDev.OpenUI.Components.AutoSize;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Graphics.Shapes;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Components.Simple;

// TODO: add colors and font size

/// <summary>
/// A label displaying text.
/// </summary>
public class Label : UIComponent
{
    /// <summary>
    /// The display text.
    /// </summary>
    public UIProperty<Label, string> Text { get; }

    /// <inheritdoc/>
    protected override Size ComputeSizeHint() =>
        Application.FontServices.ComputeTextSize(Text.Value);

    /// <inheritdoc />
    protected override bool DefaultIsFocusable => false;

    /// <inheritdoc />
    protected override IAutoSizeStrategy DefaultAutoWidth => AutoSizeStrategy.WrapComponentCenter;

    /// <inheritdoc />
    protected override IAutoSizeStrategy DefaultAutoHeight => AutoSizeStrategy.WrapComponentCenter;

    /// <summary>
    /// Creates a new label.
    /// </summary>
    /// <param name="parent">The application that contain this label.</param>
    public Label(Application parent) : base(parent)
    {
        Text = new UIProperty<Label, string>(this, string.Empty);
        Text.ValueChange += (_, _) => SizeHintCache.MarkDirty();
        RenderFrame += Label_RenderFrame;
    }

    /// <summary>
    /// Creates a new label.
    /// </summary>
    /// <param name="parent">The application that contain this label.</param>
    /// <param name="text">The label text.</param>
    public Label(Application parent, string text) : this(parent)
    {
        Text.Value = text;
    }

    private void Label_RenderFrame(UIComponent sender, RenderingEventArgs e)
    {
        var size = RelativeDrawingArea.Size;
        var canvas = e.Canvas;

        canvas.Render(new Text(new Color(0, 0, 0, 0), new Color(255, 255, 255, 255), Text.Value), new(Point.Zero, size));
    }
}
