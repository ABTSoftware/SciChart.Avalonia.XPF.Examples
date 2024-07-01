using System;

namespace SciChart.Examples.Demo.Lib.Common
{
    public class DemoSettings
    {
        private static readonly Lazy<DemoSettings> _lazyInstance = new(() => new DemoSettings(), true);

        public static DemoSettings Instance => _lazyInstance.Value;

        public bool UIAutomationTestMode { get; set; }
    }
}