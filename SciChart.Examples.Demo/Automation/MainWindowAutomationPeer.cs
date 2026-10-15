// *************************************************************************************
// SCICHART® Copyright SciChart Ltd. 2011-2026. All rights reserved.
//
// Web:     http://www.scichart.com
// Support: support@scichart.com
// Sales:   sales@scichart.com
//
// MainWindowAutomationPeer.cs is part of the SCICHART® Examples. Permission is hereby granted
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
                }
            }
        }

        private static string GetValue()
        {
            if (App.UIAutomationTestMode)
            {
                // Report the example that is actually on screen, not the one last requested
                var displayedPage = Navigator.Instance.CurrentExample;
                if (displayedPage != null)
                {
                    var module = ServiceLocator.Container.Resolve<IModule>();

                    if (module.Examples.TryGetValue(displayedPage.PageId, out var displayed))
                    {
                        return displayed.GetExampleUri();
                    }
                }
            }
            return null;
        }
    }
}