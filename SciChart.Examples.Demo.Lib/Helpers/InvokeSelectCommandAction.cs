using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using SciChart.Examples.Demo.Lib.Common;
using SciChart.Examples.ExternalDependencies.Controls.CoverFlow;

namespace SciChart.Examples.Demo.Lib.Helpers
{
    /* NOTE:
     * We use Microsoft.Xaml.Behaviors.Behavior as a base class for this behaviour. We have embedded the source for
     * MS Behaviours in our SciChart.Examples.ExternalDependencies DLL for example purposes only and for compatibility with 
     * WPF and .NET Core
     *
     * What you should do is reference either System.Windows.Interactivity or Microsoft.Xaml.Behaviors.Wpf from NuGet
     * as it is not recommended to reference SciChart.Examples.ExternalDependencies in your applications 
     */

    public class InvokeSelectCommandAction : TriggerAction<Control>
    {
        protected override void Invoke(object parameter)
        {
            if (parameter is SelectionChangedEventArgs)
            {
                var eventArgs = parameter as SelectionChangedEventArgs;

                if ((eventArgs.AddedItems.Count > 0 ? eventArgs.AddedItems[0] : null) is ISelectable selectedExample)
                {
                    selectedExample.SelectCommand.Execute(selectedExample);
                }
            }
            else if (parameter is CoverFlowEventArgs)
            {
                var eventArgs = parameter as CoverFlowEventArgs;
                
                if (eventArgs?.Item is ISelectable categoryViewModel)
                {
                    categoryViewModel.SelectCommand?.Execute(categoryViewModel);
                }             
            }
        }
    }
}
