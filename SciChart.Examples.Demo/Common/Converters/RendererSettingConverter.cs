using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SciChart.Examples.Demo.ViewModels;

#if !XPF
using SciChart.Drawing.VisualXcceleratorRasterizer;
#endif

namespace SciChart.Examples.Demo.Common.Converters;

internal class RendererSettingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        object selectedRenderer = DependencyProperty.UnsetValue;

        if (value is SettingsViewModel settingViewModel)
        {
            var renderer = Activator.CreateInstance(settingViewModel.SelectedRenderer);
#if !XPF
            if (renderer is VisualXcceleratorRenderSurface vxRenderSurface)
            {
                vxRenderSurface.UseAlternativeFillSource = settingViewModel.UseAlternativeFillSource;
            }
#endif
            selectedRenderer = renderer ?? DependencyProperty.UnsetValue;
        }

        return selectedRenderer;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}