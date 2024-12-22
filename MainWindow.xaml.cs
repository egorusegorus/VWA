using System.Security;
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
        public SecureString secureString;
        public MainWindow()
        {
            InitializeComponent();
            ContentPlaceholder.Content = new Login(this);
            

        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
			ContentPlaceholder.Content = new Login(this);
		}

		private void Ausruestungsmanagement_Click(object sender, RoutedEventArgs e)
		{
			
            role=role.Trim();
			if (role != null) {



                if (role.Equals("admin_b_role") || role.Equals("admin_a_role"))
                {
					ContentPlaceholder.Content = new Aam(this);
					ContentPlaceholder.InvalidateVisual();
				}else
                if (role== "benutzer_role") 
            {
                    ContentPlaceholder.Content = new Bam(this);
					ContentPlaceholder.InvalidateVisual();
				}
				/*if (secureString != null) {
					FileEncription fileEncription = new FileEncription();
                    string severdata= fileEncription.ConvertToPlainString(secureString);
                    MessageBox.Show(severdata);
				}*/
			}
		}
	}
}