using System.Windows.Controls;
using SciChart.Examples.Demo.Lib.ViewModels;
using SciChart.UI.Bootstrap;
using Unity;

namespace SciChart.Examples.Demo.Lib.Views
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();

            DataContext = ServiceLocator.Container.Resolve<IMainWindowViewModel>();
        }
    }
}