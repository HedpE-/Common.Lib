using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Common.Lib.AttachedProperties
{
    /// <summary>
    /// WPF/XAML Attached Property Helper Class that enables a window to be dragged by dragging the Registered control
    /// https://stackoverflow.com/a/35945363
    /// </summary>
    public class EnableDragHelper
    {
        /// <summary>
        /// EnableDrag Dependency Property
        /// </summary>
        public static readonly DependencyProperty EnableDragProperty = DependencyProperty.RegisterAttached(
            "EnableDrag",
            typeof(bool),
            typeof(EnableDragHelper),
            new PropertyMetadata(default(bool), OnLoaded));

        private static void OnLoaded(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var uiElement = dependencyObject as UIElement;
            if (uiElement == null || (dependencyPropertyChangedEventArgs.NewValue is bool) == false)
            {
                return;
            }
            if ((bool)dependencyPropertyChangedEventArgs.NewValue == true)
            {
                uiElement.MouseMove += UIElementOnMouseMove;
            }
            else
            {
                uiElement.MouseMove -= UIElementOnMouseMove;
            }

        }

        private static void UIElementOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            var uiElement = sender as UIElement;
            if (uiElement != null)
            {
                if (mouseEventArgs.LeftButton == MouseButtonState.Pressed)
                {
                    DependencyObject parent = uiElement;
                    int avoidInfiniteLoop = 0;
                    // Search up the visual tree to find the first parent window.
                    while ((parent is Window) == false)
                    {
                        parent = VisualTreeHelper.GetParent(parent);

                        if (parent == null) // if the element is in a Dialog, it will eventually end up in a 
                            return;

                        avoidInfiniteLoop++;
                        if (avoidInfiniteLoop == 1000)
                        {
                            // Something is wrong - we could not find the parent window.
                            return;
                        }
                    }

                    var window = parent as Window;
                    window.DragMove();
                }
            }
        }

        /// <summary>
        /// Method used to set the <see cref="EnableDragProperty"/> value for a given <seealso cref="DependencyObject"/>
        /// </summary>
        /// <param name="element"><see cref="DependencyObject"/></param>
        /// <param name="value">New property value</param>
        public static void SetEnableDrag(DependencyObject element, bool value)
        {
            element.SetValue(EnableDragProperty, value);
        }

        /// <summary>
        /// Method used to get the <see cref="EnableDragProperty"/> value for a given <seealso cref="DependencyObject"/>
        /// </summary>
        /// <param name="element"><see cref="DependencyObject"/></param>
        /// <returns><see cref="EnableDragProperty"/> value for given <seealso cref="DependencyObject"/></returns>
        public static bool GetEnableDrag(DependencyObject element)
        {
            return (bool)element.GetValue(EnableDragProperty);
        }
    }
}
