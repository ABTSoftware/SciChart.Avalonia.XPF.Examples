using System;
using System.Globalization;
using System.Windows.Data;

namespace SciChart.Examples.Demo.Lib.Common.Converters
{
    public class IconSizeConverter : IValueConverter
    {
        public double SizeMultiplier { get; set; } = 1d;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double size && !double.IsNaN(size))
            {
                return Math.Max(0d, size * SizeMultiplier);
            }
            return 0d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}