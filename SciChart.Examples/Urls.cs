namespace SciChart.Examples
{
    /// <summary>
    /// Place to put web links which are launched at various places throughout the app
    /// </summary>
    public static class Urls
    {
    	public static string SciChartWebSite => "https://www.scichart.com";  	
    	public static string DocumentationRootUrl => "https://www.scichart.com/wpf-chart-documentation";    		
#if XPF
		public static string GithubRootUrl => "https://github.com/ABTSoftware/SciChart.Avalonia.XPF.Examples/";	
		public static string GithubExampleRootUrl => "https://github.com/ABTSoftware/SciChart.Avalonia.XPF.Examples/tree/main/SciChart.Examples/Examples/";	
		public static string ReleaseArticle => "https://www.scichart.com/blog/running-scichart-wpf-on-linux-its-possible-heres-how/";
#else
        public static string GithubRootUrl => "https://github.com/ABTSoftware/SciChart.Wpf.Examples/tree/SciChart_v8_Dev";
        public static string GithubExampleRootUrl => "https://github.com/ABTSoftware/SciChart.Wpf.Examples/tree/SciChart_v8_Dev/Examples/SciChart.Examples/Examples";
        public static string ReleaseArticle => "https://scichart.com/scichart-wpf-v8-0-released";
#endif
    }
}