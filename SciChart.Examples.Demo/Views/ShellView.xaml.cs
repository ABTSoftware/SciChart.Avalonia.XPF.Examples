using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SciChart.Core.Extensions;

namespace SciChart.Examples.Demo.Views
{
    public partial class ShellView : UserControl
    {
        public ShellView()
        {
            InitializeComponent();

            SearchBoxWrapper.SizeChanged += (s, e) =>
            {
                if (e.WidthChanged)
                {
                    if (SearchBoxWrapper.ActualWidth <= 400)
                    {
                        SearchBox.Width = double.NaN;
                        SearchBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                    }
                    else
                    {
                        SearchBox.Width = 400;
                        SearchBox.HorizontalAlignment = HorizontalAlignment.Right;
                    }
                }
            };
        }

        private void OnSciChartLogoMouseDown(object sender, MouseButtonEventArgs e)
        {
            var uri = new Uri(Urls.SciChartWebSite);

            uri.Launch();
        }
    }
}