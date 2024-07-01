using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Navigation;
using SciChart.Core.Extensions;
using SciChart.Examples.Demo.Lib.CodeHighlighter;
using SciChart.Examples.Demo.Lib.CodeHighlighter.Formatting;

namespace SciChart.Examples.Demo.Lib.Extensions
{
    public static class RichTextBoxExtensions
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached
            ("Text", typeof(string), typeof(RichTextBoxExtensions), new PropertyMetadata(null, OnTextChanged));

        public static string GetText(DependencyObject o)
        {
            return (string)o.GetValue(TextProperty);
        }

        public static void SetText(DependencyObject o, string value)
        {
            o.SetValue(TextProperty, value);
        }

        private static WeakReference _colorizer;

        public static Paragraph Highlight(this RichTextBox rtb)
        {
            Paragraph paragraph = null;

            if (rtb != null)
            {
                var blocks = rtb.Document.Blocks;
                blocks.Clear();

                paragraph = new Paragraph();
                var xif = new XamlInlineFormatter(paragraph);

                CodeColorizer cc;
                if (_colorizer != null && _colorizer.IsAlive)
                {
                    cc = (CodeColorizer) _colorizer.Target;
                }
                else
                {
                    cc = new CodeColorizer();
                    _colorizer = new WeakReference(cc);
                }

                var languageType = (SourceLanguageType) rtb.GetValue(CodeHighlighter.CodeHighlighter.SourceLanguageProperty);
                ILanguage language = CreateLanguageInstance(languageType);

                var content = (string) rtb.GetValue(CodeHighlighter.CodeHighlighter.SourceCodeProperty);
                cc.Colorize(content, language, xif, StyleSheets.GetStyleSheet());

                blocks.Add(paragraph);
            }

            return paragraph;
        }

        private static ILanguage CreateLanguageInstance(SourceLanguageType type)
        {
            switch (type)
            {
                case SourceLanguageType.CSharp:
                    return Languages.CSharp;

                case SourceLanguageType.Cpp:
                    return Languages.Cpp;

                case SourceLanguageType.JavaScript:
                    return Languages.JavaScript;

                case SourceLanguageType.VisualBasic:
                    return Languages.VbDotNet;

                case SourceLanguageType.Xaml:
                case SourceLanguageType.Xml:
                    return Languages.Xml;

                default:
                    throw new InvalidOperationException("Could not locate the provider.");
            }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichTextBox rtb)
            {
                rtb.IsDocumentEnabled = true;

                var text = (string)e.NewValue;
                var xamlString = GetXamlFrom(text);

                LoadXamlInto(rtb, xamlString);
            }
        }

        private static void LoadXamlInto(RichTextBox rtb, string xamlStr)
        {
            var msDocument = new MemoryStream((new ASCIIEncoding()).GetBytes(xamlStr));
            var formattedDocument = XamlReader.Load(msDocument) as FlowDocument;

            rtb.Document = formattedDocument;
            SubscribeToAllHyperlinks(formattedDocument);
            msDocument.Close();
        }

        private static string GetXamlFrom(string text)
        {
            var xamlStr = new StringBuilder();

            xamlStr.Append("<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">");
            xamlStr.Append("<Paragraph xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">");
            xamlStr.Append(text);
            xamlStr.Append("</Paragraph>");
            xamlStr.Append("</FlowDocument>");

            return xamlStr.ToString();
        }

        private static void SubscribeToAllHyperlinks(FlowDocument doc)
        {
            var hyperlinks = GetVisuals(doc).OfType<Hyperlink>();
            foreach (var link in hyperlinks)
            {
                link.TargetName = null;
                link.RequestNavigate += LinkOnClick;
            }
        }

        private static void LinkOnClick(object sender, RequestNavigateEventArgs e)
        {
            e.Uri.Launch();
            e.Handled = true;
        }

        private static IEnumerable<DependencyObject> GetVisuals(DependencyObject root)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            {
                yield return child;

                foreach (var descendants in GetVisuals(child))
                {
                    yield return descendants;
                }
            }
        }
    }
}