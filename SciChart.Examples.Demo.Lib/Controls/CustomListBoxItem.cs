using System.Windows;
using System.Windows.Controls;

namespace SciChart.Examples.Demo.Lib.Controls
{
    [TemplateVisualState(GroupName = "GroupStates", Name = "GroupState")]
    [TemplateVisualState(GroupName = "GroupStates", Name = "ExampleState")]
    public class CustomListBoxItem : ListBoxItem
    {
        public static readonly DependencyProperty IsGroupProperty = DependencyProperty.Register
            (nameof(IsGroup), typeof (bool), typeof (CustomListBoxItem), new PropertyMetadata(false, IsGroupPropertyChanged));

        public bool IsGroup
        {
            get => (bool)GetValue(IsGroupProperty);
            set => SetValue(IsGroupProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UpdateGroupState((bool)GetValue(IsGroupProperty));
        }

        private void UpdateGroupState(bool isGroup)
        {
            VisualStateManager.GoToState(this, isGroup ? "GroupState" : "ExampleState", true);
        }

        private static void IsGroupPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (d is CustomListBoxItem customListBoxItem)
            {
                customListBoxItem.UpdateGroupState((bool)args.NewValue);
            }
        }
    }
}