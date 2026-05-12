using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using SciChart.Examples.Demo.Helpers;
using SciChart.Examples.Demo.Helpers.Navigation;
using SciChart.UI.Bootstrap;
using Unity;

namespace SciChart.Examples.Demo.Automation
{
    public sealed class MainWindowAutomationPeer : WindowAutomationPeer, IValueProvider
    {
        public bool IsReadOnly => false;

        public string Value => GetValue();

        public MainWindowAutomationPeer(Window owner) : base(owner)
        {
            if (App.UIAutomationTestMode)
            {
                owner.Topmost = true;
            }
        }

        protected override string GetClassNameCore()
        {
            return GetType().Name;
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Value)
            {
                return this;
            }
            return base.GetPattern(patternInterface);
        }

        public void SetValue(string value)
        {
            // The value is a URI string used to navigate to the example
            var exampleUri = value?.Trim();

            if (App.UIAutomationTestMode && !string.IsNullOrEmpty(exampleUri))
            {
                var module = ServiceLocator.Container.Resolve<IModule>();
                var example = module.Examples
                    .Select(x => x.Value)
                    .FirstOrDefault(x => string.Equals(x.GetExampleUri(), exampleUri, StringComparison.InvariantCultureIgnoreCase));

                if (example != null)
                {
                    module.CurrentExample = example;
                    Navigator.Instance.Navigate(example);
                    Navigator.Instance.Push(example);
                }
            }
        }

        private static string GetValue()
        {
            if (App.UIAutomationTestMode)
            {
                var module = ServiceLocator.Container.Resolve<IModule>();
                return module.CurrentExample?.GetExampleUri();
            }
            return null;
        }
    }
}