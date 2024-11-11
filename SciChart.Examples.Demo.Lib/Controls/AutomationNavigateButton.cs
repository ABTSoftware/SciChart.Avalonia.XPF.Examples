using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using SciChart.Core.Utility;
using SciChart.Examples.Demo.Lib.Common;
using SciChart.Examples.Demo.Lib.Helpers.Navigation;
using SciChart.UI.Bootstrap;
using Unity;

namespace SciChart.Examples.Demo.Lib.Controls
{
    public class AutomationNavigateButton : Button
    {
        public string ExampleUri { get; set; }

        public AutomationNavigateButton()
        {
            if (!SciChartRuntimeInfo.IsXPF || !DemoSettings.Instance.UIAutomationTestMode)
            {
                IsEnabled = false;
                IsHitTestVisible = false;
                Visibility = Visibility.Collapsed;
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new NavigateAutomationPeer(this);
        }

        protected override void OnClick()
        {
            base.OnClick();

            if (DemoSettings.Instance.UIAutomationTestMode)
            {
                var module = ServiceLocator.Container.Resolve<IModule>();
                var examples = module.Examples.Select(x => x.Value);

                if (examples.Any() && !string.IsNullOrEmpty(ExampleUri))
                {
                    var example = examples.FirstOrDefault(x => $"{x.TopLevelCategory}/{x.Group}/{x.Title}" == ExampleUri);
                    if (example != null)
                    {
                        module.CurrentExample = example;
                        Navigator.Instance.Navigate(example);
                    }
                }

                ExampleUri = null;
            }
        }
    }
}
