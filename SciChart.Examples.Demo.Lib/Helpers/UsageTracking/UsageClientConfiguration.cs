using SciChart.UI.Bootstrap;

namespace SciChart.Examples.Demo.Lib.Helpers.UsageTracking
{
    public interface IUsageClientConfiguration
    {
        string Address { get; }
    }

    [ExportType(typeof(IUsageClientConfiguration), CreateAs.Singleton)]
    public class UsageClientConfiguration : IUsageClientConfiguration
    {
        public string Address => "http://licensing.scichart.com";
    } 
}