using System.Windows.Input;
using SciChart.Charting.Common.Helpers;
using SciChart.Examples.Demo.Lib.Common;
using SciChart.Examples.Demo.Lib.Helpers.Navigation;
using SciChart.UI.Bootstrap;
using Unity;

namespace SciChart.Examples.Demo.Lib.ViewModels
{
    public class SearchExampleViewModel : ISelectable
    {
        public Example Example { get; set; }

        public ICommand SelectCommand { get; set; }

        public SearchExampleViewModel()
        {
            SelectCommand = new ActionCommand(() =>
            {
                if (Navigator.Instance.CanNavigateTo(AppPage.ExamplesPageId))
                {
                    Navigator.Instance.Navigate(AppPage.ExamplesPageId);
                }
                Example.SelectCommand.Execute(Example);
                ServiceLocator.Container.Resolve<IMainWindowViewModel>().SearchText = "";
            });
        }
    }
}