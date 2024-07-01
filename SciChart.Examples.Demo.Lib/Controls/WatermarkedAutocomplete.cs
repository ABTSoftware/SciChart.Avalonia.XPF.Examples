using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SciChart.Examples.Demo.Lib.Controls
{
    [TemplatePart(Name = "Text", Type = typeof(TextBox))]
    [TemplatePart(Name = "Popup", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_SearchIcon", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_Watermark", Type = typeof(TextBlock))]
    public class WatermarkedAutocomplete : AutoCompleteBox
    {
        private Popup _contentListPopup;

        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register
            (nameof(Watermark), typeof(string), typeof(WatermarkedAutocomplete), new PropertyMetadata(null));

        public string Watermark
        {
            get => (string)GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        public WatermarkedAutocomplete()
        {
            DefaultStyleKey = typeof(WatermarkedAutocomplete);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _contentListPopup = GetTemplateChild("Popup") as Popup;

            if (_contentListPopup != null && Window.GetWindow(this) is Window window)
            {
                window.LocationChanged += WindowOnLocationChanged;
            }
        }

        private void WindowOnLocationChanged(object sender, EventArgs e)
        {
            if (_contentListPopup?.IsOpen == true)
            {
                _contentListPopup.IsOpen = false;
            }
        }
    }
}