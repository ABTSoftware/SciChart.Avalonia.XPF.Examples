using SciChart.Examples.Demo.Lib.Common;

namespace SciChart.Examples.Demo.Lib.ViewModels
{
    public interface IExampleViewModel
    {
        bool InitReady { get; set; }

        Example SelectedExample { get; set; }

        ExportExampleViewModel ExportExampleViewModel { get; }

        BreadCrumbViewModel BreadCrumbViewModel { get; }
    }
}