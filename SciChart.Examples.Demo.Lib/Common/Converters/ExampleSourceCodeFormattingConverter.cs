using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using SciChart.Core.Extensions;
using SciChart.Examples.Demo.Lib.ViewModels;
using SciChart.UI.Bootstrap;
using Unity;

namespace SciChart.Examples.Demo.Lib.Common.Converters
{
    public class ExampleSourceCodeFormattingConverter : TextFormattingConverterBase, IValueConverter
    {
        private IMainWindowViewModel _mainWindowViewModel;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            _mainWindowViewModel ??= ServiceLocator.Container.Resolve<IMainWindowViewModel>();

            var searchText = _mainWindowViewModel?.SearchText ?? string.Empty;

            if (value is Dictionary<string, string> codeFiles && !searchText.IsNullOrEmpty())
            {
                var xamlFiles = codeFiles.Where(x => x.Key.EndsWith(".xaml"));
                var xamlLines = new List<string>();

                foreach (var file in xamlFiles)
                {
                    xamlLines.AddRange(file.Value.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None));
                }

                var searchTerms = searchText.Split(' ').Where(x => x != "").Select(x => x.ToLower().Trim()).ToArray();
                var toHighlight = new HashSet<string>();

                foreach (var term in searchTerms)
                {
                    var containsTerm = xamlLines.Where(x => x != "" && x.ToLower().Contains(term));
                    containsTerm.Take(2).Select(x => x.Trim()).ForEachDo(x => toHighlight.Add(x));
                }

                string result;

                if (toHighlight.Any())
                {
                    var lines = toHighlight.Take(2).Select(x => x.Trim().Replace('<', ' ').Replace('>', ' ') + '.').ToList();
                    result = HighlightText(lines, searchTerms);
                }
                else
                {
                    var lines = xamlLines.Take(2).Select(x => string.Format("... {0} ...", x.Trim().Replace('<', ' ').Replace('>', ' ').ToList()));
                    result = string.Join("...", lines);
                }

                return string.IsNullOrEmpty(result) ? "[No Results]" : result;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string HighlightText(List<string> lines, string[] terms)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length > 1000)
                {
                    var termIndex = -1;
                    var termLength = 0;

                    foreach (var term in terms)
                    {
                        var index = lines[i].IndexOf(term, StringComparison.OrdinalIgnoreCase);

                        if (index >= 0)
                        {
                            termIndex = index;
                            termLength = term.Length;
                            break;
                        }
                    }

#pragma warning disable IDE0057 // Use range operator

                    if (termIndex >= 0)
                    {
                        var startIndex = Math.Max(0, termIndex - 50);
                        var endIndex = Math.Min(lines[i].Length - 1, termIndex + termLength + 50);

                        lines[i] = lines[i].Substring(startIndex, endIndex - startIndex);
                    }
                    else
                    {

                        lines[i] = lines[i].Substring(0, Math.Min(lines[i].Length, 100));
                    }

#pragma warning restore IDE0057 // Use range operator

                }

                lines[i] = HighlightTermsBase(lines[i], terms);
            }

            var result = string.Format("... {0} ...", string.Join("...", lines));
            return result;
        }
    }
}