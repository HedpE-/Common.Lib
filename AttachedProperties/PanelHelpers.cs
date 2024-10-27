using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Common.Lib.AttachedProperties
{
    public static class PanelHelpers
    {
        public static Thickness? GetChildMargin(Panel obj)
        {
            return (Thickness?)obj.GetValue(ChildMarginProperty);
        }

        public static void SetChildMargin(Panel obj, Thickness? value)
        {
            obj.SetValue(ChildMarginProperty, value);
        }

        /// <summary>
        /// Apply a fixed margin to all direct children of the Panel, overriding all other margins.
        /// Panel descendants include Grid, StackPanel, WrapPanel, and UniformGrid
        /// </summary>
        public static readonly DependencyProperty ChildMarginProperty =
            DependencyProperty.RegisterAttached("ChildMargin", typeof(Thickness?), typeof(PanelHelpers),
                new PropertyMetadata(null, ChildMargin_PropertyChanged));

        private static void ChildMargin_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as Panel;

            target.Loaded += (s, e2) => ApplyChildMargin(target, (Thickness?)e.NewValue);

            ApplyChildMargin(target, (Thickness?)e.NewValue);
        }

        public static void ApplyChildMargin(Panel panel, Thickness? margin)
        {
            int count = VisualTreeHelper.GetChildrenCount(panel);

            object value = margin.HasValue ? margin.Value : DependencyProperty.UnsetValue;

            for (var i = 0; i < count; ++i)
            {
                var child = VisualTreeHelper.GetChild(panel, i) as FrameworkElement;

                if (child != null)
                {
                    child.SetValue(FrameworkElement.MarginProperty, value);
                }
            }
        }
    }
}
