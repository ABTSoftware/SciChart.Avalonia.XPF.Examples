using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;

namespace SciChart.Examples.Demo.Lib.Automation
{
    /// <summary>
    /// Used to assist automation peers for ItemsControl.
    /// </summary>
    public sealed class ChildItemsAutomationPeer : UIElementAutomationPeer
    {
        public ChildItemsAutomationPeer(UIElement owner) : base(owner)
        {
        }

        protected override List<AutomationPeer> GetChildrenCore()
        {
            var list = base.GetChildrenCore();
            list?.AddRange(GetChildPeers(Owner));
            return list;
        }

        private static List<AutomationPeer> GetChildPeers(UIElement element)
        {
            var list = new List<AutomationPeer>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                if (VisualTreeHelper.GetChild(element, i) is UIElement child)
                {
                    AutomationPeer childPeer;
                    if (child is GroupItem)
                    {
                        childPeer = new ChildItemsAutomationPeer(child);
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
