using System.Windows;
using System.Windows.Controls;

namespace SciChart.Examples.Demo.Lib.Controls
{
    public class LoadingIndicator : ContentControl
    {
        public static readonly DependencyProperty EllipseDiameterProperty = DependencyProperty.Register
            (nameof(EllipseDiameter), typeof(double), typeof(LoadingIndicator), new PropertyMetadata(8d));

        public static readonly DependencyProperty EllipseIntervalProperty = DependencyProperty.Register
            (nameof(EllipseInterval), typeof(double), typeof(LoadingIndicator), new PropertyMetadata(4d));

        public double EllipseDiameter
        {
            get => (double)GetValue(EllipseDiameterProperty);
            set => SetValue(EllipseDiameterProperty, value);
        }

        public double EllipseInterval
        {
            get => (double)GetValue(EllipseIntervalProperty);
            set => SetValue(EllipseIntervalProperty, value);
        }

        public LoadingIndicator()
        {
            DefaultStyleKey = typeof(LoadingIndicator);
        }
    }
}