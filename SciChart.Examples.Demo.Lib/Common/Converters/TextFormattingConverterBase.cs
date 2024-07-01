using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using SciChart.Core.Extensions;
using SciChart.Data.Model;

namespace SciChart.Examples.Demo.Lib.Common.Converters
{
    public class TextFormattingConverterBase : DependencyObject
    {
         public static readonly DependencyProperty HighlightBrushProperty = DependencyProperty.Register
            (nameof(HighlightBrush), typeof(Brush), typeof(TextFormattingConverterBase), new PropertyMetadata(null));

        public Brush HighlightBrush
        {
            get => (Brush)GetValue(HighlightBrushProperty);
            set => SetValue(HighlightBrushProperty, value);
        }

        public string HighlightTermsBase(string text, string[] terms)
        {
            var indexSet = new HashSet<int>();

            foreach (var term in terms)
            {
                var indices = text.ToLower().AllIndexesOf(term).Reverse().ToList();
                
                foreach (var index in indices)
                {
                    for (int j = 0; j < term.Length; j++)
                    {
                        indexSet.Add(j + index);
                    }
                }
            }

            if (indexSet.Count > 0)
            {
                var list = indexSet.ToList();               
                var ranges = GetRanges(list);

                ranges.Reverse();

                var color = (HighlightBrush as SolidColorBrush)?.Color ?? Colors.Black;
                var colorString = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);

                foreach (var range in ranges)
                {
                    text = text.Insert(range.Max + 1, "</Run>");
                    text = text.Insert(range.Min, $"<Run Foreground=\"{colorString}\">");
                }
            }

            return text;
        }

        private static List<IndexRange> GetRanges(List<int> list)
        {
            if (list?.Any() == true)
            {
                list.Sort();

                var rangeList = new List<IndexRange>();
                var indexRange = new IndexRange(list[0], list[0]);

                for (int i = 1; i < list.Count; i++)
                {
                    if (list[i] - list[i - 1] > 1)
                    {
                        indexRange.Max = list[i - 1];
                        rangeList.Add(indexRange);
                        indexRange = new IndexRange(list[i], list[i]);
                    }
                }

                indexRange.Max = list.Last();
                rangeList.Add(indexRange);

                return rangeList;
            }

            return Enumerable.Empty<IndexRange>().ToList();
        }
    }
}