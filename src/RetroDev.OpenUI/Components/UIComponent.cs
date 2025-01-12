﻿using System.Reflection.Metadata.Ecma335;
using RetroDev.OpenUI.Components.AutoSize;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Exceptions;
using RetroDev.OpenUI.Properties;
using RetroDev.OpenUI.Utils;

namespace RetroDev.OpenUI.Components;

// TODO: Use property binding (UIProperty) to bind ThemeManager properties.
// ThemeManager(Dictionary<string, Color>) {} allows specifying a theme dynamically (e.g. load from xml). Then it has some proeprties like PrimaryColor, Background, etc.
// You can create the ThemeManager in Application and pass it to each component and bind the colors. Then you can switch theme (e.g. manager.SetTheme(DarkThemeDictionary)) and colors are automatically updated.

// TODO: Same way as before, add a TextResourceManager class to manage text (e.g. with language) and create it in Application.
// They can also have dictionary/xml. Then bind these properties in your project and you will get automatic language change.

// TODO: Same way, add a AssetManager class to load assets (images, xml files, text resources, etc.). Maybe AssetManager can also load themes and text resources.

// TODO: Add LookAndFeel() {} which will do the actual rendering (create it Application). Switching look and feel allows to immediately change the shapes drawn for components.

/// <summary>
/// The abstract calss for all UI components (windows, buttons, etc.).
/// </summary>
public abstract class UIComponent
{
    private readonly List<UIComponent> _children = [];
    private UIComponent? _focusedComponent;
    private Point? _mouseDragPointAbsolute = null;
    private Point? _mouseLastDragPointAbsolute = null;
    private CachedValue<Area> _relativeDrawingArea;
    private CachedValue<Area> _absoluteDrawingArea;

    /// <summary>
    /// Mouse button press inside <see cref="this"/> window.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, MouseEventArgs> MousePress = (_, _) => { };

    /// <summary>
    /// Mouse button release inside <see cref="this"/> window.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, MouseEventArgs> MouseRelease = (_, _) => { };

    /// <summary>
    /// Mouse position changed inside <see cref="this"/> window.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, MouseEventArgs> MouseMove = (_, _) => { };

    /// <summary>
    /// Mouse dragging. This means that a left click has happend whithin <see cref="this"/> compnent <see cref="AbsoluteDrawingArea"/>
    /// and the mouse is moving while still pressed.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, MouseDragEventArgs> MouseDrag = (_, _) => { };

    /// <summary>
    /// Mouse dragging start.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, MouseEventArgs> MouseDragBegin = (_, _) => { };

    /// <summary>
    /// Mouse dragging ends.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, MouseEventArgs> MouseDragEnd = (_, _) => { };

    /// <summary>
    /// Key is pressed inside <see cref="this"/> window.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, KeyEventArgs> KeyPress = (_, _) => { };

    /// <summary>
    /// Key is released inside <see cref="this"/> window.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, KeyEventArgs> KeyRelease = (_, _) => { };

    /// <summary>
    /// Text is inserted in <see cref="this"/> window.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, TextInputEventArgs> TextInput = (_, _) => { };

    /// <summary>
    /// A frame need to be rendered. Use this event to render at the bottom of the children.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, RenderingEventArgs> RenderFrame = (_, _) => { };

    /// <summary>
    /// The children components have been rendered. Use this event handler to render on top of the children.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, RenderingEventArgs> ChildrenRendered = (_, _) => { };

    /// <summary>
    /// The application in which <see langword="this"/> <see cref="UIComponent"/> runs.
    /// </summary>
    public Application Application { get; }

    /// <summary>
    /// The parent <see cref="UIComponent"/> containing <see langword="this" /> <see cref="UIComponent"/>.
    /// </summary>
    public UIComponent? Parent { get; private set; }

    /// <summary>
    /// Gets the root component, usually a <see cref="Window"/>.
    /// </summary>
    public UIComponent Root => Parent?.Root ?? this;

    /// <summary>
    /// The component unique identifier.
    /// </summary>
    public UIProperty<UIComponent, string> ID { get; }

    /// <summary>
    /// The component top-left corner X-coordinate in pixels.
    /// </summary>
    /// <remarks>The X-coordinate is relative to the parent component rendering area.</remarks>
    public UIProperty<UIComponent, PixelUnit> X { get; }

    /// <summary>
    /// The component top-left corner Y-coordinate in pixels.
    /// </summary>
    /// <remarks>The Y-coordinate is relative to the parent component rendering area.</remarks>
    public UIProperty<UIComponent, PixelUnit> Y { get; }

    /// <summary>
    /// The component width in pixels.
    /// </summary>
    public UIProperty<UIComponent, PixelUnit> Width { get; }

    /// <summary>
    /// The component height in pixels.
    /// </summary>
    public UIProperty<UIComponent, PixelUnit> Height { get; }

    /// <summary>
    /// Whether the component is rendered or not.
    /// </summary>
    public UIProperty<UIComponent, ComponentVisibility> Visibility { get; }

    /// <summary>
    /// Specifies how to automatically specify this component width.
    /// </summary>
    public UIProperty<UIComponent, IAutoSizeStrategy> AutoWidth { get; }

    /// <summary>
    /// Specifies how to automatically specify this component height.
    /// </summary>
    public UIProperty<UIComponent, IAutoSizeStrategy> AutoHeight { get; }

    /// <summary>
    /// Whether this component can get focus.
    /// </summary>
    public UIProperty<UIComponent, bool> Focusable { get; }

    /// <summary>
    /// Whether this component has focus.
    /// </summary>
    public UIProperty<UIComponent, bool> Focus { get; }

    /// <summary>
    /// Whether this component is enabled and can receive events.
    /// </summary>
    public UIProperty<UIComponent, bool> Enabled { get; }

    /// <summary>
    /// The ideal component size which allows to correctly display the whole component.
    /// </summary>  
    public Size SizeHint => SizeHintCache.Value;

    /// <summary>
    /// The cached value for <see cref="SizeHint"/>.
    /// Classes deriving from <see cref="UIComponent"/> must implement <see cref="ComputeSizeHint"/> to compute size hints
    /// and invalidate the cache calling <see cref="CachedValue{T}.MarkDirty"/> whenever the size hint needs to recomputed.
    /// Size hints are cached because they might be time consuming.
    /// </summary>
    protected CachedValue<Size> SizeHintCache { get; }

    /// <summary>
    /// The initial value of the <see cref="Visibility"/> property.
    /// </summary>
    protected virtual ComponentVisibility DefaultVisibility => ComponentVisibility.Visible;

    /// <summary>
    /// The initial value of <see cref="AutoWidth"/>.
    /// </summary>
    protected virtual IAutoSizeStrategy DefaultAutoWidth => AutoSizeStrategy.MatchParent;

    /// <summary>
    /// The default value of <see cref="AutoHeight"/>.
    /// </summary>
    protected virtual IAutoSizeStrategy DefaultAutoHeight => AutoSizeStrategy.MatchParent;

    /// <summary>
    /// The initial value of <see cref="Focusable"/>.
    /// </summary>
    protected virtual bool DefaultIsFocusable => true;

    /// <summary>
    /// The 2D area (in pixels) where this component is rendered. The area is relative to the parent area,
    /// so [(0, 0), (100, 100)] would indicate that the component is rendered at the top-left of the paraent component,
    /// and it has size 100x100 pixels.
    /// </summary>
    public Area RelativeDrawingArea => _relativeDrawingArea.Value;

    /// <summary>
    /// The 2D area (in pixels) where this component is rendered. The area is absolute to the window and it is clipped
    /// so that it doesn't go out of the parent boundaries, if <see langword="this"/> component has a parent.
    /// So, if this component <see cref="RelativeDrawingArea"/> is [(10, 10), (100, 100)], and the parent <see cref="AbsoluteDrawingArea"/>
    /// is [(20, 20), (200, 200)], the <see cref="RelativeDrawingArea"/> of <see langword="this"/> component will be [(30, 30), (100, 100)].
    /// </summary>
    /// <remarks>
    /// The absolute drawing area is clipped so that it doesn't go out of the parent drawing area. Clipping is done by resizing
    /// the area.
    /// </remarks>
    public Area AbsoluteDrawingArea => _absoluteDrawingArea.Value;

    protected UIComponent(Application application)
    {
        Application = application;
        Application.LifeCycle.ThrowIfPropertyCannotBeSet();

        ID = new UIProperty<UIComponent, string>(this, string.Empty);
        X = new UIProperty<UIComponent, PixelUnit>(this, PixelUnit.Auto);
        Y = new UIProperty<UIComponent, PixelUnit>(this, PixelUnit.Auto);
        Width = new UIProperty<UIComponent, PixelUnit>(this, PixelUnit.Auto);
        Height = new UIProperty<UIComponent, PixelUnit>(this, PixelUnit.Auto);
        Height = new UIProperty<UIComponent, PixelUnit>(this, PixelUnit.Auto);
        Visibility = new UIProperty<UIComponent, ComponentVisibility>(this, DefaultVisibility);
        AutoWidth = new UIProperty<UIComponent, IAutoSizeStrategy>(this, DefaultAutoWidth);
        AutoHeight = new UIProperty<UIComponent, IAutoSizeStrategy>(this, DefaultAutoHeight);
        Focusable = new UIProperty<UIComponent, bool>(this, DefaultIsFocusable);
        Focus = new UIProperty<UIComponent, bool>(this, false);
        Enabled = new UIProperty<UIComponent, bool>(this, true);

        _relativeDrawingArea = new CachedValue<Area>(ComputeRelativeDrawingArea);
        _absoluteDrawingArea = new CachedValue<Area>(ComputeAbsoluteDrawingArea);

        SizeHintCache = new CachedValue<Size>(ComputeSizeHint);
        SizeHintCache.OnMarkDirty += (_, _) => MarkCachesAsDirty();

        RegisterDrawingAreaEvents();

        Focus.ValueChange += Focus_ValueChange;
        MousePress += UIComponent_MousePress;
    }

    /// <summary>
    /// Computes the ideal component size which allows to correctly display the whole component.
    /// </summary>
    /// <returns></returns>
    protected abstract Size ComputeSizeHint();

    /// <summary>
    /// Adds a child to <see langword="this" /> component.
    /// </summary>
    /// <param name="component">The child component to add.</param>
    /// <param name="index">
    /// The index where to insert the <paramref name="component"/>. If <see langword="null" /> the <paramref name="component"/> is
    /// appended to the children list.</param>
    /// <exception cref="ArgumentException">
    /// If a child with the same <see cref="ID"/> as the given <paramref name="component"/> already exists.
    /// </exception>
    protected virtual void AddChild(UIComponent component, int? index = null)
    {
        Application.LifeCycle.ThrowIfPropertyCannotBeSet();
        component.Parent?.RemoveChild(component);
        component.Parent = this;
        component.AttachEventsFromParent();
        if (index == null) _children.Add(component);
        else if (index + 1 < _children.Count) _children.Insert((int)index + 1, component);
        else _children.Add(component);
    }

    /// <summary>
    /// Gets all the child components of <see cref="this"/> comoponet.
    /// </summary>
    /// <returns>The list of child component.</returns>
    protected virtual IEnumerable<UIComponent> GetChildren() =>
        new List<UIComponent>(_children);

    /// <summary>
    /// Removes the given child <paramref name="component"/> from <see cref="this"/> component.
    /// </summary>
    /// <param name="component">The child component to remove.</param>
    /// <returns><see langword="true" /> if successfully removed, otherwise <see langword="false" />.</returns>
    protected bool RemoveChild(UIComponent component)
    {
        Application.LifeCycle.ThrowIfPropertyCannotBeSet();
        component.DetachEventsFromParent();
        if (component.Parent == this) component.Parent = null;
        return _children.Remove(component);
    }

    protected void OnMousePress(MouseEventArgs e)
    {
        MousePress?.Invoke(this, e);
    }

    protected void OnMouseRelease(MouseEventArgs e)
    {
        MouseRelease?.Invoke(this, e);
    }

    protected void OnMouseMove(MouseEventArgs e)
    {
        MouseMove?.Invoke(this, e);
    }

    protected void OnKeyPress(KeyEventArgs e)
    {
        KeyPress?.Invoke(this, e);
    }

    protected void OnKeyRelease(KeyEventArgs e)
    {
        KeyRelease?.Invoke(this, e);
    }

    protected void OnTextInput(TextInputEventArgs e)
    {
        TextInput?.Invoke(this, e);
    }

    /// <summary>
    /// Validates <see langword="this" /> <see cref="UIComponent"/> and throw <see cref="UIPropertyValidationException"/> if one or more
    /// properties are in an invalid state.
    /// Call base.Validate() at the bottom of your implementation if overriding this method.
    /// </summary>
    /// <remarks>
    /// This method may be called at any time to perform validation, but it is automatically called within the <see cref="IEventSystem.BeforeRender"/> event,
    /// because a <see cref="UIComponent"/> needs to be validated before rendering a frame, which requires accessing component properties.
    /// Note that validation must should be performed when a property value change (unless strictly necessary) because it makes it more complicated to
    /// update property values. For example if a <see cref="UIComponent"/> has properties A and B, and validation checks A &lt; B, if A = 10 and B = 20,
    /// if updating A and B to 30 and 40 respectively, the code below would crash
    /// A.Value = 30; // would throw exception because A.Value &gt; B.Value (30 &gt; 20).
    /// B.Value = 40;
    /// But if validation is not performed, the framework would still allow detecting errors before frame update.
    /// </remarks>
    public void Validate()
    {
        UIComponentValidateImplementation();
        ValidateImplementation();
        _children.ForEach(child => child.Validate());
    }

    /// <summary>
    /// Re-computes the position and size of the children of <see langword="this" /> <see cref="UIComponent"/> if necessary.
    /// </summary>
    /// <remarks>
    /// This method may be called at any time to update the internal children state, but it is automatically called within the <see cref="IEventSystem.BeforeRender"/> event,
    /// for performance reason. The method is used mostly for layouts and, if called every time a property changes (<see cref="Width" />, <see cref="Height"/>, etc.)
    /// it could degrade performance.
    /// </remarks>
    public void RepositionChildren()
    {
        if (Visibility.Value == ComponentVisibility.Collapsed) return;
        RepositionChildrenImplementation();
        _children.ForEach(child => child.RepositionChildren());
    }

    /// <summary>
    /// The validation logic called by <see cref="Validate"/> overridden by subclasses of <see cref="UIComponent"/>.
    /// </summary>
    protected virtual void ValidateImplementation()
    {
    }

    /// <summary>
    /// The children repositioning logic called by <see cref="RepositionChildren"/> overridden by subclasses of <see cref="UIComponent"/>.
    /// </summary>
    protected virtual void RepositionChildrenImplementation() { }

    protected void OnRenderFrame(RenderingEventArgs renderingArgs)
    {
        Application.LifeCycle.ThrowIfNotOnRenderingPhase();

        if (Visibility.Value == Components.ComponentVisibility.Visible)
        {
            renderingArgs.Canvas.ContainerAbsoluteDrawingArea = AbsoluteDrawingArea;
            RenderFrame.Invoke(this, renderingArgs);
            _children.ForEach((child) => child.OnRenderFrame(renderingArgs));

            renderingArgs.Canvas.ContainerAbsoluteDrawingArea = AbsoluteDrawingArea;
            ChildrenRendered?.Invoke(this, renderingArgs);
        }
    }

    private void UIComponentValidateImplementation()
    {
        if (!Width.Value.IsAuto && Width.Value < 0.0f) throw new UIPropertyValidationException($"Width must be greater or equal to zero, found {Width.Value}", this);
        if (!Height.Value.IsAuto && Height.Value < 0.0f) throw new UIPropertyValidationException($"Height must be greater or equal to zero, found {Height.Value}", this);
        if (!Focusable.Value && Focus.Value) throw new UIPropertyValidationException("Cannot focus a component that is not focusable", this);
        if (!Enabled.Value && Focus.Value) throw new UIPropertyValidationException("Cannot focus a component that is not enabled");
    }

    private void UIComponent_MousePress(UIComponent sender, MouseEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            MouseDragBegin.Invoke(this, e);
            _mouseDragPointAbsolute = e.AbsoluteLocation;
            _mouseLastDragPointAbsolute = e.AbsoluteLocation;
        }
    }

    private void AttachEventsFromParent()
    {
        if (Parent == null) return;
        Parent.MousePress += _parent_MousePress;
        Parent.MouseRelease += _parent_MouseRelease;
        Parent.MouseMove += _parent_MouseMove;
        Parent.KeyPress += _parent_KeyPress;
        Parent.KeyRelease += _parent_KeyRelease;
        Parent.TextInput += _parent_TextInput;
    }

    private void DetachEventsFromParent()
    {
        if (Parent == null) return;
        Parent.MousePress -= _parent_MousePress;
        Parent.MouseRelease -= _parent_MouseRelease;
        Parent.MouseMove -= _parent_MouseMove;
        Parent.KeyPress -= _parent_KeyPress;
        Parent.KeyRelease -= _parent_KeyRelease;
        Parent.TextInput -= _parent_TextInput;
    }

    private void _parent_MouseMove(UIComponent sender, MouseEventArgs mouseEventArgs)
    {
        if (mouseEventArgs.AbsoluteLocation.IsWithin(AbsoluteDrawingArea) && Visibility.Value == Components.ComponentVisibility.Visible && Enabled)
        {
            MouseMove.Invoke(this, new MouseEventArgs(mouseEventArgs.AbsoluteLocation,
                                                      mouseEventArgs.AbsoluteLocation - AbsoluteDrawingArea.TopLeft,
                                                      mouseEventArgs.Button));
        }

        if (_mouseDragPointAbsolute != null && _mouseLastDragPointAbsolute != null)
        {
            var offset = mouseEventArgs.AbsoluteLocation - _mouseLastDragPointAbsolute;
            _mouseLastDragPointAbsolute = mouseEventArgs.AbsoluteLocation;
            MouseDrag.Invoke(this, new MouseDragEventArgs(_mouseDragPointAbsolute, _mouseLastDragPointAbsolute, offset));
        }
    }

    private void _parent_MouseRelease(UIComponent sender, MouseEventArgs mouseEventArgs)
    {
        var e = new MouseEventArgs(mouseEventArgs.AbsoluteLocation,
                                   mouseEventArgs.AbsoluteLocation - AbsoluteDrawingArea.TopLeft,
                                   mouseEventArgs.Button);

        if (mouseEventArgs.AbsoluteLocation.IsWithin(AbsoluteDrawingArea) && Visibility.Value == Components.ComponentVisibility.Visible && Enabled)
        {
            MouseRelease.Invoke(this, e);
        }

        if (_mouseDragPointAbsolute != null)
        {
            MouseDragEnd.Invoke(this, e);
        }

        _mouseDragPointAbsolute = null;
        _mouseLastDragPointAbsolute = null;
    }

    private void _parent_MousePress(UIComponent sender, MouseEventArgs mouseEventArgs)
    {
        if (mouseEventArgs.AbsoluteLocation.IsWithin(AbsoluteDrawingArea) && Visibility.Value == Components.ComponentVisibility.Visible && Enabled)
        {
            MousePress.Invoke(this, new MouseEventArgs(mouseEventArgs.AbsoluteLocation,
                                                       mouseEventArgs.AbsoluteLocation - AbsoluteDrawingArea.TopLeft,
                                                       mouseEventArgs.Button));
        }
    }

    private void _parent_KeyPress(UIComponent sender, KeyEventArgs keyEventArgs)
    {
        if (Visibility.Value == Components.ComponentVisibility.Visible && (Focus || !Focusable))
        {
            KeyPress.Invoke(this, new KeyEventArgs(keyEventArgs.Button));
        }
    }

    private void _parent_KeyRelease(UIComponent sender, KeyEventArgs keyEventArgs)
    {
        if (Visibility.Value == ComponentVisibility.Visible && (Focus || !Focusable))
        {
            KeyRelease.Invoke(this, new KeyEventArgs(keyEventArgs.Button));
        }
    }

    private void _parent_TextInput(UIComponent sender, TextInputEventArgs textInputEventArgs)
    {
        if (Visibility.Value == ComponentVisibility.Visible && (Focus || !Focusable))
        {
            TextInput.Invoke(this, new TextInputEventArgs(textInputEventArgs.Text));
        }
    }

    private void Focus_ValueChange(UIComponent sender, ValueChangeEventArgs<bool> e)
    {
        RequestFocusFor(this);
    }

    // Ensure that only one child component has focus.
    private void RequestFocusFor(UIComponent component)
    {
        // Only the root component can manage focus, because only one object can be focusable at a time in a window.
        // TODO: When implementing focus groups, just change the logic here to not delegate this to the parent.
        if (Parent != null)
        {
            Parent.RequestFocusFor(component);
            return;
        }

        if (_focusedComponent != null)
        {
            _focusedComponent.Focus.Value = false;
        }

        _focusedComponent = component;
    }

    private void RegisterDrawingAreaEvents()
    {
        X.ValueChange += (_, _) => MarkCachesAsDirty();
        Y.ValueChange += (_, _) => MarkCachesAsDirty();
        Width.ValueChange += (_, _) => MarkCachesAsDirty();
        Height.ValueChange += (_, _) => MarkCachesAsDirty();
        AutoWidth.ValueChange += (_, _) => MarkCachesAsDirty();
        AutoHeight.ValueChange += (_, _) => MarkCachesAsDirty();
        Visibility.ValueChange += (_, _) => MarkCachesAsDirty();
    }

    private void MarkCachesAsDirty()
    {
        _relativeDrawingArea.MarkDirty();
        _absoluteDrawingArea.MarkDirty();
        _children.ForEach(c => c.MarkCachesAsDirty());
    }

    private Area ComputeRelativeDrawingArea()
    {
        var (x, width) = AutoWidth.Value.ComputeHorizontalArea(this);
        var (y, height) = AutoHeight.Value.ComputeVerticalArea(this);

        var topLeft = new Point(x, y);
        var size = Visibility.Value != ComponentVisibility.Collapsed ? new Size(width, height) : Size.Zero;

        return new Area(topLeft, size);
    }

    private Area ComputeAbsoluteDrawingArea() => RelativeDrawingArea.ToAbsolute(Parent?.AbsoluteDrawingArea)
                                                                    .Clip(Parent?.AbsoluteDrawingArea);
}
