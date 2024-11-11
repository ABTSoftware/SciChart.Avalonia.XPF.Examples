using System.Collections.Generic;
using System.Windows.Input;
using SciChart.Examples.Demo.Lib.Common;

namespace SciChart.Examples.Demo.Lib.ViewModels
{
    public class ExampleCategoryViewModel : ISelectable
    {
        public string Category { get; set; }

        public bool IsHomeCategory { get; set; }

        public IEnumerable<string> Groups { get; set; }

        public ICommand SelectCommand { get; set; }
    }
}
