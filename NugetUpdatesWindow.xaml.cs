using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace MacroserviceExplorer
{
    /// <summary>
    /// Interaction logic for NugetUpdatesWindow.xaml
    /// </summary>
    public partial class NugetUpdatesWindow : Window
    {
        public ObservableCollection<MyNugetRef> NugetList { get; set; }
        public NugetUpdatesWindow()
        {
            InitializeComponent();
            DataContext = NugetList;
        }

    }
}
