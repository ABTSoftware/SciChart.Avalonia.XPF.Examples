using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SciChart.Examples.Demo.Lib.Controls
{
    public class Breadcrumb : ContentControl
    {
        public static readonly DependencyProperty HomeCommandProperty = DependencyProperty.Register
            (nameof(HomeCommand), typeof(ICommand), typeof(Breadcrumb), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty BreadcrumbItemsProperty = DependencyProperty.Register
            (nameof(BreadcrumbItems), typeof(IEnumerable), typeof(Breadcrumb), new PropertyMetadata(default(IEnumerable)));

        public static readonly DependencyProperty HomeButtonStyleProperty = DependencyProperty.Register
            (nameof(HomeButtonStyle), typeof(Style), typeof(Breadcrumb), new PropertyMetadata(default(Style)));

        public static readonly DependencyProperty BreadcrumbItemStyleProperty = DependencyProperty.Register
            (nameof(BreadcrumbItemStyle), typeof(Style), typeof(Breadcrumb), new PropertyMetadata(default(Style)));

        public static readonly DependencyProperty SeparatorTemplateProperty = DependencyProperty.Register
            (nameof(SeparatorTemplate), typeof(DataTemplate), typeof(Breadcrumb), new PropertyMetadata(default(DataTemplate)));
        
        public static readonly DependencyProperty IsItemSelectedProperty = DependencyProperty.RegisterAttached
            ("IsItemSelected", typeof(bool), typeof(Breadcrumb), new PropertyMetadata(false));

        public Breadcrumb()
        {
            DefaultStyleKey = typeof(Breadcrumb);
        }

        public ICommand HomeCommand
        {
            get => (ICommand)GetValue(HomeCommandProperty);
            set => SetValue(HomeCommandProperty, value);
        }

        public IEnumerable BreadcrumbItems
        {
            get => (IEnumerable)GetValue(BreadcrumbItemsProperty);
            set => SetValue(BreadcrumbItemsProperty, value);
        }

        public Style HomeButtonStyle
        {
            get => (Style)GetValue(HomeButtonStyleProperty);
            set => SetValue(HomeButtonStyleProperty, value);
        }

        public Style BreadcrumbItemStyle
        {
            get => (Style)GetValue(BreadcrumbItemStyleProperty);
            set => SetValue(BreadcrumbItemStyleProperty, value);
        }

        public DataTemplate SeparatorTemplate
        {
            get => (DataTemplate)GetValue(SeparatorTemplateProperty);
            set => SetValue(SeparatorTemplateProperty, value);
        }

        public static void SetIsItemSelected(DependencyObject element, bool value)
        {
            element.SetValue(IsItemSelectedProperty, value);
        }

        public static bool GetIsItemSelected(DependencyObject element)
        {
            return (bool) element.GetValue(IsItemSelectedProperty);
        }
    }
}
