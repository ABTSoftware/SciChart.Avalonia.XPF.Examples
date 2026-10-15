// *************************************************************************************
// SCICHART® Copyright SciChart Ltd. 2011-2026. All rights reserved.
//
// Web:     http://www.scichart.com
// Support: support@scichart.com
// Sales:   sales@scichart.com
//
// NavigationTreeViewItem.cs is part of the SCICHART® Examples. Permission is hereby granted
// to modify, create derivative works, distribute and publish any part of this source
// code whether for commercial, private or personal use.
//
// The SCICHART® examples are distributed in the hope that they will be useful, but
// without any warranty. It is provided "AS IS" without warranty of any kind, either
// expressed or implied.
// *************************************************************************************
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace SciChart.Examples.Demo.Controls
{
    public class NavigationTreeViewItem : TreeViewItem
    {
        private FrameworkElement _itemsHost;
        private Storyboard _storyboard;
        private static readonly Duration _duration = new(TimeSpan.FromMilliseconds(500));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_ItemHeader") is FrameworkElement itemsHeader)
            {
                itemsHeader.MouseLeftButtonDown += (s, e) => IsExpanded = !IsExpanded;
            }

            _itemsHost = GetTemplateChild("PART_ItemsHost") as FrameworkElement;

            if (_itemsHost != null)
            {
                _itemsHost.Height = IsExpanded ? double.NaN : 0d;
            }
        }

        protected override void OnExpanded(RoutedEventArgs e)
        {
            base.OnExpanded(e);
            AnimateSlide(true);
        }

        protected override void OnCollapsed(RoutedEventArgs e)
        {
            base.OnCollapsed(e);
            AnimateSlide(false);
        }

        private void AnimateSlide(bool expand)
        {
            if (_itemsHost == null) return;

            // Clear any in-flight animation first. An active animation holds the property at its
            // animated value and overrides the local value, so without this our Height = NaN / reads
            // below would be ignored. Passing a null AnimationTimeline removes the animation clock.
            _itemsHost.BeginAnimation(HeightProperty, null);
            _itemsHost.BeginAnimation(OpacityProperty, null);

            var heightFrom = _itemsHost.ActualHeight;
            var heightTo = 0d;

            if (expand)
            {
                // We need the content's natural height to know the animation's end value, but the
                // content is collapsed (Height == 0). So we briefly let it size to content and force
                // a synchronous layout pass to read that height.
                _itemsHost.Height = double.NaN;

                // UpdateLayout() runs the Measure + Arrange passes synchronously, right now, on the
                // UI thread. That is what makes ActualHeight valid on the next line.
                _itemsHost.UpdateLayout();

                // ActualHeight now reflects the fully-expanded content size.
                heightTo = _itemsHost.ActualHeight;

                // Set the starting point back to 0. Because everything from setting Height = NaN to
                // starting the animation below happens in this single synchronous call without ever
                // yielding to the Dispatcher, the render thread never gets to composite a frame at the
                // full height - so the user sees no flicker. The first frame painted is the animation
                // starting from 0.
                heightFrom = 0d;
            }

            var heightAnimation = new DoubleAnimation
            {
                From = heightFrom,
                To = heightTo,
                Duration = _duration,
                EasingFunction = new CubicEase
                {
                    EasingMode = expand ? EasingMode.EaseOut : EasingMode.EaseIn
                }
            };

            var opacityAnimation = new DoubleAnimation
            {
                From = expand ? 0d : 1d,
                To = expand ? 1d : 0d,
                Duration = _duration
            };

            Storyboard.SetTargetProperty(heightAnimation, new PropertyPath(HeightProperty));
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(OpacityProperty));

            _storyboard?.Stop(_itemsHost);
            _storyboard = new Storyboard();

            _storyboard.Children.Add(heightAnimation);
            _storyboard.Children.Add(opacityAnimation);

            // After expanding, release the fixed height so the subtree can grow/shrink with its
            // content afterwards (e.g. when a nested item is itself expanded). We remove the
            // animation clock (so it stops holding the end value) and hand control back to Auto.
            // On collapse we deliberately leave the animation holding Height at 0.
            if (expand)
            {
                _storyboard.Completed += OnStoryboardCompleted;
            }

            _storyboard.Begin(_itemsHost, true);
        }

        private void OnStoryboardCompleted(object sender, EventArgs e)
        {
            _storyboard.Completed -= OnStoryboardCompleted;

            _itemsHost.BeginAnimation(HeightProperty, null);
            _itemsHost.BeginAnimation(OpacityProperty, null);

            _itemsHost.Height = double.NaN;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new NavigationTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is NavigationTreeViewItem;
        }
    }
}