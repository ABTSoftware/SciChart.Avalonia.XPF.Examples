using System;
using System.Windows;
using System.Windows.Controls;
using SciChart.Core.Extensions;

namespace SciChart.Examples.Demo.Lib.Views
{
    public partial class SourceTipsView : UserControl
    {
        public SourceTipsView()
        {
            InitializeComponent();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(Urls.GithubRootUrl);
            uri.Launch();
        }
    }
}
