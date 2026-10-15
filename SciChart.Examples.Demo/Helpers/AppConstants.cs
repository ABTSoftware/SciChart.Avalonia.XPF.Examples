// *************************************************************************************
// SCICHART® Copyright SciChart Ltd. 2011-2026. All rights reserved.
//
// Web:     http://www.scichart.com
// Support: support@scichart.com
// Sales:   sales@scichart.com
//
// AppConstants.cs is part of the SCICHART® Examples. Permission is hereby granted
// to modify, create derivative works, distribute and publish any part of this source
// code whether for commercial, private or personal use.
//
// The SCICHART® examples are distributed in the hope that they will be useful, but
// without any warranty. It is provided "AS IS" without warranty of any kind, either
// expressed or implied.
// *************************************************************************************
namespace SciChart.Examples.Demo.Helpers
{
    public static class AppConstants
    {
        public const string ExamplesAssemblyName = "SciChart.Examples";     
        public const string ExamplesComponentPath = "SciChart.Examples;component/";

        public const string ExternalAssemblyName = "SciChart.Examples.ExternalDependencies";
        public const string ExternalComponentPath = "SciChart.Examples.ExternalDependencies;component/";

#if XPF
        public const string DemoAssemblyName = "SciChart.Examples.Demo.Xpf";
        public const string DemoComponentPath = "SciChart.Examples.Demo.Xpf;component/";
#else
        public const string DemoAssemblyName = "SciChart.Examples.Demo";
        public const string DemoComponentPath = "SciChart.Examples.Demo;component/";
#endif
    }
}