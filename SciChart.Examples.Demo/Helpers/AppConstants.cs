namespace SciChart.Examples.Demo.Helpers
{
    public static class AppConstants
    {
        public const string AssemblyName = "SciChart.Examples";     
        public const string ComponentPath = "SciChart.Examples;component/";

#if XPF
        public const string DemoAssemblyName = "SciChart.Examples.Demo.Xpf";
        public const string DemoComponentPath = "SciChart.Examples.Demo.Xpf;component/";
#else
        public const string DemoAssemblyName = "SciChart.Examples.Demo";
        public const string DemoComponentPath = "SciChart.Examples.Demo;component/";
#endif
    }
}