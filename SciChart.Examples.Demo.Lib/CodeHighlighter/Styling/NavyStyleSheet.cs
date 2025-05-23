﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
// No style analysis for imported project.

using System.Windows.Media;
using SciChart.Examples.Demo.Lib.CodeHighlighter.Common;

namespace SciChart.Examples.Demo.Lib.CodeHighlighter.Styling
{
    public class NavyStyleSheet : IStyleSheet
    {
        private readonly static Color DullRed = new Color { A = 255, R = 163, G = 21, B = 21 };

        private static readonly StyleDictionary _styles;

        static NavyStyleSheet()
        {
            _styles = new StyleDictionary
            {
                new Style(ScopeName.PlainText)
                {
                    Foreground = Color.FromArgb(0xFF, 0xE6, 0xE6, 0xE6),
                    Background = Color.FromArgb(0xFF, 0x1C, 0x1c, 0x1C)
                },
                new Style(ScopeName.HtmlServerSideScript)
                {
                    Background = Colors.Yellow
                },
                new Style(ScopeName.HtmlComment)
                {
                    Foreground = Color.FromArgb(0xFF, 0x67, 0xBC, 0xAD)
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
                    Foreground = Color.FromArgb(0xFF, 0x67, 0xBC, 0xAD)
                },
                new Style(ScopeName.XmlDocTag)
                {
                    Foreground = Color.FromArgb(0xFF, 0x67, 0xBC, 0xAD)
                },
                new Style(ScopeName.XmlDocComment)
                {
                    Foreground = Color.FromArgb(0xFF, 0x67, 0xBC, 0xAD)
                },
                new Style(ScopeName.String)
                {
                    Foreground = Color.FromArgb(0xFF, 0xD6, 0x8D, 0x75)
                },
                new Style(ScopeName.StringCSharpVerbatim)
                {
                    Foreground = Color.FromArgb(0xFF, 0xD6, 0x8D, 0x75)
                },
                new Style(ScopeName.Keyword)
                {
                    Foreground = Color.FromArgb(0xFF, 0x46, 0x8C, 0xC6)
                },
                new Style(ScopeName.PreprocessorKeyword)
                {
                    Foreground = Color.FromArgb(0xFF, 0x46, 0x8C, 0xC6)
                },
                new Style(ScopeName.HtmlEntity)
                {
                    Foreground = Color.FromArgb(0xFF, 0xE6, 0xE6, 0xE6)
                },
                new Style(ScopeName.XmlAttribute)
                {
                    Foreground = Color.FromArgb(0xFF, 0x89, 0xCA, 0xDA)
                },
                new Style(ScopeName.XmlAttributeQuotes)
                {
                    Foreground = Color.FromArgb(0xFF, 0x59, 0x9C, 0xD6)
                },
                new Style(ScopeName.XmlAttributeValue)
                {
                    Foreground = Color.FromArgb(0xFF, 0xA3, 0x3D, 0x86)
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
                    Foreground = Color.FromArgb(0xFF, 0x80, 0x80, 0x80)
                },
                new Style(ScopeName.XmlName)
                {
                    Foreground = Color.FromArgb(0xFF, 0xE6, 0xE6, 0xE6)
                },
                new Style(ScopeName.ClassName)
                {
                    Foreground = Color.FromArgb(0xFF, 0x4E, 0xC9, 0xB0)
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

        public string Name => "NavyStyleSheet";

        public StyleDictionary Styles => _styles;
    }
}