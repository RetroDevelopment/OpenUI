﻿using RetroDev.OpenUI.Components.AutoArea;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Graphics.Shapes;
using RetroDev.OpenUI.Properties;
using RetroDev.OpenUI.Themes;

namespace RetroDev.OpenUI.Components.Simple;

// TODO: add colors and font size

/// <summary>
/// A checkbox to mark with a tick if something is checked.
/// </summary>
public class CheckBox : UIComponent
{
    /// <summary>
    /// Whether the checkbox is checked.
    /// </summary>
    public UIProperty<CheckBox, bool> Checked { get; }

    /// <summary>
    /// The color of the check box circle.
    /// </summary>
    public UIProperty<CheckBox, Color> CircleColor { get; }

    /// <summary>
    /// The background color of the checkbox when it <see cref="CheckBox"/> is <see langword="false" />.
    /// </summary>
    public UIProperty<CheckBox, Color> UncheckedBackgroundColor { get; }

    /// <summary>
    /// The background color when the checkbox is disabled.
    /// </summary>
    public UIProperty<CheckBox, Color> DisabledBackgroundColor { get; }

    /// <summary>
    /// The color indicating that the checkbox is focused.
    /// </summary>
    public UIProperty<CheckBox, Color> FocusColor { get; }

    /// <inheritdoc/>
    protected override Size ComputeSizeHint() => new(80, 30); // TODO: Maybe same size as default label text size (which is 20).

    /// <summary>
    /// Creates a new checkbox.
    /// </summary>
    /// <param name="parent">The application that contain this checkbox.</param>
    public CheckBox(Application parent) : base(parent, autoWidth: AutoSize.Wrap, autoHeight: AutoSize.Wrap)
    {
        Checked = new UIProperty<CheckBox, bool>(this, false);
        CircleColor = new UIProperty<CheckBox, Color>(this, Application.Theme.TextColor, BindingType.DestinationToSource);
        UncheckedBackgroundColor = new UIProperty<CheckBox, Color>(this, Application.Theme.SecondaryColorDisabled, BindingType.DestinationToSource);
        DisabledBackgroundColor = new UIProperty<CheckBox, Color>(this, Application.Theme.PrimaryColorDisabled, BindingType.DestinationToSource);
        FocusColor = new UIProperty<CheckBox, Color>(this, Application.Theme.BorderColor, BindingType.DestinationToSource);

        BackgroundColor.BindDestinationToSource(Application.Theme.SecondaryColorDisabled);

        RenderFrame += CheckBox_RenderFrame;
        MousePress += CheckBox_MousePress;
    }

    private void CheckBox_RenderFrame(UIComponent sender, RenderingEventArgs e)
    {
        var size = RelativeDrawingArea.Size;
        var canvas = e.Canvas;
        float minimumDimension = Math.Min(size.Width, size.Height);
        PixelUnit cornerRadius = minimumDimension / 2.0f;
        var circleRadius = size.Height;
        var backgroundColor = Enabled ? (Checked ? BackgroundColor.Value : UncheckedBackgroundColor.Value) : DisabledBackgroundColor.Value;
        var backgroundRectangle = new Rectangle(backgroundColor, CornerRadiusX: cornerRadius, CornerRadiusY: cornerRadius);
        var circleDrawArea = Checked ? new Area(new Point(size.Width - circleRadius, 0.0f), new Size(size.Height, size.Height)) :
                                       new Area(Point.Zero, new Size(size.Height, size.Height));
        var circle = new Circle(CircleColor);
        var focusRectangle = new Rectangle(BorderColor: FocusColor, BorderThickness: 5.0f, CornerRadiusX: cornerRadius, CornerRadiusY: cornerRadius);

        canvas.Render(backgroundRectangle, RelativeDrawingArea.Fill());
        canvas.Render(circle, circleDrawArea);

        if (Focus.Value)
        {
            canvas.Render(focusRectangle, RelativeDrawingArea.Fill());
        }
    }

    private void CheckBox_MousePress(UIComponent sender, Events.MouseEventArgs e)
    {
        Checked.Value = !Checked;
        Focus.Value = true;
    }
}
