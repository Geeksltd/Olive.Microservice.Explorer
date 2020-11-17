using System.Threading;
using System.Windows;

namespace MicroserviceExplorer.UI
{
    /// <summary>
    /// Interaction logic for Loading.xaml
    /// </summary>
    public partial class Loading : Window
    {
        public Loading(CancellationTokenSource cancellationTokenSource)
        {
            InitializeComponent();
            if (cancellationTokenSource.IsCancellationRequested)
                Close();
        }
    }
}