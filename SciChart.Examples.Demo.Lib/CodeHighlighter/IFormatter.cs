﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
// <auto-generated />
// No style analysis for imported project.

using System.Collections.Generic;
using SciChart.Examples.Demo.Lib.CodeHighlighter.Parsing;

namespace SciChart.Examples.Demo.Lib.CodeHighlighter
{
    /// <summary>
    /// Defines the contract for a source code formatter.
    /// </summary>
    public interface IFormatter
    {
        /// <summary>
        /// Writes the parsed source code to the ouput using the specified style sheet.
        /// </summary>
        /// <param name="parsedSourceCode">The parsed source code to format and write to the output.</param>
        /// <param name="scopes">The captured scopes for the parsed source code.</param>
        /// <param name="styleSheet">The style sheet according to which the source code will be formatted.</param>
        void Write(string parsedSourceCode,
                   IList<Scope> scopes,
                   IStyleSheet styleSheet);
    }
}