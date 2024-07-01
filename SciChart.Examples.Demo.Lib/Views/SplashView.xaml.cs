using System;
using System.Windows;
using System.Windows.Controls;
using SciChart.Core.Extensions;

namespace SciChart.Examples.Demo.Lib.Views
{
    public partial class SplashView : UserControl
    {
        public SplashView()
        {
            InitializeComponent();
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(Urls.ReleaseArticle);
            uri.Launch();
        }
    }
}