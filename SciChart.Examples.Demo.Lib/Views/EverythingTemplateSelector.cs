using System.Windows;
using SciChart.Charting.Common.Helpers;
using SciChart.Examples.Demo.Lib.Common;
using SciChart.Examples.Demo.Lib.ViewModels;

namespace SciChart.Examples.Demo.Lib.Views
{
    public class EverythingTemplateSelector : DataTemplateSelector
    {
        private DataTemplate _exampleDataTemplate;
        private DataTemplate _groupDataTemplate;

        public DataTemplate ExampleDataTemplate
        {
            get => _exampleDataTemplate;
            set
            {
                _exampleDataTemplate = value;
                UpdateControlTemplate();
            }
        }

        public DataTemplate GroupDataTemplate
        {
            get => _groupDataTemplate;
            set
            {
                _groupDataTemplate = value;
                UpdateControlTemplate();
            }
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var dataTemplate = base.SelectTemplate(item, container);

            if (item is Example)
            {
                dataTemplate = ExampleDataTemplate;
            }
            else if (item is EverythingGroupViewModel)
            {
                dataTemplate = GroupDataTemplate;
            }
            return dataTemplate;
        }
    }
}