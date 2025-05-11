using System.Windows.Controls;

namespace GrpsOverAllExamplesClients.Ui.HealthCheck
{
    /// <summary>
    /// Interaction logic for HealthCheckControl.xaml
    /// </summary>
    public partial class HealthCheckControl : UserControl
    {
        public HealthCheckControl()
        {
            InitializeComponent();
            DataContext = new HealthCheckControlViewModel();
        }
    }
}
