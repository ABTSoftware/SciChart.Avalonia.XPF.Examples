using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;

namespace SciChart.Examples.Demo.Lib.Controls
{
    /// <summary>
    /// Used to assist automation peers for ItemsControl. See also <see cref="ItemsControlAutomationPeer"/>
    /// </summary>
    public class GenericAutomationPeer : UIElementAutomationPeer
    {
        public GenericAutomationPeer(UIElement owner) : base(owner)
        {
        }

        protected override List<AutomationPeer> GetChildrenCore()
        {
            var list = base.GetChildrenCore();
            list?.AddRange(GetChildPeers(Owner));
            return list;
        }

        private List<AutomationPeer> GetChildPeers(UIElement element)
        {
            var list = new List<AutomationPeer>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                if (VisualTreeHelper.GetChild(element, i) is UIElement child)
                {
                    AutomationPeer childPeer;
                    if (child is GroupItem)
                    {
                        childPeer = new GenericAutomationPeer(child);
                    }
                    else
                    {
                        childPeer = CreatePeerForElement(child);
                    }
                    if (childPeer != null)
                    {
                        list.Add(childPeer);
                    }
                    else
                    {
                        list.AddRange(GetChildPeers(child));
                    }
                }
            }
            return list;
        }
    }
}
