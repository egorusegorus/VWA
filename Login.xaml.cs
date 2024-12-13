using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
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
using System.Security;
using MySqlConnector;
using NPoco;



namespace VWA
{
	/// <summary>
	/// Interaktionslogik für Login.xaml
	/// </summary>
	public partial class Login : UserControl
	{
		SecureString secure;
		private MainWindow _mainWindow;
		public Login(MainWindow mainWindow)
		{
			InitializeComponent();
			_mainWindow = mainWindow;
		}
		

		private void btnLogin_Click(object sender, RoutedEventArgs e)
		{
			FileEncription fileEncription = new FileEncription();
			string connectionString = fileEncription.DecryptFileToString("SConfig1E.txt", "YourPassword") +
									  fileEncription.DecryptFileToString("SConfig2E.txt", "YourPassword")
									  + txtUser.Text + ";"
									  + fileEncription.DecryptFileToString("SConfig3E.txt", "YourPassword")
									  + txtPassword.Password + ";"
									  + fileEncription.DecryptFileToString("SConfig4E.txt", "YourPassword")
									  + fileEncription.DecryptFileToString("SConfig5E.txt", "YourPassword");
			secure = fileEncription.ToSecureString(connectionString);
			ConnectToDB connectToDB = new ConnectToDB();
			//connectToDB.ConnectToDB1(connectionString);
			string Role =connectToDB.ShowRole(connectionString);
			connectionString=null;
			if(Role!=null) { 
				_mainWindow.Ausrüstungsmanagement.IsEnabled = true; 
				_mainWindow.role=Role;
				txtPassword.Password = "";
				txtUser.Text = "";
			}
		}

	}
}

/*
 CREATE DATABASE VWA;


CREATE ROLE Admin_A;
CREATE ROLE Admin_B;
CREATE ROLE Benutzer;


CREATE USER admin_a WITH PASSWORD 'strongpasswordA';
GRANT Admin_A TO admin_a;

CREATE USER admin_b WITH PASSWORD 'strongpasswordB';
GRANT Admin_B TO admin_b;

CREATE USER user_vwa WITH PASSWORD 'userpassword';
GRANT Benutzer TO user_vwa;

GRANT ALL PRIVILEGES ON DATABASE VWA TO Admin_A;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO Admin_A;


GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO Admin_B;


GRANT CREATE, CONNECT, TEMP ON DATABASE VWA TO Admin_B;
REVOKE CREATE ROLE, DROP ROLE ON DATABASE VWA FROM Admin_B;

GRANT CONNECT ON DATABASE VWA TO Benutzer;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO Benutzer;

-- Tworzenie tabel
CREATE TABLE public.example_table (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Przekazanie właściciela tabel Administratorowi Typu A
ALTER TABLE public.example_table OWNER TO Admin_A;

-- Zapewnienie ze nowe tabele dziedzicza odpowiednie uprawnienia
ALTER DEFAULT PRIVILEGES IN SCHEMA public
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO Admin_B;
ALTER DEFAULT PRIVILEGES IN SCHEMA public
GRANT SELECT ON TABLES TO Benutzer;


-- logowanie aktywnosci
ALTER SYSTEM SET log_statement = 'all';
SELECT pg_reload_conf();

 */

