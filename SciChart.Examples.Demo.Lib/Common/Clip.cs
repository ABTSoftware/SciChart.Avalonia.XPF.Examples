using System.Windows;
using System.Windows.Media;

namespace SciChart.Examples.Demo.Lib.Common
{
    public class Clip
    {
        public static readonly DependencyProperty ToBoundsProperty = DependencyProperty.RegisterAttached
            ("ToBounds", typeof(bool), typeof(Clip), new PropertyMetadata(false, OnToBoundsPropertyChanged));

        public static bool GetToBounds(DependencyObject depObj)
        {
            return (bool)depObj.GetValue(ToBoundsProperty);
        }

        public static void SetToBounds(DependencyObject depObj, bool clipToBounds)
        {
            depObj.SetValue(ToBoundsProperty, clipToBounds);
        }

        private static void OnToBoundsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe)
            {
                ClipToBounds(fe);

                fe.Loaded += OnLoaded;
                fe.SizeChanged += OnSizeChanged;
            }
        }

        private static void ClipToBounds(FrameworkElement fe)
        {
            fe.Clip = GetToBounds(fe)
                ? new RectangleGeometry {Rect = new Rect(0, 0, fe.ActualWidth, fe.ActualHeight)}
                : null;
        }

        private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ClipToBounds(sender as FrameworkElement);
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            ClipToBounds(sender as FrameworkElement);
        }
    }
}