using System.Collections.Generic;
using System.Windows.Input;
using SciChart.Examples.Demo.Lib.Common;

namespace SciChart.Examples.Demo.Lib.ViewModels
{
    public class ExampleGroupViewModel : ISelectable
    {
        public string Group { get; set; }

        public IEnumerable<Example> Examples { get; set; }

        public ICommand SelectCommand { get; set; }
    }
}