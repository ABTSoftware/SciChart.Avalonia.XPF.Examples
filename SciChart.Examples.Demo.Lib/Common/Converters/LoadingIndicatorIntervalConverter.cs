using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SciChart.Core.Extensions;

namespace SciChart.Examples.Demo.Lib.Common.Converters
{
    public class LoadingIndicatorIntervalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double interval && interval.IsDefined())
            {
                return new Thickness(interval, 0d, 0d, 0d);
            }
            return new Thickness(0d);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}