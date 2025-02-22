﻿using RetroDev.OpenUI.Components.AutoSize;
using RetroDev.OpenUI.Components.Simple;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Components.Containers;

// TODO: optimize. There are too many Linq queries. Cache them.
// TODO: maybe create a Mapper class mapping TreeNode into ListBox elements.

/// <summary>
/// A hierarchical list of items.
/// </summary>
/// <remarks>
/// Although list boxes are typically used to list text in order to allow selecting one or more options,
/// the <see cref="TreeBox"/> class allows to list not only text but any <see cref="UIComponent"/>.
/// </remarks>
public class TreeBox : Container
{
    private static readonly PixelUnit FoldUnfoldButtonSize = 20;
    private static readonly PixelUnit IndentationSize = 20;

    private readonly ListBox _listBox;
    private readonly List<TreeNode> _nodes = [];

    protected override Size ComputeSizeHint() => new(100, 100);

    public override IEnumerable<UIComponent> Children => _listBox.Children.Cast<GridLayout>().Select(c => c.Children.ElementAt(2));

    public UIProperty<TreeBox, TreeNode?> SelectedNode { get; }

    public TreeBox(Application application) : base(application)
    {
        _listBox = new ListBox(application);
        _listBox.AutoWidth.Value = AutoSizeStrategy.MatchParent;
        _listBox.AutoHeight.Value = AutoSizeStrategy.MatchParent;
        AddChild(_listBox);

        SelectedNode = new UIProperty<TreeBox, TreeNode?>(this, null);
        // TODO SelectedNode can be bound to _listBox.SelectedItem and converters!
        SelectedNode.ValueChange += SelectedNode_ValueChange;
        _listBox.SelectedIndex.ValueChange += SelectedIndex_ValueChange;

        RepositionChildren += TreeBox_RepositionChildren;
    }

    public void AddTreeNode(TreeNode component)
    {
        AddTreeNode(component, null);
    }

    internal void AddTreeNode(TreeNode component, TreeNode? after = null)
    {
        component._root = this;
        var gridLayout = new GridLayout(Application);
        // Later on in resize children we will set appropriate sizes
        gridLayout.Width.Value = float.PositiveInfinity;
        gridLayout.Height.Value = float.PositiveInfinity;
        var foldUnfoldButton = new Button(Application);
        foldUnfoldButton.Text.Value = "*";
        foldUnfoldButton.Width.Value = FoldUnfoldButtonSize;
        foldUnfoldButton.Height.Value = FoldUnfoldButtonSize;
        foldUnfoldButton.Action += (_, _) =>
        {
            component.Collapsed.Value = !component.Collapsed;
        };

        var panel = new Panel(Application);

        gridLayout.Rows.Value = 1;
        gridLayout.Columns.Value = 3u;
        gridLayout.ColumnSizes.Value = $"{IndentationSize * component.Indentation}px,{FoldUnfoldButtonSize}px,*";
        gridLayout.AddComponent(panel);
        gridLayout.AddComponent(foldUnfoldButton);
        gridLayout.AddComponent(component.Content.Value);

        gridLayout.AutoWidth.Value = AutoSizeStrategy.WrapComponentLeftTop;
        gridLayout.AutoHeight.Value = AutoSizeStrategy.WrapComponentLeftTop;

        UIComponent? afterComponent;

        if (after != null)
        {
            afterComponent = after.Content.Value;
            foreach (var child in after.GetRecursiveChildren())
            {
                if (Children.Any(c => c == child.Content.Value))
                {
                    afterComponent = child.Content.Value;
                }
            }

            var afterGridLayout = _listBox.Children
                                          .Cast<GridLayout>()
                                          .First(c => c.Children.ElementAt(2) == afterComponent);
            _listBox.AddComponent(gridLayout, afterGridLayout);
            var index = _listBox.Children.Cast<GridLayout>().ToList().FindIndex(c => c == afterGridLayout);
            if (index + 1 < _listBox.Children.Count()) _nodes.Insert(index + 1, component);
            else _nodes.Add(component);
        }
        else
        {
            _listBox.AddComponent(gridLayout);
            _nodes.Add(component);
        }

        foreach (var child in component._children)
        {
            AddTreeNode(child, component);
        }
    }

    public void RemoveTreeNode(TreeNode node)
    {
        if (node.Parent != null)
        {
            node.Parent.RemoveChild(node);
        }
        else
        {
            InternalRemoveTreeNode(node);
        }
    }


    public void InternalRemoveTreeNode(TreeNode node)
    {
        var elementIndex = Children.ToList().FindIndex(c => c == node.Content.Value);
        if (elementIndex < 0) throw new ArgumentException("Element not found in tree box");
        _listBox.RemoveComponent((uint)elementIndex);
        _nodes.Remove(node);

        var childrenCopy = new List<TreeNode>(node._children);
        foreach (var child in childrenCopy)
        {
            node.RemoveChild(child);
        }
    }

    public void Clear() =>
        _nodes.Where(n => n.Parent == null).ToList().ForEach(RemoveTreeNode);

    private void SelectedNode_ValueChange(TreeBox sender, ValueChangeEventArgs<TreeNode?> e)
    {
        if (e.CurrentValue == null)
        {
            _listBox.SelectedIndex.Value = null;
        }
        else
        {
            var selectedIndex = Children.ToList().FindIndex(c => c == e.CurrentValue.Content.Value);
            if (selectedIndex < 0) throw new ArgumentException("Selected node not found");
            _listBox.SelectedIndex.Value = (uint)selectedIndex;
        }
    }

    private void SelectedIndex_ValueChange(ListBox sender, ValueChangeEventArgs<uint?> e)
    {
        if (e.CurrentValue == null)
        {
            SelectedNode.Value = null;
        }
        else
        {
            var selectedNode = _nodes[(int)e.CurrentValue.Value];
            SelectedNode.Value = selectedNode;
        }
    }


    private void TreeBox_RepositionChildren(UIComponent sender, EventArgs e)
    {
        foreach (var gridLayout in _listBox.Children.Cast<GridLayout>())
        {
            // TODO: better way of inferring indentation space.
            var indentationSpace = int.Parse(gridLayout.ColumnSizes.Value.Split(',')[0].TrimEnd("px".ToCharArray()));
            var componentSize = gridLayout.Children.ElementAt(2).RelativeDrawingArea.Size;
            gridLayout.Width.Value = indentationSpace + FoldUnfoldButtonSize + componentSize.Width;
            gridLayout.Height.Value = Math.Max(FoldUnfoldButtonSize, componentSize.Height.Value);
        }

        UpdateCollapseState();
    }

    private void UpdateCollapseState()
    {
        foreach (var node in _nodes)
        {
            var gridLayout = _listBox.Children.Cast<GridLayout>().ToList().Find(c => c.Children.ElementAt(2) == node.Content.Value) ?? throw new ArgumentException("Cannot find node to expand in tree box");
            var collapseButton = (Button)gridLayout.Children.ElementAt(1);
            if (node._children.Count == 0)
            {
                collapseButton.Visibility.Value = ComponentVisibility.Hidden;
            }
            else if (node.Collapsed)
            {
                collapseButton.Visibility.Value = ComponentVisibility.Visible;
                collapseButton.Text.Value = "+";
            }
            else
            {
                collapseButton.Visibility.Value = ComponentVisibility.Visible;
                collapseButton.Text.Value = "-";
            }

            if (node.ShouldDisplay)
            {
                gridLayout.Visibility.Value = ComponentVisibility.Visible;
            }
            else
            {
                gridLayout.Visibility.Value = ComponentVisibility.Collapsed;
            }
        }
    }
}
