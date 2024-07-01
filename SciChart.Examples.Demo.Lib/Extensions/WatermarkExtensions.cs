using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SciChart.Core.Extensions;

namespace SciChart.Examples.Demo.Lib.Extensions
{
    public class WatermarkExtensions
    {
        public static readonly DependencyProperty WatermarkContentProperty = DependencyProperty.RegisterAttached
            ("WatermarkContent", typeof(object), typeof(WatermarkExtensions), new PropertyMetadata(default(object)));

        public static void SetWatermarkContent(DependencyObject element, object value)
        {
            element.SetValue(WatermarkContentProperty, value);
        }

        public static object GetWatermarkContent(DependencyObject element)
        {
            return element.GetValue(WatermarkContentProperty);
        }

        public static readonly DependencyProperty WatermarkContentTemplateProperty = DependencyProperty.RegisterAttached
            ("WatermarkContentTemplate", typeof(DataTemplate), typeof(WatermarkExtensions), new PropertyMetadata(default(DataTemplate)));

        public static void SetWatermarkContentTemplate(DependencyObject element, DataTemplate value)
        {
            element.SetValue(WatermarkContentTemplateProperty, value);
        }

        public static DataTemplate GetWatermarkContentTemplate(DependencyObject element)
        {
            return (DataTemplate)element.GetValue(WatermarkContentTemplateProperty);
        }

        public static readonly DependencyProperty LoseFocusOnEscapeProperty = DependencyProperty.RegisterAttached
            ("LoseFocusOnEscape", typeof(bool), typeof(WatermarkExtensions), new PropertyMetadata(default(bool), OnLoseFocusOnEscapeChanged));

        public static void SetLoseFocusOnEscape(DependencyObject element, bool value)
        {
            element.SetValue(LoseFocusOnEscapeProperty, value);
        }

        public static bool GetLoseFocusOnEscape(DependencyObject element)
        {
            return (bool)element.GetValue(LoseFocusOnEscapeProperty);
        }

        private static void OnLoseFocusOnEscapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox && (bool)e.NewValue)
            {
                textBox.KeyDown += (s, arg) =>
                {
                    if (arg.Key == Key.Escape)
                    {
                        if (textBox.Parent is UIElement parent)
                        {
                            var parentControl = parent.FindVisualParent<Control>();

                            parentControl?.Focus();                           
                        }
                    }
                };
            }
        }
    }
}
