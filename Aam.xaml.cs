﻿using MySqlConnector;
using NPoco;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for Aam.xaml
    /// </summary>
    public partial class Aam : UserControl
    {
		private SecureString secureString;
		public Aam(MainWindow mainWindow)
        {
            InitializeComponent();
            secureString= mainWindow.secureString;
			LoadDate();
			

		}

		public void LoadDate()
		{
			try
			{
				FileEncription fileEncription = new FileEncription();
				string connectionString = fileEncription.ConvertToPlainString(secureString);
				
				using (var connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					

					// Zapytanie SQL, które pobiera wszystkie dane z tabeli benutzer
					string query = @"
                SELECT 
                    ID,
                    Nachname,
                    Vorname,
                    Email,
                    Benutzername,
					Geaendert_Von,
					Geaendert_Am
                FROM 
                    benutzer";

					// Utwórz DataTable i załaduj dane
					var dataTable = new DataTable();
					using (var adapter = new MySqlDataAdapter(query, connection))
					{
						adapter.Fill(dataTable);
					}

					// Wyłącz automatyczne generowanie kolumn
					gridBenutzer.AutoGenerateColumns = true;

					// Przypisz dane do DataGrid
					gridBenutzer.ItemsSource = dataTable.DefaultView;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		private List<Benutzer> selectedBenutzers = new List<Benutzer>();


		private void SafeInDB(SecureString secureString, List<Benutzer> selectedBenutzers)
		{
			try
			{
				// Upewniamy się, że connection string jest poprawny
				FileEncription fileEncription = new FileEncription();
				string connectionString = fileEncription.ConvertToPlainString(secureString);
				string temp = connectionString;
				string pattern = @"Uid=(.*?);";
				Match match = Regex.Match(temp, pattern);

				if (match.Success)
				{
					string value = match.Groups[1].Value;
					value = value.Trim();
					temp = value;
				}
				if (string.IsNullOrWhiteSpace(connectionString))
				{
					MessageBox.Show("Connection string jest pusty!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				// Sprawdzamy, czy lista selectedBenutzers nie jest pusta
				if (selectedBenutzers == null || !selectedBenutzers.Any())
				{
					MessageBox.Show("Lista Benutzer jest pusta!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				using (var connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					Console.WriteLine("Połączono z bazą danych.");

					foreach (var benutzer in selectedBenutzers)
					{
						// Walidacja danych
						if (benutzer.ID == 0)
						{
							Console.WriteLine($"Benutzer o ID 0 został pominięty (nie można zaktualizować rekordu bez ID).");
							continue;
						}

						// Przypisanie danych z GUI do obiektów w liście, ale nie zmieniając ID
						benutzer.Nachname = txtBoxNachname.Text;
						benutzer.Vorname = txtBoxVorname.Text;
						benutzer.Email = txtBoxEmail.Text;
						benutzer.Benutzername = txtBoxBenutzername.Text;

						// Ustawienie wartości Geaendert_Von i Geaendert_Am
						benutzer.Geaendert_Von = temp; // Domyślny użytkownik
						benutzer.Geaendert_Am = DateTime.Now;

						// Zapytanie SQL do aktualizacji
						string query = @"
                UPDATE benutzer
                SET 
                    Nachname = @Nachname,
                    Vorname = @Vorname,
                    Email = @Email,
                    Benutzername = @Benutzername,
                    Geaendert_Von = @Geaendert_Von,
                    Geaendert_Am = @Geaendert_Am
                WHERE 
                    ID = @Id;";

						try
						{
							using (var cmd = new MySqlCommand(query, connection))
							{
								cmd.Parameters.AddWithValue("@Id", benutzer.ID);
								cmd.Parameters.AddWithValue("@Nachname", benutzer.Nachname);
								cmd.Parameters.AddWithValue("@Vorname", benutzer.Vorname);
								cmd.Parameters.AddWithValue("@Email", benutzer.Email);
								cmd.Parameters.AddWithValue("@Benutzername", benutzer.Benutzername);
								cmd.Parameters.AddWithValue("@Geaendert_Von", benutzer.Geaendert_Von);
								cmd.Parameters.AddWithValue("@Geaendert_Am", benutzer.Geaendert_Am);

								// Wykonanie zapytania SQL
								int rowsAffected = cmd.ExecuteNonQuery();

								if (rowsAffected > 0)
								{
									Console.WriteLine($"Benutzer o ID {benutzer.ID} został pomyślnie zapisany.");
								}
								else
								{
									Console.WriteLine($"Nie znaleziono rekordu o ID {benutzer.ID}.");
								}
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Błąd podczas zapisywania Benutzer o ID {benutzer.ID}: {ex.Message}");
						}
					}
				}

				// Komunikat o sukcesie
				//MessageBox.Show("Wszystkie zmiany zostały zapisane!", "Gut gemacht", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				// Obsługa błędów
				MessageBox.Show($"Wystąpił błąd podczas zapisywania danych: {ex.Message}\n{ex.StackTrace}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			txtBoxBenutzername.Text = null;
			txtBoxEmail.Text = null;
			txtBoxNachname.Text = null;
			txtBoxVorname.Text = null;
			PasswordBox.Password = null;
			btnBenutzerHinZuFuegen.IsEnabled = false;
			btnBenutzerLöschen.IsEnabled = false;
			btnBenutzerBearbeiten.IsEnabled = false;
			LoadDate();
		
		}

		private void SafeInFelder()
		{
			// Sprawdź, czy wiersz jest zaznaczony
			if (gridBenutzer.SelectedItem != null)
			{
				// Jeśli element w DataGrid jest typu DataRowView
				if (gridBenutzer.SelectedItem is DataRowView rowView)
				{
					// Pobierz dane z wiersza, aby przypisać je do obiektu typu Benutzer
					var selectedBenutzer = new Benutzer
					{
						ID = rowView["ID"] != DBNull.Value ? Convert.ToInt32(rowView["ID"]) : 0,
						Nachname = rowView["Nachname"] != DBNull.Value ? rowView["Nachname"].ToString() : string.Empty,
						Vorname = rowView["Vorname"] != DBNull.Value ? rowView["Vorname"].ToString() : string.Empty,
						Email = rowView["Email"] != DBNull.Value ? rowView["Email"].ToString() : string.Empty,
						Benutzername = rowView["Benutzername"] != DBNull.Value ? rowView["Benutzername"].ToString() : string.Empty,
						Geaendert_Von = rowView["Geaendert_Von"] != DBNull.Value ? rowView["Geaendert_Von"].ToString() : string.Empty,
						Geaendert_Am = rowView["Geaendert_Am"] != DBNull.Value ? Convert.ToDateTime(rowView["Geaendert_Am"]) : DateTime.MinValue
					};

					// Dodaj zaznaczonego użytkownika do listy
					selectedBenutzers.Clear(); // Czyszczenie poprzednich zaznaczeń
					selectedBenutzers.Add(selectedBenutzer);

					// Przypisanie danych do kontrolek TextBox
					txtBoxNachname.Text = selectedBenutzer.Nachname;
					txtBoxVorname.Text = selectedBenutzer.Vorname;
					txtBoxEmail.Text = selectedBenutzer.Email;
					txtBoxBenutzername.Text = selectedBenutzer.Benutzername;  // Przypisanie użytkownika do TextBox
				}
			}
			else
			{
				// Jeśli nie ma zaznaczonego wiersza, wyczyść listę
				selectedBenutzers.Clear();
			}
		}

		private void gridBenutzer_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (gridBenutzer.SelectedItem != null)
			{

				btnBenutzerLöschen.IsEnabled = true;
				btnBenutzerBearbeiten.IsEnabled = true;
				btnBenutzerHinZuFuegen.IsEnabled = false;
			}
			SafeInFelder();
				
				

				
		}

		private void btnBenutzerZeigen_Click(object sender, RoutedEventArgs e)
		{
			LoadDate();
			btnBenutzerLöschen.IsEnabled = false;
			btnBenutzerBearbeiten.IsEnabled = false;
			btnBenutzerHinZuFuegen.IsEnabled = false;
			txtBoxEmail.Text=null;
			txtBoxBenutzername.Text=null;
			txtBoxNachname.Text = null;
			txtBoxVorname.Text = null;
			PasswordBox.Password = "";

		}

		private void btnBenutzerLöschen_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				FileEncription fileEncription = new FileEncription();
				string connectionString = fileEncription.ConvertToPlainString(secureString);


				int id = 0;
				if (gridBenutzer.SelectedItem is DataRowView rowView)
				{
					id = Convert.ToInt32(rowView["ID"]);

				}
				else
				{
					MessageBox.Show("Bitte Datensatz wählen!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				using (var connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					using (var cmd = connection.CreateCommand())
					{
						// Poprawne zapytanie SQL
						cmd.CommandText = "DELETE FROM bENUTZER WHERE ID = @id";
						cmd.Parameters.AddWithValue("@id", id);
						cmd.ExecuteNonQuery();
					}
					btnBenutzerLöschen.IsEnabled = false;

					btnBenutzerBearbeiten.IsEnabled = false;
					LoadDate();
				}
			}
			catch (Exception ex) { }
			}

		private void btnBenutzerBearbeiten_Click(object sender, RoutedEventArgs e)
		{
			SafeInDB(secureString,selectedBenutzers);

		}
		void EnableHinzufuegen()
		{// Sprawdzamy, czy wszystkie pola tekstowe są wypełnione oraz czy PasswordBox nie jest pusty
			if (!string.IsNullOrWhiteSpace(txtBoxBenutzername.Text) &&
				!string.IsNullOrWhiteSpace(txtBoxEmail.Text) &&
				!string.IsNullOrWhiteSpace(txtBoxNachname.Text) &&
				!string.IsNullOrWhiteSpace(txtBoxVorname.Text) &&
				!string.IsNullOrWhiteSpace(PasswordBox.Password))
			{
				btnBenutzerHinZuFuegen.IsEnabled = true;
			}
			else
			{
				btnBenutzerHinZuFuegen.IsEnabled = false;
			}
		}

		private void txtBoxNachname_TextChanged(object sender, TextChangedEventArgs e)
		{
			EnableHinzufuegen();
		}

		private void txtBoxVorname_TextChanged(object sender, TextChangedEventArgs e)
		{
			EnableHinzufuegen();
		}

		private void txtBoxBenutzername_TextChanged(object sender, TextChangedEventArgs e)
		{
			EnableHinzufuegen();
		}

		private void txtBoxEmail_TextChanged(object sender, TextChangedEventArgs e)
		{
			EnableHinzufuegen();
		}

		private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
		{
			EnableHinzufuegen();
		}

        private void btnBenutzerHinZuFuegen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileEncription fileEncription = new FileEncription();
                string connectionString = fileEncription.ConvertToPlainString(secureString);

                string benutzername = txtBoxBenutzername.Text.Trim();
                string email = txtBoxEmail.Text.Trim();
                string nachname = txtBoxNachname.Text.Trim();
                string vorname = txtBoxVorname.Text.Trim();
                string password = PasswordBox.Password.Trim();
                string geaendert_von = "system";
                DateTime dateTime = DateTime.Now;

                if (string.IsNullOrWhiteSpace(benutzername) || string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(nachname) || string.IsNullOrWhiteSpace(vorname) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Alle Felder müssen ausgefüllt sein!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                InsertUserToDatabase(connectionString, benutzername, nachname, vorname, email, geaendert_von, dateTime);
                CreateMysqlUser(connectionString, benutzername, password);
                GrantPermissions(connectionString, benutzername);

                MessageBox.Show("Benutzer erfolgreich hinzugefügt!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler: {ex.Message}\n{ex.StackTrace}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                Clipboard.SetText(ex.ToString());
            }

            ClearFields();
            btnBenutzerHinZuFuegen.IsEnabled = true;
        }

        private void InsertUserToDatabase(string connectionString, string benutzername, string nachname, string vorname, string email, string geaendert_von, DateTime geaendert_am)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO benutzer (Benutzername, Nachname, Vorname, Email, Geaendert_Von, Geaendert_Am)
                         VALUES (@Benutzername, @Nachname, @Vorname, @Email, @Geaendert_Von, @Geaendert_Am);";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Benutzername", benutzername);
                    cmd.Parameters.AddWithValue("@Nachname", nachname);
                    cmd.Parameters.AddWithValue("@Vorname", vorname);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Geaendert_Von", geaendert_von);
                    cmd.Parameters.AddWithValue("@Geaendert_Am", geaendert_am);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void CreateMysqlUser(string connectionString, string benutzername, string password)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "CREATE USER @Benutzername@localhost IDENTIFIED BY @Password;";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Benutzername", benutzername);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void GrantPermissions(string connectionString, string benutzername)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
            GRANT SELECT, INSERT, UPDATE, DELETE ON VWA.* TO @Benutzername@localhost;
            GRANT benutzer_role TO @Benutzername@localhost;";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Benutzername", benutzername);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void ClearFields()
        {
            txtBoxBenutzername.Clear();
            txtBoxEmail.Clear();
            txtBoxNachname.Clear();
            txtBoxVorname.Clear();
            PasswordBox.Clear();
        }

    }
}
	
	

