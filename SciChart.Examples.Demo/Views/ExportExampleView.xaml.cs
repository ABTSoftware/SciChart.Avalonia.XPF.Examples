using System;
using System.Windows.Controls;
using SciChart.Core.Extensions;

namespace SciChart.Examples.Demo.Views
{
    /// <summary>
    /// Interaction logic for ExportExampleView.xaml
    /// </summary>
    public partial class ExportExampleView : UserControl
    {
        public ExportExampleView()
        {
            InitializeComponent();
        }

        private void Hyperlink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var uri = new Uri(Urls.GithubRootUrl);
            uri.Launch();
        }
    }
}
