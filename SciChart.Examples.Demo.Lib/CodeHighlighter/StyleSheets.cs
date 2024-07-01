// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
// No style analysis for imported project.

using SciChart.Examples.Demo.Lib.CodeHighlighter.Styling;

namespace SciChart.Examples.Demo.Lib.CodeHighlighter
{
    /// <summary>
    /// Provides easy access to ColorCode's built-in style sheets.
    /// </summary>
    public static class StyleSheets
    {
        /// <summary>
        /// Gets or sets a theme name.
        /// </summary>
        public static string ThemeName { get; set; } = "Navy";

        /// <summary>
        /// Returns a new instance of the style sheet based on the theme name.
        /// </summary>
        public static IStyleSheet GetStyleSheet()
        {
            if (ThemeName == "Navy")
            { 
                return new NavyStyleSheet();
            }

            if (ThemeName == "Dark")
            {
                return new DarkStyleSheet();
            }

            return new DefaultStyleSheet();
        }
    }
}