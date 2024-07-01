using System;
using System.Windows.Controls;
using System.Windows.Input;
using SciChart.Core.Extensions;

namespace SciChart.Examples.Demo.Lib.Views
{
    public partial class ShellView : UserControl
    {
        public ShellView()
        {
            InitializeComponent();           
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var uri = new Uri(Urls.SciChartWebSite);
            uri.Launch();
        }
    }
}