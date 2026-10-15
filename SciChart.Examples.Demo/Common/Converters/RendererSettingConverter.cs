// *************************************************************************************
// SCICHART® Copyright SciChart Ltd. 2011-2026. All rights reserved.
//
// Web:     http://www.scichart.com
// Support: support@scichart.com
// Sales:   sales@scichart.com
//
// RendererSettingConverter.cs is part of the SCICHART® Examples. Permission is hereby granted
// to modify, create derivative works, distribute and publish any part of this source
// code whether for commercial, private or personal use.
//
// The SCICHART® examples are distributed in the hope that they will be useful, but
// without any warranty. It is provided "AS IS" without warranty of any kind, either
// expressed or implied.
// *************************************************************************************
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SciChart.Examples.Demo.ViewModels;

#if !XPF
using SciChart.Drawing.VisualXcceleratorRasterizer;
#endif

namespace SciChart.Examples.Demo.Common.Converters;

public class RendererSettingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        object selectedRenderer = DependencyProperty.UnsetValue;

        if (value is SettingsViewModel settingViewModel)
        {
            var renderer = Activator.CreateInstance(settingViewModel.SelectedRenderer);

            selectedRenderer= renderer ?? DependencyProperty.UnsetValue;
        }

        return selectedRenderer;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}