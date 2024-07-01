using System.Windows.Automation.Peers;
using System.Windows.Controls;
using SciChart.Examples.Demo.Lib.Helpers;

namespace SciChart.Examples.Demo.Lib.Controls
{
    public class ItemsControlWithUIAutomation : ItemsControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new GenericAutomationPeer(this);
        }
    }
}
