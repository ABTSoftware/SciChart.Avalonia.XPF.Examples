using System.Windows.Input;

namespace SciChart.Examples.Demo.Lib.Common
{
    public interface ISelectable
    {
        ICommand SelectCommand { get; set; }
    }
}
