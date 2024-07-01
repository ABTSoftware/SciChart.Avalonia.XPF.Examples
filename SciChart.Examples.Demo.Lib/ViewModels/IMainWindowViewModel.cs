using SciChart.Examples.Demo.Lib.Common;
using ActionCommand = SciChart.Charting.Common.Helpers.ActionCommand;

namespace SciChart.Examples.Demo.Lib.ViewModels
{
    public interface IMainWindowViewModel
    {
        bool InitReady { get; set; }

        bool SearchBoxEnabled { get; set; }

        string SearchText { get; set; }

        Example SelectedExample { get; set; }

        ActionCommand HideSearchCommand { get; }

        ActionCommand ResetSelectedCommand { get; }
    }
}
