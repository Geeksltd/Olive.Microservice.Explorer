using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace MicroserviceExplorer.Utils
{
    /// <summary>
    /// Class that provides the Watermark attached property
    /// </summary>
    [EscapeGCop("This class is from outside resources")]
    public static class WatermarkService
    {
        /// <summary>
        /// Watermark Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached(
            "Watermark",
            typeof(object),
            typeof(WatermarkService),
            new FrameworkPropertyMetadata((object)null, OnWatermarkChanged));

        #region Private Fields

        /// <summary>
        /// Dictionary of ItemsControls
        /// </summary>
        static readonly Dictionary<object, ItemsControl> ItemsControls = new Dictionary<object, ItemsControl>();

        #endregion

        /// <summary>
        /// Gets the Watermark property.  This dependency property indicates the watermark for the control.
        /// </summary>
        /// <param name="dependencyObject"><see cref="DependencyObject"/> to get the property from</param>
        /// <returns>The value of the Watermark property</returns>
        static object GetWatermark(DependencyObject dependencyObject)
        {
            return (object)dependencyObject.GetValue(WatermarkProperty);
        }

        /// <summary>
        /// Sets the Watermark property.  This dependency property indicates the watermark for the control.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject"/> to set the property on</param>
        /// <param name="value">value of the property</param>
        public static void SetWatermark(DependencyObject d, object value)
        {
            d.SetValue(WatermarkProperty, value);
        }

        /// <summary>
        /// Handles changes to the Watermark property.
        /// </summary>
        /// <param name="dependencyObject"><see cref="DependencyObject"/> that fired the event</param>
        /// <param name="_">A <see cref="DependencyPropertyChangedEventArgs"/> that contains the event data.</param>
        static void OnWatermarkChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs _)
        {
            var control = (Control)dependencyObject;
            control.Loaded += Control_Loaded;

            switch (dependencyObject)
            {
                case ComboBox _:
                    control.GotKeyboardFocus += Control_GotKeyboardFocus;
                    control.LostKeyboardFocus += Control_Loaded;
                    break;
                case TextBox _:
                    control.GotKeyboardFocus += Control_GotKeyboardFocus;
                    control.LostKeyboardFocus += Control_Loaded;
                    ((TextBox)control).TextChanged += Control_GotKeyboardFocus;
                    break;
                default:
                    break;
            }

            switch (dependencyObject)
            {
                case ItemsControl i when !(i is ComboBox):
                    // for Items property  
                    i.ItemContainerGenerator.ItemsChanged += ItemsChanged;
                    ItemsControls.Add(i.ItemContainerGenerator, i);

                    // for ItemsSource property  
                    var prop = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, i.GetType());
                    prop.AddValueChanged(i, ItemsSourceChanged);
                    break;
                default:
                    break;
            }
        }

        #region Event Handlers

        /// <summary>
        /// Handle the GotFocus event on the control
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data.</param>
        static void Control_GotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            var control = (Control)sender;
            if (ShouldShowWatermark(control))
            {
                ShowWatermark(control);
            }
            else
            {
                RemoveWatermark(control);
            }
        }

        /// <summary>
        /// Handle the Loaded and LostFocus event on the control
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data.</param>
        static void Control_Loaded(object sender, RoutedEventArgs e)
        {
            var control = (Control)sender;
            if (ShouldShowWatermark(control))
            {
                ShowWatermark(control);
            }
        }

        /// <summary>
        /// Event handler for the items source changed event
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        static void ItemsSourceChanged(object sender, EventArgs e)
        {
            var itemsControl = (ItemsControl)sender;
            if (itemsControl.ItemsSource != null)
            {
                if (ShouldShowWatermark(itemsControl))
                {
                    ShowWatermark(itemsControl);
                }
                else
                {
                    RemoveWatermark(itemsControl);
                }
            }
            else
            {
                ShowWatermark(itemsControl);
            }
        }

        /// <summary>
        /// Event handler for the items changed event
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="ItemsChangedEventArgs"/> that contains the event data.</param>
        static void ItemsChanged(object sender, ItemsChangedEventArgs e)
        {
            ItemsControl control;
            if (ItemsControls.TryGetValue(sender, out control))
            {
                if (ShouldShowWatermark(control))
                {
                    ShowWatermark(control);
                }
                else
                {
                    RemoveWatermark(control);
                }
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Remove the watermark from the specified element
        /// </summary>
        /// <param name="control">Element to remove the watermark from</param>
        static void RemoveWatermark(UIElement control)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);

            // layer could be null if control is no longer in the visual tree
            if (layer != null)
            {
                Adorner[] adorners = layer.GetAdorners(control);
                if (adorners == null) return;

                foreach (Adorner adorner in adorners)
                {
                    if (adorner is WatermarkAdorner)
                    {
                        adorner.Visibility = Visibility.Hidden;
                        layer.Remove(adorner);
                    }
                }
            }
        }

        /// <summary>
        /// Show the watermark on the specified control
        /// </summary>
        /// <param name="control">Control to show the watermark on</param>
        static void ShowWatermark(Control control)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);

            // layer could be null if control is no longer in the visual tree
            if (layer != null)
            {
                layer.Add(new WatermarkAdorner(control, GetWatermark(control)));
            }
        }

        /// <summary>
        /// Indicates whether or not the watermark should be shown on the specified control
        /// </summary>
        /// <param name="control"><see cref="Control"/> to test</param>
        /// <returns>true if the watermark should be shown; false otherwise</returns>
        static bool ShouldShowWatermark(Control control)
        {
            switch (control)
            {
                case ComboBox _:
                    return (control as ComboBox)?.Text == string.Empty;
                case TextBoxBase _:
                    return (control as TextBox)?.Text == string.Empty;
                case ItemsControl _:
                    return ((ItemsControl)control).Items.Count == 0;
                default:
                    return false;
            }
        }

        #endregion
    }
}