﻿using System;
using System.Windows;
using System.Windows.Controls;
using SciChart.Examples.Demo.Lib.Bootstrapper;
using SciChart.Examples.Demo.Lib.ViewModels;
using SciChart.UI.Bootstrap;
using Unity;

namespace SciChart.Examples.Demo.Lib.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();

            IsVisibleChanged += OnIsVisibleChanged;

            ServiceLocator.Container.Resolve<IBootstrapper>().WhenInit += (s, e) =>
            {
                Action operation = () =>
                {
                    var settingsViewModel = ServiceLocator.Container.Resolve<ISettingsViewModel>();
                    settingsViewModel.ParentViewModel = ServiceLocator.Container.Resolve<IMainWindowViewModel>();
                    DataContext = settingsViewModel;
                };

                Dispatcher.BeginInvoke(operation);
            };
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is ISettingsViewModel viewModel)
            {
                viewModel.OnIsVisibleChanged((bool)e.NewValue);
            }
        }
    }
}
