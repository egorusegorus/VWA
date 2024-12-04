using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace VWA
{
	/// <summary>
	/// Interaktionslogik für Login.xaml
	/// </summary>
	public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent();
        }

		private void btnLogin_Click(object sender, RoutedEventArgs e)
		{
			string User = txtUser.Text;
			string Password = txtPassword.Password;
			LoadTxt loadTxt = new LoadTxt();
			string fileContent = loadTxt.ReadEmbeddedTextFile("VWA.SConfig.txt");
			fileContent=fileContent+";"+ "Uid ="+User+";"+"Pwd ="+Password;
			bottomPanel.Content = fileContent;
		}
	}
	
	}


