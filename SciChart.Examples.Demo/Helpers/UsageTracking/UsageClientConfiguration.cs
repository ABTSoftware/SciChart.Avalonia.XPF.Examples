using SciChart.UI.Bootstrap;

namespace SciChart.Examples.Demo.Helpers.UsageTracking
{
    public interface IUsageClientConfiguration
    {
        string Address { get; }
    }

    [ExportType(typeof(IUsageClientConfiguration), CreateAs.Singleton)]
    public class UsageClientConfiguration : IUsageClientConfiguration
    {
        public string Address => Properties.Settings.Default.UsageServiceAddress;
    } 
}
