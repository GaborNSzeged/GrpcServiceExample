using System.Windows.Controls;

namespace GrpsOverAllExamplesClients.Ui.Authentication
{
    /// <summary>
    /// Interaction logic for AuthenticationControl.xaml
    /// </summary>
    public partial class AuthenticationControl : UserControl
    {
        public AuthenticationControl()
        {
            InitializeComponent();
            DataContext = new AuthenticationControlViewModel();
        }
    }
}
