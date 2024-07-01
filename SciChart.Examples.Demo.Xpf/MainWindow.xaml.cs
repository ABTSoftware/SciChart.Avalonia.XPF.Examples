using System;
using System.Windows;
using SciChart.Examples.Demo.Lib.Bootstrapper;
using SciChart.Examples.Demo.Lib.Common;
using SciChart.Examples.Demo.Lib.ViewModels;
using SciChart.UI.Bootstrap;
using Unity;

namespace SciChart.Examples.Demo.Xpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ServiceLocator.Container.Resolve<IBootstrapper>().WhenInit += (s, e) =>
            {
                Action operation = () => { DataContext = ServiceLocator.Container.Resolve<IMainWindowViewModel>(); };
                Dispatcher.BeginInvoke(operation);
            };

            // Maximise a window that is too large for the screen 
            if (Width > SystemParameters.WorkArea.Width || Height > SystemParameters.WorkArea.Height)
            {
                WindowState = WindowState.Maximized;                
            }

            // Always topmost if /quickstart mode used by UIAutomationTests
            if (DemoSettings.Instance.UIAutomationTestMode)
            {
                Topmost = true;
            }
        }
    }
}