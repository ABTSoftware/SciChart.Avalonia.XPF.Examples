﻿using System.Windows;
using System.Windows.Media;
using SciChart.Examples.Demo.Lib.ViewModels;
using SciChart.UI.Bootstrap;
using Unity;

namespace SciChart.Examples.Demo.Lib.Views
{
    public partial class ExampleView
    {
        public ExampleView()
        {
            InitializeComponent();

            DataContext = ServiceLocator.Container.Resolve<IExampleViewModel>();
        }

        private void OnExamplesPopupSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var rectangleGeometry = new RectangleGeometry
            {
                Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height)
            };

            ((UIElement)sender).Clip = rectangleGeometry;
        }
    }
}