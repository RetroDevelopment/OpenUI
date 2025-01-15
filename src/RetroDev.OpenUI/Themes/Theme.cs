﻿using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Themes;

/// <summary>
/// The base class represnting a OpenUI theme. It contains all fields needed by OpenUI default components.
/// Extend this class to include additional theme parameters.
/// </summary>
public class Theme : ThemeBase
{
    /// <summary>
    /// The default color in case a color is not defined.
    /// </summary>
    public static readonly Color DefaultColor = Color.Black;

    /// <summary>
    /// Main window background.
    /// </summary>
    public BindableProperty<Theme, Color> MainBackground { get; }

    /// <summary>
    /// Background used for containers.
    /// </summary>
    public BindableProperty<Theme, Color> PrimaryBackground { get; }

    /// <summary>
    /// Primary background color.
    /// </summary>
    public BindableProperty<Theme, Color> PrimaryColor { get; }

    /// <summary>
    /// Light version of <see cref="PrimaryColor"/> mostly used for disable components.
    /// </summary>
    public BindableProperty<Theme, Color> PrimaryColorLight { get; }

    /// <summary>
    /// Secondary background color.
    /// </summary>
    public BindableProperty<Theme, Color> SecondaryColor { get; }

    /// <summary>
    /// Light version of <see cref="SecondaryColor"/>.
    /// </summary>
    public BindableProperty<Theme, Color> SecondaryColorLight { get; }

    /// <summary>
    /// The main color for text.
    /// </summary>
    public BindableProperty<Theme, Color> TextColor { get; }

    /// <summary>
    /// Light version of <see cref="TextColor"/>, usually for disabled text.
    /// </summary>
    public BindableProperty<Theme, Color> TextColorLight { get; }

    /// <summary>
    /// Color for border surrounding UI elements.
    /// </summary>
    public BindableProperty<Theme, Color> BorderColor { get; }

    /// <summary>
    /// Creates a new theme.
    /// </summary>
    /// <param name="colors">The name - color mapping.</param>
    public Theme(Application application) : base()
    {
        List<BindingType> allowedBindings = [BindingType.SourceToDestination];

        MainBackground = new BindableProperty<Theme, Color>(this, DefaultColor, application, allowedBindings);
        PrimaryBackground = new BindableProperty<Theme, Color>(this, DefaultColor, application, allowedBindings);
        PrimaryColor = new BindableProperty<Theme, Color>(this, DefaultColor, application, allowedBindings);
        PrimaryColorLight = new BindableProperty<Theme, Color>(this, DefaultColor, application, allowedBindings);
        SecondaryColor = new BindableProperty<Theme, Color>(this, DefaultColor, application, allowedBindings);
        SecondaryColorLight = new BindableProperty<Theme, Color>(this, DefaultColor, application, allowedBindings);
        TextColor = new BindableProperty<Theme, Color>(this, DefaultColor, application, allowedBindings);
        TextColorLight = new BindableProperty<Theme, Color>(this, DefaultColor, application, allowedBindings);
        BorderColor = new BindableProperty<Theme, Color>(this, DefaultColor, application, allowedBindings);
    }
}
