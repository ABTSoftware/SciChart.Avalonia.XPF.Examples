using System.Windows.Input;
using SciChart.Charting.Common.Helpers;
using SciChart.Examples.Demo.Lib.Common;
using SciChart.Examples.Demo.Lib.Helpers;
using SciChart.Examples.Demo.Lib.Helpers.Navigation;

namespace SciChart.Examples.Demo.Lib.ViewModels
{
    public class MostPopularExampleViewModel : ISelectable
    {
        public ExampleRating Rating { get; set; }

        public Example Example { get; set; }

        public ICommand SelectCommand { get; set; }

        public MostPopularExampleViewModel()
        {
            SelectCommand = new ActionCommand(() =>
            {
                if (Navigator.Instance.CanNavigateTo(AppPage.ExamplesPageId))
                {
                    Navigator.Instance.Navigate(AppPage.ExamplesPageId);
                }
                Example.SelectCommand.Execute(Example);
            });
        }

        public MostPopularExampleViewModel(Example example) : this()
        {
            Example = example;
            Rating = new ExampleRating();
        }

        public MostPopularExampleViewModel(Example example, ExampleRating rating) : this()
        {
            Example = example;
            Rating = rating;
        }
    }
}