// *************************************************************************************
// SCICHART® Copyright SciChart Ltd. 2011-2026. All rights reserved.
//
// Web:     http://www.scichart.com
// Support: support@scichart.com
// Sales:   sales@scichart.com
//
// VirtualizingWrapPanel.cs is part of the SCICHART® Examples. Permission is hereby granted
// to modify, create derivative works, distribute and publish any part of this source
// code whether for commercial, private or personal use.
//
// The SCICHART® examples are distributed in the hope that they will be useful, but
// without any warranty. It is provided "AS IS" without warranty of any kind, either
// expressed or implied.
// *************************************************************************************
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SciChart.Examples.Demo.Controls
{
    /// <summary>
    /// Implemented by items which occupy the whole row of the <see cref="VirtualizingWrapPanel"/>.
    /// </summary>
    public interface IVirtualizingWrapPanelItem
    {
        bool IsFullWidth { get; }
    }

    /// <summary>
    /// Arranges items into rows the way the <see cref="WrapPanel"/> does, but generates containers only for
    /// the items which are within the viewport. Item sizes are declared by <see cref="ItemWidth"/>,
    /// <see cref="ItemHeight"/> and <see cref="FullWidthItemHeight"/>, so the layout of the whole extent is
    /// known without realizing a single item, which is what makes the virtualization possible.
    /// Items which implement <see cref="IVirtualizingWrapPanelItem"/> and report
    /// <see cref="IVirtualizingWrapPanelItem.IsFullWidth"/> occupy a row of their own.
    /// The panel honours <see cref="VirtualizingPanel.IsVirtualizingProperty"/> and
    /// <see cref="VirtualizingPanel.VirtualizationModeProperty"/> of its owner, and realizes
    /// <see cref="CacheLengthRatio"/> viewports above and below the visible one.
    /// </summary>
    public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
    {
        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register
            (nameof(ItemWidth), typeof(double), typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(100d, FrameworkPropertyMetadataOptions.AffectsMeasure, OnItemSizeChanged));

        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register
            (nameof(ItemHeight), typeof(double), typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(100d, FrameworkPropertyMetadataOptions.AffectsMeasure, OnItemSizeChanged));

        public static readonly DependencyProperty FullWidthItemHeightProperty = DependencyProperty.Register
            (nameof(FullWidthItemHeight), typeof(double), typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(36d, FrameworkPropertyMetadataOptions.AffectsMeasure, OnItemSizeChanged));

        public static readonly DependencyProperty CacheLengthRatioProperty = DependencyProperty.Register
            (nameof(CacheLengthRatio), typeof(double), typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(0.5d, FrameworkPropertyMetadataOptions.AffectsMeasure));

        // Scrolling steps of the panel as an IScrollInfo. The mouse wheel step is close to the one which
        // the ScrollViewer of the Home view used before this panel took the scrolling over.
        private const double ScrollLineDelta = 48d;
        private const double MouseWheelDelta = 240d;

        // Bounds of every item in extent coordinates, index aligned with the items of the owner
        private readonly List<Rect> _itemBounds = new List<Rect>();

        private Size _extent;
        private Size _viewport;

        private double _verticalOffset;
        private double _lastBoundsWidth = double.NaN;
        private bool _areItemBoundsDirty = true;
        
        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        public double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public double FullWidthItemHeight
        {
            get => (double)GetValue(FullWidthItemHeightProperty);
            set => SetValue(FullWidthItemHeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the size of the area above and below the viewport, in viewports, which is realized
        /// together with it. The items of that area are ready by the time the user scrolls them into view,
        /// at the cost of keeping their containers, and whatever their item template loads, in memory.
        /// </summary>
        public double CacheLengthRatio
        {
            get => (double)GetValue(CacheLengthRatioProperty);
            set => SetValue(CacheLengthRatioProperty, value);
        }

        /// <summary>
        /// Gets the bounds of the item with the specified index within the scrollable extent of the panel,
        /// or <see cref="Rect.Empty"/> when the index is out of range. The bounds are known for every item,
        /// including the ones which are not realized.
        /// </summary>
        public Rect GetItemBounds(int index)
        {
            // The bounds are normally rebuilt by the measure pass, but they are read from the outside too, e.g.
            // while handling ScrollChanged. Rebuilding them here as well keeps such a caller from getting the
            // bounds which belong to the items the panel held before the last change to them.
            if (_areItemBoundsDirty && !double.IsNaN(_lastBoundsWidth))
            {
                UpdateItemBounds(_lastBoundsWidth, ItemsControl.GetItemsOwner(this)?.Items);
            }

            return index >= 0 && index < _itemBounds.Count ? _itemBounds[index] : Rect.Empty;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // Accessing InternalChildren initializes the item container generator
            var children = InternalChildren;
            var generator = ItemContainerGenerator;
            var owner = ItemsControl.GetItemsOwner(this);
            var items = owner?.Items;

            // The width is not scrollable, so the panel wraps the items within the width it is given
            var width = double.IsInfinity(availableSize.Width) ? _viewport.Width : availableSize.Width;

            UpdateItemBounds(width, items);

            var viewport = new Size(width, double.IsInfinity(availableSize.Height) ? _extent.Height : availableSize.Height);
            var offset = CoerceVerticalOffset(_verticalOffset, viewport);

            if (_viewport != viewport || _verticalOffset != offset)
            {
                _viewport = viewport;
                _verticalOffset = offset;

                ScrollOwner?.InvalidateScrollInfo();
            }

            GetItemRange(owner, out var firstItem, out var lastItem);

            if (firstItem >= 0)
            {
                var startPosition = generator.GeneratorPositionFromIndex(firstItem);

                // The position of the child which corresponds to the first item of the range. When the item is
                // not realized yet, the generator position points at the previous realized one, hence the offset.
                var childIndex = startPosition.Offset == 0 ? startPosition.Index : startPosition.Index + 1;

                using (generator.StartAt(startPosition, GeneratorDirection.Forward, true))
                {
                    for (int i = firstItem; i <= lastItem; i++, childIndex++)
                    {
                        if (!(generator.GenerateNext(out var isNewlyRealized) is UIElement child)) break;

                        var cellSize = _itemBounds[i].Size;

                        // A recycled container is not reported as newly realized, but it has been taken out of
                        // the visual tree, so it has to be inserted back and prepared for its new item
                        if (isNewlyRealized || !children.Contains(child))
                        {
                            if (childIndex >= children.Count)
                            {
                                AddInternalChild(child);
                            }
                            else
                            {
                                InsertInternalChild(childIndex, child);
                            }

                            generator.PrepareItemContainer(child);
                        }

                        child.Measure(cellSize);
                    }
                }
            }

            VirtualizeItemsOutsideRange(owner, firstItem, lastItem);

            return new Size(width, Math.Min(viewport.Height, _extent.Height));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var children = InternalChildren;
            var generator = ItemContainerGenerator;

            for (int i = 0; i < children.Count; i++)
            {
                var itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));
                if (itemIndex < 0 || itemIndex >= _itemBounds.Count) continue;

                var bounds = _itemBounds[itemIndex];
                children[i].Arrange(new Rect(bounds.X, bounds.Y - _verticalOffset, bounds.Width, bounds.Height));
            }

            return finalSize;
        }

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            base.OnItemsChanged(sender, args);

            // Any change to the items invalidates the bounds: even a replacement which keeps their number
            // can turn an item into a full width one and reflow every row below it
            _areItemBoundsDirty = true;

            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    break;

                case NotifyCollectionChangedAction.Move:
                    RemoveInternalChildRange(args.OldPosition.Index, args.ItemUICount);
                    break;
            }

            InvalidateMeasure();
        }

        protected override void OnClearChildren()
        {
            base.OnClearChildren();

            RemoveInternalChildRange(0, InternalChildren.Count);

            _verticalOffset = 0d;
            _itemBounds.Clear();
            _areItemBoundsDirty = true;

            InvalidateMeasure();
        }

        // Called by the owner to scroll to an item which is not realized, e.g. by ListBox.ScrollIntoView
        protected override void BringIndexIntoView(int index)
        {
            var bounds = GetItemBounds(index);
            if (!bounds.IsEmpty)
            {
                MakeVerticallyVisible(bounds);
            }
        }

        // Lays the items out into rows and updates the extent, without realizing any of the items
        private void UpdateItemBounds(double availableWidth, ItemCollection items)
        {
            if (!_areItemBoundsDirty && _lastBoundsWidth == availableWidth) return;

            _areItemBoundsDirty = false;
            _lastBoundsWidth = availableWidth;
            _itemBounds.Clear();

            var itemWidth = ItemWidth;
            var itemHeight = ItemHeight;
            var fullWidthItemHeight = FullWidthItemHeight;

            double x = 0d, y = 0d, rowHeight = 0d;

            for (int i = 0; items != null && i < items.Count; i++)
            {
                var isFullWidth = items[i] is IVirtualizingWrapPanelItem item && item.IsFullWidth;
                var size = isFullWidth
                    ? new Size(Math.Max(0d, availableWidth), fullWidthItemHeight)
                    : new Size(itemWidth, itemHeight);

                // A full width item always starts a new row, the other ones start a new row once they do not fit
                if (x > 0d && (isFullWidth || x + size.Width > availableWidth))
                {
                    x = 0d;
                    y += rowHeight;
                    rowHeight = 0d;
                }

                _itemBounds.Add(new Rect(x, y, size.Width, size.Height));

                x += size.Width;
                rowHeight = Math.Max(rowHeight, size.Height);

                if (isFullWidth)
                {
                    x = 0d;
                    y += rowHeight;
                    rowHeight = 0d;
                }
            }

            var extent = new Size(Math.Max(0d, availableWidth), y + rowHeight);
            if (_extent != extent)
            {
                _extent = extent;

                ScrollOwner?.InvalidateScrollInfo();
            }
        }

        // The range of items which have to be realized: the ones within the viewport, plus the ones within the
        // cache area above and below it. Everything outside of the range is virtualized.
        private void GetItemRange(ItemsControl owner, out int firstItem, out int lastItem)
        {
            if (owner != null && !GetIsVirtualizing(owner))
            {
                firstItem = _itemBounds.Count > 0 ? 0 : -1;
                lastItem = _itemBounds.Count - 1;

                return;
            }

            var cacheLength = Math.Max(0d, CacheLengthRatio) * _viewport.Height;
            var top = _verticalOffset - cacheLength;
            var bottom = _verticalOffset + _viewport.Height + cacheLength;

            firstItem = -1;
            lastItem = -1;

            for (int i = 0; i < _itemBounds.Count; i++)
            {
                var bounds = _itemBounds[i];

                if (bounds.Bottom > top && bounds.Top < bottom)
                {
                    if (firstItem < 0)
                    {
                        firstItem = i;
                    }
                    lastItem = i;
                }
                else if (firstItem >= 0)
                {
                    // The items are laid out top to bottom, so there is nothing within the range below this one
                    break;
                }
            }
        }

        // Takes the containers of the items which are outside of the range out of the visual tree. This is the
        // virtualization itself: containers, and everything their item template loads, exist for that range only.
        // In Recycling mode the containers are handed back to the generator, which reuses them for the items
        // scrolled into view instead of building their item template from scratch.
        private void VirtualizeItemsOutsideRange(ItemsControl owner, int firstItem, int lastItem)
        {
            var children = InternalChildren;
            var generator = ItemContainerGenerator;
            var isRecycling = owner != null && GetVirtualizationMode(owner) == VirtualizationMode.Recycling;

            for (int i = children.Count - 1; i >= 0; i--)
            {
                var childPosition = new GeneratorPosition(i, 0);
                var itemIndex = generator.IndexFromGeneratorPosition(childPosition);

                if (itemIndex < firstItem || itemIndex > lastItem)
                {
                    if (isRecycling)
                    {
                        ((IRecyclingItemContainerGenerator)generator).Recycle(childPosition, 1);
                    }
                    else
                    {
                        generator.Remove(childPosition, 1);
                    }

                    RemoveInternalChildRange(i, 1);
                }
            }
        }

        private double CoerceVerticalOffset(double offset, Size viewport)
        {
            return Math.Max(0d, Math.Min(offset, _extent.Height - viewport.Height));
        }

        private static void OnItemSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VirtualizingWrapPanel)d)._areItemBoundsDirty = true;
        }

        private void MakeVerticallyVisible(Rect bounds)
        {
            if (bounds.Top < _verticalOffset)
            {
                SetVerticalOffset(bounds.Top);
            }
            else if (bounds.Bottom > _verticalOffset + _viewport.Height)
            {
                SetVerticalOffset(bounds.Bottom - _viewport.Height);
            }
        }

        #region IScrollInfo

        public bool CanHorizontallyScroll { get; set; }

        public bool CanVerticallyScroll { get; set; }

        public double ExtentWidth => _extent.Width;

        public double ExtentHeight => _extent.Height;

        public double ViewportWidth => _viewport.Width;

        public double ViewportHeight => _viewport.Height;

        public double HorizontalOffset => 0d;

        public double VerticalOffset => _verticalOffset;

        public ScrollViewer ScrollOwner { get; set; }

        public void LineUp() => SetVerticalOffset(_verticalOffset - ScrollLineDelta);

        public void LineDown() => SetVerticalOffset(_verticalOffset + ScrollLineDelta);

        public void PageUp() => SetVerticalOffset(_verticalOffset - _viewport.Height);

        public void PageDown() => SetVerticalOffset(_verticalOffset + _viewport.Height);

        public void MouseWheelUp() => SetVerticalOffset(_verticalOffset - MouseWheelDelta);

        public void MouseWheelDown() => SetVerticalOffset(_verticalOffset + MouseWheelDelta);

        // The panel wraps the items within the available width, so there is nothing to scroll horizontally
        public void LineLeft() { }

        public void LineRight() { }

        public void PageLeft() { }

        public void PageRight() { }

        public void MouseWheelLeft() { }

        public void MouseWheelRight() { }

        public void SetHorizontalOffset(double offset) { }

        public void SetVerticalOffset(double offset)
        {
            var newOffset = CoerceVerticalOffset(offset, _viewport);
            if (newOffset == _verticalOffset) return;

            _verticalOffset = newOffset;

            ScrollOwner?.InvalidateScrollInfo();
            InvalidateMeasure();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            var children = InternalChildren;

            // The visual is usually a part of the item template, so its container has to be found first
            var child = visual as UIElement;
            while (child != null && !children.Contains(child))
            {
                child = VisualTreeHelper.GetParent(child) as UIElement;
            }

            if (child == null) return rectangle;

            var itemIndex = ItemContainerGenerator.IndexFromGeneratorPosition(new GeneratorPosition(children.IndexOf(child), 0));

            var bounds = GetItemBounds(itemIndex);
            if (bounds.IsEmpty) return rectangle;

            MakeVerticallyVisible(bounds);

            return new Rect(bounds.X, bounds.Y - _verticalOffset, bounds.Width, bounds.Height);
        }

        #endregion
    }
}
