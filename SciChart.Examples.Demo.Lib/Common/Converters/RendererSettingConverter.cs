using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#if !XPF
using SciChart.Drawing.VisualXcceleratorRasterizer;
using SciChart.Examples.Demo.Lib.ViewModels;
#endif

namespace SciChart.Examples.Demo.Lib.Common.Converters
{
    public class RendererSettingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object selectedRenderer = DependencyProperty.UnsetValue;
#if !XPF
            if (value is SettingsViewModel settingViewModel)
            {
                var renderer = Activator.CreateInstance(settingViewModel.SelectedRenderer);
                if (renderer is VisualXcceleratorRenderSurface vxRenderSurface)
                {
                    vxRenderSurface.UseAlternativeFillSource = settingViewModel.UseAlternativeFillSource;
                }

                selectedRenderer = renderer ?? DependencyProperty.UnsetValue;
            }
#endif
            return selectedRenderer;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}