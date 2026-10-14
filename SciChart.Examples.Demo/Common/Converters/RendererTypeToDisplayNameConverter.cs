using System;
using System.Globalization;
using System.Windows.Data;
using SciChart.Drawing.VisualXcceleratorRasterizer;
using SciChart.Drawing.XamlRasterizer;

#if !XPF
using SciChart.Drawing.HighQualityRasterizer;
using SciChart.Drawing.HighSpeedRasterizer;
#endif

namespace SciChart.Examples.Demo.Common.Converters
{
    public class RendererTypeToDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Type rendererType && parameter is string rendererTypeName)
            {
                return rendererType.Name.Equals(rendererTypeName);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is string rendererTypeName)
            {
                switch (rendererTypeName)
                {
#if !XPF
                    case "HighQualityRenderSurface": return typeof(HighQualityRenderSurface);
                    case "HighSpeedRenderSurface": return typeof(HighSpeedRenderSurface);
#endif
                    case "XamlRenderSurface": return typeof(XamlRenderSurface);
                    case "VisualXcceleratorRenderSurface": return typeof(VisualXcceleratorRenderSurface);
                }
            }
            return Binding.DoNothing;
        }
    }
}
