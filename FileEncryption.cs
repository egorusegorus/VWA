using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
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
        // string ConnectionString { get; init; } = ConfigurationManager.ConnectionStrings["db"].ConnectionString;

        public Login()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string User = txtUser.Text;
            string Password = txtPassword.Password;
            string fileContent = ReadEmbeddedTextFile("VWA.SConfig.txt");
            bottomPanel.Content = fileContent;
        }
        private string ReadEmbeddedTextFile(string resourceName)
        {
            string content = string.Empty;

            // Uzyskiwanie strumienia dla zasobu osadzonego
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        content = reader.ReadToEnd(); // Wczytanie całego tekstu do zmiennej
                    }
                }
                else
                {
                    content = "Zasób nie został znaleziony."; // Obsługa przypadku, gdy zasób nie istnieje
                }
            }

            return content;
        }

    }

    public class FileEncryption
    {
        // Funkcja do szyfrowania tekstu za pomocą AES
        public static string EncryptText(string plainText, string key, string iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key); // Klucz AES (16 bajtów dla AES-128)
                aesAlg.IV = Encoding.UTF8.GetBytes(iv);   // Wejściowy wektor inicjujący (IV)

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText); // Zapisz tekst do strumienia
                        }
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray()); // Zwróć zaszyfrowaną zawartość jako Base64
                }
            }
        }

        // Funkcja do deszyfrowania tekstu
        public static string DecryptText(string cipherText, string key, string iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key); // Klucz AES
                aesAlg.IV = Encoding.UTF8.GetBytes(iv);   // Wejściowy wektor inicjujący (IV)

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd(); // Odczytaj odszyfrowany tekst
                        }
                    }
                }
            }
        }
    }
}


