﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SciChart.Examples.Demo.Lib.Controls
{
    public class EndlessItemsControl : ListBox
    {
        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register
            (nameof(IsBusy), typeof(bool), typeof(EndlessItemsControl), new PropertyMetadata(false));

        public static readonly DependencyProperty AllItemsProperty = DependencyProperty.Register
            (nameof(AllItems), typeof(IEnumerable), typeof(EndlessItemsControl), new PropertyMetadata(null, OnAllItemsPropertyChanged));

        public static readonly DependencyProperty StartCapacityProperty = DependencyProperty.Register
            (nameof(StartCapacity), typeof (int), typeof (EndlessItemsControl), new PropertyMetadata(10));

        public static readonly DependencyProperty AddMoreCapacityProperty = DependencyProperty.Register
            (nameof(AddMoreCapacity), typeof (int), typeof (EndlessItemsControl), new PropertyMetadata(10));

        public static readonly DependencyProperty PreLoadingDelayProperty = DependencyProperty.Register
            (nameof(PreLoadingDelay), typeof (TimeSpan), typeof (EndlessItemsControl), new PropertyMetadata(TimeSpan.FromSeconds(3)));

        public bool IsBusy
        {
            get => (bool)GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }

        public IEnumerable AllItems
        {
            get => (IEnumerable)GetValue(AllItemsProperty);
            set => SetValue(AllItemsProperty, value);
        }

        public int StartCapacity
        {
            get => (int)GetValue(StartCapacityProperty);
            set => SetValue(StartCapacityProperty, value);
        }

        public int AddMoreCapacity
        {
            get => (int)GetValue(AddMoreCapacityProperty);
            set => SetValue(AddMoreCapacityProperty, value);
        }

        public TimeSpan PreLoadingDelay
        {
            get => (TimeSpan)GetValue(PreLoadingDelayProperty);
            set => SetValue(PreLoadingDelayProperty, value);
        }

        private List<object> _allItems;

        private ScrollViewer _scrollViewer;

        public ObservableCollection<object> CurrentItems { get; set; }

        public EndlessItemsControl()
        {
            DefaultStyleKey = typeof(EndlessItemsControl);
            CurrentItems = new ObservableCollection<object>();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _scrollViewer = (ScrollViewer)GetTemplateChild("PART_ScrollViewer");

            var listener = new PropertyChangeNotifier();

            listener.PropertyChanged += (s, e) =>
            {
                var offset = _scrollViewer.VerticalOffset;
                var height = _scrollViewer.ScrollableHeight;

                if (offset >= height && _allItems != null && _allItems.Count > 0)
                {
                    AddMoreItems(); 
                }
            };

            var binding = new Binding("VerticalOffset") { Source = _scrollViewer };

            listener.Attach(_scrollViewer, binding);
        }

        private void AddMoreItems()
        {
            var isBusy = (bool)GetValue(IsBusyProperty);
            if (!isBusy)
            {
                SetValue(IsBusyProperty, true);
                SetValue(IsBusyProperty, false);
                SetValue(IsBusyProperty, true);

                var delay = (TimeSpan)GetValue(PreLoadingDelayProperty);
                
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(delay);

                    _allItems.Take(10).ToList().ForEach(item =>
                    {
                        Dispatcher.BeginInvoke(new Action(() => CurrentItems.Add(item)));

                        _allItems.Remove(item);
                    });

                }).ContinueWith(_ =>
                {
                    Dispatcher.BeginInvoke(new Action(() => SetValue(IsBusyProperty, false)));
                });
            }
        }

        private static void OnAllItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var itemsControl = (EndlessItemsControl)d;
            if (itemsControl != null)
            {
                var newItems = ((IEnumerable<object>)args.NewValue).ToList();
                itemsControl.RefhreshItemsSource(newItems);
                itemsControl._scrollViewer?.ScrollToTop();
            }
        }

        private void RefhreshItemsSource(List<object> newItems)
        {
            var startCapacity = (int)GetValue(StartCapacityProperty);

            _allItems = newItems;
            CurrentItems = new ObservableCollection<object>(_allItems.Take(startCapacity));
            _allItems.Take(startCapacity).ToList().ForEach(item => _allItems.Remove(item));

            var binding = new Binding
            {
                Path = new PropertyPath("CurrentItems"),
                Source = this,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };

            SetBinding(ItemsSourceProperty, binding);
        }
    }
}
