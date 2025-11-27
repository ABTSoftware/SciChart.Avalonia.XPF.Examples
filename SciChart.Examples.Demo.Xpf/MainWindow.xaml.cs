using System.Windows;
using System.Windows.Automation.Peers;
using SciChart.Examples.Demo.Lib.Automation;
using SciChart.Examples.Demo.Lib.Bootstrapper;
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
                Dispatcher.BeginInvoke(() => DataContext = ServiceLocator.Container.Resolve<IMainWindowViewModel>());
            };
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new MainWindowAutomationPeer(this);
        }
    }
}