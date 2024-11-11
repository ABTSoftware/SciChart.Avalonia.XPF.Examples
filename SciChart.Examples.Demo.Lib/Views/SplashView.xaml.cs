using System;
using System.Windows;
using System.Windows.Controls;
using SciChart.Core.Extensions;
using SciChart.Core.Utility;

namespace SciChart.Examples.Demo.Lib.Views
{
    public partial class SplashView : UserControl
    {
        public SplashView()
        {
            InitializeComponent();
#if XPF
            vTextBlock.Text = $"SciChart Avalonia XPF SDK {SciChartRuntimeInfo.GetVersion()}.";
#else
            vTextBlock.Text = $"SciChart WPF SDK {SciChartRuntimeInfo.GetVersion()}.";
#endif
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(Urls.ReleaseArticle);
            uri.Launch();
        }
    }
}