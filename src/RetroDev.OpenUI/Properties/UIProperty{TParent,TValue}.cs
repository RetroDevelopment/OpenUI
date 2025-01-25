﻿using System.Diagnostics;
using RetroDev.OpenUI.Components;

namespace RetroDev.OpenUI.Properties;

/// <summary>
/// Describes a property used in this UI framework. It allows for flexible binding.
/// </summary>
/// <typeparam name="TComponent">The class owning this property.</typeparam>
/// <typeparam name="TValue">The property value type.</typeparam>
/// <param name="parent">The object owning this property.</param>
/// <param name="value">The property value.</param>
/// <param name="allowedBinding">
/// The allowed <see cref="BindingType"/> (<see cref="BindingType.TwoWays"/> by default).
/// </param>
/// <remarks>
/// If <paramref name="allowedBinding"/> is <see cref="BindingType.TwoWays"/> it means that bidirectional binding is allowed, including (<see cref="BindingType.SourceToDestination"/> and <see cref="BindingType.DestinationToSource"/>).
/// </remarks>
[DebuggerDisplay("{Value}")]
public class UIProperty<TComponent, TValue>(TComponent parent, TValue value, BindingType allowedBinding = BindingType.TwoWays) : BindableProperty<TValue>(value, parent.Application, allowedBinding) where TComponent : UIComponent
{
    /// <summary>
    /// The <see cref="UIComponent"/> owning <see langword="this" /> <see cref="UIProperty{TComponent, TValue}"/>.
    /// </summary>
    public TComponent Component { get; } = parent;

    /// <summary>
    /// The property value.
    /// </summary>
    public override TValue Value
    {
        set
        {
            base.Value = value;
            Component.Application._eventSystem.InvalidateRendering(); // TODO: do not push one event for each call but just one if the rendering has not been invalidated yet
        }
        get => base.Value;
    }

    /// <summary>
    /// Creates a new property.
    /// </summary>
    /// <param name="parent">The object owning this property.</param>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    /// <param name="allowedBinding">The allowed <see cref="BindingType"/> (<see cref="BindingType.TwoWays"/> by default).</param>
    public UIProperty(TComponent parent, BindableProperty<TValue> destinationProperty, BindingType bindingType = BindingType.TwoWays, BindingType allowedBinding = BindingType.TwoWays) : this(parent, destinationProperty.Value, allowedBinding)
    {
        Bind(destinationProperty, bindingType);
    }
}
