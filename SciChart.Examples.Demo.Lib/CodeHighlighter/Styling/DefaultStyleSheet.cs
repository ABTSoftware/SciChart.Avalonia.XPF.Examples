﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
// No style analysis for imported project.

using System.Windows.Media;
using SciChart.Examples.Demo.Lib.CodeHighlighter.Common;

namespace SciChart.Examples.Demo.Lib.CodeHighlighter.Styling
{
    public class DefaultStyleSheet : IStyleSheet
    {
        private readonly static Color DullRed = new Color { A = 255, R = 163, G = 21, B = 21 };

        private static readonly StyleDictionary _styles;

        static DefaultStyleSheet()
        {
            _styles = new StyleDictionary
            {
                new Style(ScopeName.PlainText)
                {
                    Foreground = Colors.Black,
                    Background = Colors.White
                },
                new Style(ScopeName.HtmlServerSideScript)
                {
                    Background = Colors.Yellow
                },
                new Style(ScopeName.HtmlComment)
                {
                    Foreground = Colors.Green
                },
                new Style(ScopeName.HtmlTagDelimiter)
                {
                    Foreground = Colors.Blue
                },
                new Style(ScopeName.HtmlElementName)
                {
                    Foreground = DullRed
                },
                new Style(ScopeName.HtmlAttributeName)
                {
                    Foreground = Colors.Red
                },
                new Style(ScopeName.HtmlAttributeValue)
                {
                    Foreground = Colors.Blue
                },
                new Style(ScopeName.HtmlOperator)
                {
                    Foreground = Colors.Blue
                },
                new Style(ScopeName.Comment)
                {
                    Foreground = Colors.Green
                },
                new Style(ScopeName.XmlDocTag)
                {
                    Foreground = Colors.Gray
                },
                new Style(ScopeName.XmlDocComment)
                {
                    Foreground = Colors.Green
                },
                new Style(ScopeName.String)
                {
                    Foreground = DullRed
                },
                new Style(ScopeName.StringCSharpVerbatim)
                {
                    Foreground = DullRed
                },
                new Style(ScopeName.Keyword)
                {
                    Foreground = Colors.Blue
                },
                new Style(ScopeName.PreprocessorKeyword)
                {
                    Foreground = Colors.Blue
                },
                new Style(ScopeName.HtmlEntity)
                {
                    Foreground = Colors.Red
                },
                new Style(ScopeName.XmlAttribute)
                {
                    Foreground = Colors.Red
                },
                new Style(ScopeName.XmlAttributeQuotes)
                {
                    Foreground = Colors.Black
                },
                new Style(ScopeName.XmlAttributeValue)
                {
                    Foreground = Colors.Blue
                },
                new Style(ScopeName.XmlCDataSection)
                {
                    Foreground = Colors.Gray
                },
                new Style(ScopeName.XmlComment)
                {
                    Foreground = Colors.Green
                },
                new Style(ScopeName.XmlDelimiter)
                {
                    Foreground = Colors.Blue
                },
                new Style(ScopeName.XmlName)
                {
                    Foreground = DullRed
                },
                new Style(ScopeName.ClassName)
                {
                    Foreground = Colors.Cyan
                },
                new Style(ScopeName.CssSelector)
                {
                    Foreground = DullRed
                },
                new Style(ScopeName.CssPropertyName)
                {
                    Foreground = Colors.Red
                },
                new Style(ScopeName.CssPropertyValue)
                {
                    Foreground = Colors.Blue
                },
                new Style(ScopeName.SqlSystemFunction)
                {
                    Foreground = Colors.Magenta
                }
            };
        }

        public string Name => "DefaultStyleSheet";

        public StyleDictionary Styles => _styles;
    }
}