using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VWA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
       
    {
        // Login login = new Login();
        public string role;
        public MainWindow()
        {
            InitializeComponent();
            ContentPlaceholder.Content = new Login(this);

        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            
        }

		private void Ausrüstungsmanagement_Click(object sender, RoutedEventArgs e)
		{
            if (role.Equals("admin_a_role") || role.Equals("admin_a_role")) 
            { }
            else if (role.Equals("benutzer_role")) 
            { }
		}
	}
}