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

namespace MacroserviceExplorer.Controls
{
    /// <summary>
    /// Interaction logic for WindowTitlebarControl.xaml
    /// </summary>
    public partial class WindowTitlebarControl : UserControl
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

        static void OnTitleChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var titlebarControl = (WindowTitlebarControl)d;
            titlebarControl.OnTitleChanged(e);
        }
        void OnTitleChanged(DependencyPropertyChangedEventArgs e)
        {
            txtTitle.Text = e.NewValue.ToString();
        }

        #endregion

        #region AlwaysOnTop

        public static readonly DependencyProperty AlwaysOnTopProperty =
            DependencyProperty.Register("AlwaysOnTop", typeof(bool), typeof(WindowTitlebarControl), new PropertyMetadata(false, OnAlwaysOnTopChanged));

        public bool AlwaysOnTop
        {
            get => (bool)GetValue(AlwaysOnTopProperty);
            set => SetValue(AlwaysOnTopProperty, value);
        }

        static void OnAlwaysOnTopChanged(DependencyObject d,DependencyPropertyChangedEventArgs e)
        {
            var alwaysOnTopbarControl = (WindowTitlebarControl)d;
            alwaysOnTopbarControl.OnAlwaysOnTopChanged(e);
        }
        void OnAlwaysOnTopChanged(DependencyPropertyChangedEventArgs e)
        {
            ontop.IsChecked = (bool) e.NewValue;
        }


        #endregion


        void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.MainWindow?.DragMove();
            if (e.ClickCount == 2)
                Maximize_OnClick(sender, e);
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


        public event EventHandler RefreshClicked;
        protected virtual void OnRefreshClicked()
        {
            RefreshClicked?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<bool> AlwayOnTopCheckaged;
        protected virtual void OnAlwayOnTopCheckaged(bool e)
        {
            AlwayOnTopCheckaged?.Invoke(this, e);
        }

        void ToggleSwitchBase_OnChecked(object sender, RoutedEventArgs e)
        {
            var sw = (ToggleSwitch.HorizontalToggleSwitch)sender;
            OnAlwayOnTopCheckaged(sw.IsChecked);
        }

    }
}
