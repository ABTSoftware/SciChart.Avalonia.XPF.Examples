// *************************************************************************************
// SCICHART® Copyright SciChart Ltd. 2011-2026. All rights reserved.
//
// Web:     http://www.scichart.com
// Support: support@scichart.com
// Sales:   sales@scichart.com
//
// ScrollNavigatedItemsControl.cs is part of the SCICHART® Examples. Permission is hereby granted
// to modify, create derivative works, distribute and publish any part of this source
// code whether for commercial, private or personal use.
//
// The SCICHART® examples are distributed in the hope that they will be useful, but
// without any warranty. It is provided "AS IS" without warranty of any kind, either
// expressed or implied.
// *************************************************************************************
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using SciChart.Examples.Demo.ViewModels;

namespace SciChart.Examples.Demo.Controls
{
    public class ScrollNavigatedItemsControl : ItemsControl
    {
        public static readonly DependencyProperty NavigatedItemProperty = DependencyProperty.Register
            (nameof(NavigatedItem), typeof(TileViewModel), typeof(ScrollNavigatedItemsControl),
            new PropertyMetadata(null, OnNavigatedItemChanged));

        private ScrollViewer _scrollViewer;
        private VirtualizingWrapPanel _itemsPanel;

        // Reentrancy guard for the scroll<->NavigatedItem feedback loop
        // Null  Idle; either handler may proceed
        // True  ScrollChanged fired from user input; suppresses the programmatic ScrollToVerticalOffset
        // False ScrollToVerticalOffset was called programmatically; suppresses the ScrollChanged handler
        private bool? _isScrollOriginalSource;

        public TileViewModel NavigatedItem
        {
            get => (TileViewModel)GetValue(NavigatedItemProperty);
            set => SetValue(NavigatedItemProperty, value);
        }

        public ScrollNavigatedItemsControl()
        {
            Loaded += (s,e) =>
            {
                EnsureTemplateParts();

                var currentValue = GetValue(NavigatedItemProperty);
                if (currentValue != null && currentValue != DependencyProperty.UnsetValue)
                {
                    // SC-9515: scroll to current NavigatedItem in case it was set before Loaded
                    ScrollToNavigatedItem(NavigatedItem);
                }
                else if (ItemContainerGenerator.Items?.FirstOrDefault() is TileViewModel item)
                {
                    SetCurrentValue(NavigatedItemProperty, item);
                }
            };
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            EnsureTemplateParts();

            try
            {
                if (!_isScrollOriginalSource.HasValue)
                {
                    _isScrollOriginalSource = true;

                    if (GetFirstItemInView() is TileViewModel item)
                    {
                        SetCurrentValue(NavigatedItemProperty, item);
                    }
                }
            }
            finally
            {
                _isScrollOriginalSource = null;
            }
        }

        private object GetFirstItemInView()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (TryGetItemBounds(i, out var bounds) && bounds.Top >= _scrollViewer.VerticalOffset)
                {
                    return Items[i];
                }
            }
            return null;
        }

        private bool TryGetItemBounds(int index, out Rect bounds)
        {
            if (index >= 0 && index < Items.Count)
            {
                if (_itemsPanel != null)
                {
                    bounds = _itemsPanel.GetItemBounds(index);
                    return !bounds.IsEmpty;
                }

                if (ItemContainerGenerator.ContainerFromIndex(index) is FrameworkElement container)
                {
                    var viewportPoint = container.TranslatePoint(new Point(0, 0), this);

                    bounds = new Rect(viewportPoint.X, viewportPoint.Y + _scrollViewer.VerticalOffset,
                        container.ActualWidth, container.ActualHeight);

                    return true;
                }
            }

            bounds = Rect.Empty;
            return false;
        }

        private void EnsureTemplateParts()
        {
            if (_scrollViewer == null)
            {
                _scrollViewer = FindVisualChild<ScrollViewer>(this);

                if (_scrollViewer != null)
                {
                    _scrollViewer.ScrollChanged -= OnScrollChanged;
                    _scrollViewer.ScrollChanged += OnScrollChanged;
                }
            }

            _itemsPanel ??= FindVisualChild<VirtualizingWrapPanel>(this);
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                {
                    return result;
                }

                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                {
                    return descendant;
                }
            }
            return null;
        }

        private void ScrollToNavigatedItem(TileViewModel item)
        {
            if (_scrollViewer == null || _isScrollOriginalSource.HasValue) return;

            if (TryGetItemBounds(Items.IndexOf(item), out var bounds))
            {
                var newOffset = Math.Max(0, bounds.Top);
                var currentOffset = _scrollViewer.VerticalOffset;

                if (currentOffset != newOffset)
                {
                    _isScrollOriginalSource = false;
                    _scrollViewer.ScrollToVerticalOffset(newOffset);

                    // ScrollChanged is expected to release the guard once the deferred scrolling operation is
                    // over. It is not raised when the offset ends up being the current one after all, e.g. when
                    // the ScrollViewer coerces it, so the guard is released after the layout pass as well. Without
                    // that, a single such scroll would leave the guard set and break the synchronization for good.
                    Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () => _isScrollOriginalSource = null);
                }
            }
        }

        private static void OnNavigatedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // SC-9515: Check IsLoaded to ensure element and internal _scrollViewer are initialized
            if (d is ScrollNavigatedItemsControl itemsControl && itemsControl.IsLoaded)
            {
                itemsControl.ScrollToNavigatedItem(e.NewValue as TileViewModel);
            }
        }
    }
}