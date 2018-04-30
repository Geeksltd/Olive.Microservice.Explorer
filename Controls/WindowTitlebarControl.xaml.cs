using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MicroserviceExplorer.Controls
{
    /// <summary>
    /// Interaction logic for WindowTitlebarControl.xaml
    /// </summary>
    public sealed partial class WindowTitlebarControl : UserControl
    {

        public WindowTitlebarControl()
        {
            InitializeComponent();
        }

        #region Title Dependency Property
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(WindowTitlebarControl), new PropertyMetadata("Title", OnTitleChanged));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        static void OnTitleChanged(DependencyObject dpObject,
            DependencyPropertyChangedEventArgs e)
        {
            var titlebarControl = (WindowTitlebarControl)dpObject;
            titlebarControl.OnTitleChanged(e);
        }
        void OnTitleChanged(DependencyPropertyChangedEventArgs e)
        {
            txtTitle.Text = e.NewValue.ToString();
        }

        #endregion

        void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                Maximize_OnClick(sender, e);
            else
                Application.Current.MainWindow?.DragMove();
        }

        void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow?.Close();
        }

        void Maximize_OnClick(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.WindowState = Application.Current.MainWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
                
        }

        void Minimize_OnClick(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        void Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            OnRefreshClicked();
        }


        public event EventHandler<EventArgs> RefreshClicked;

        void OnRefreshClicked()
        {
            RefreshClicked?.Invoke(this, EventArgs.Empty);
        }

    }
}
