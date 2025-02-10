﻿using RetroDev.OpenUI.Components.Core.AutoArea;

namespace RetroDev.OpenUI.Components.Base;

/// <summary>
/// Abstract class for all containers, which lay out multiple <see cref="UIComponent"/> instances.
/// </summary>
/// <param name="application">The application owning this component.</param>
/// <param name="visibility">Whether the component is rendered or not.</param>
/// <param name="isFocusable">Whether the component can get focus.</param>
/// <param name="autoWidth">How to automatically determine this component width.</param>
/// <param name="autoHeight">How to automatically determine this component height.</param>
/// <param name="horizontalAlignment">The component horizontal alignment (relative to its <see cref="Parent"/>).</param>
/// <param name="verticalAlignment">The component vertical alignment (relative to its <see cref="Parent"/>).</param>
public abstract class UIContainer(Application application,
                                UIComponent.ComponentVisibility visibility = UIComponent.ComponentVisibility.Visible,
                                bool isFocusable = false,
                                IAutoSize? autoWidth = null,
                                IAutoSize? autoHeight = null,
                                IHorizontalAlignment? horizontalAlignment = null,
                                IVerticalAlignment? verticalAlignment = null) : UIWidget(application, visibility, isFocusable, autoWidth, autoHeight, horizontalAlignment, verticalAlignment), IGenericContainer
{
    public abstract IEnumerable<UIWidget> Children { get; }

    /// <summary>
    /// Gets the child component with <see cref="ID"/> equal to the given <paramref name="id"/>.
    /// </summary>
    /// <typeparam name="TComponent">The comnponent type.</typeparam>
    /// <returns>The component.</returns>
    /// <exception cref="ArgumentException">If the component does not exist.</exception>
    /// <exception cref="InvalidCastException">If the component was found but with a type not assignable to <typeparamref name="TComponent"/>.</exception>
    public TComponent GetComponent<TComponent>(string id) where TComponent : UIWidget
    {
        var children = Children.Where(c => c.ID.Value == id);
        if (!children.Any()) throw new ArgumentException($"Child with ID {id} not found in component with id {ID.Value}");
        return (TComponent)children.First();
    }
}
