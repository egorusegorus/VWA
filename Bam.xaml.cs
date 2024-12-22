using MySqlConnector;
using NPoco;
using System.Collections.Generic;
using System.Data;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace VWA
{
	/// <summary>
	/// Interaction logic for Bam.xaml
	/// </summary>
	public partial class Bam : UserControl
	{
		private SecureString secureString;
		private string FlagWoBinIch = "NoWhere";
		
		private List <Buchung> buchungList=new();
		public Bam(MainWindow mainWindow)
		{
			InitializeComponent();
			secureString = mainWindow.secureString;
			FillComboBox(ArtDerAusruestungCmbBox);

			ArtDerAusruestungCmbBox.SelectedIndex = 0;
			string query = "SELECT * FROM ausruestung WHERE ArtDerAusruestung LIKE @Value";
			LoadDataIntoDataGrid(query);
			BuchungsBeginnDatePicker.SelectedDate = DateTime.Now;
			BuchungsEndeDatePicker.SelectedDate = DateTime.Now;


		}

		public void FillComboBox(ComboBox comboBox)
		{
			FileEncription fileEncription = new FileEncription();
			try
			{
				string connectionString = fileEncription.ConvertToPlainString(secureString);
				using (var connection = new MySqlConnection(connectionString))
				{
					connection.Open();


					var database = new Database(connection);


					var query = "SELECT DISTINCT ArtDerAusruestung FROM ausruestung";
					var artDerAusruestungList = database.Fetch<string>(query);


					ArtDerAusruestungCmbBox.Items.Clear();
					ArtDerAusruestungCmbBox.ItemsSource = artDerAusruestungList;
					ArtDerAusruestungCmbBox.SelectedItem = 0;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error:" + ex);
			}
			ArtDerAusruestungCmbBox.SelectedItem = 0;
		}

		private void BuchungsEndeDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{

			LoadDataIntoDataGrid(BuchungsBeginnDatePicker, BuchungsEndeDatePicker);
		}


		private void LoadDataIntoDataGrid(string query)
		{
			try
			{

				FileEncription fileEncription = new FileEncription();
				string connectionString = fileEncription.ConvertToPlainString(secureString);


				string? Value = ArtDerAusruestungCmbBox.SelectedItem.ToString();

				using (var connection = new MySqlConnection(connectionString))
				{
					connectionString = null;
					connection.Open();


					var columnNames = new List<string>();
					using (var command = new MySqlCommand("SELECT * FROM ausruestung LIMIT 0", connection))
					using (var reader = command.ExecuteReader(CommandBehavior.SchemaOnly))
					{
						var schemaTable = reader.GetSchemaTable();
						foreach (DataRow row in schemaTable.Rows)
						{
							columnNames.Add(row["ColumnName"].ToString());
						}
					}


					var dataTable = new DataTable();
					using (var adapter = new MySqlDataAdapter(query, connection))
					{

						adapter.SelectCommand.Parameters.AddWithValue("@Value", $"%{Value}%");


						adapter.Fill(dataTable);
					}


					gridAusruestung.AutoGenerateColumns = false;


					gridAusruestung.Columns.Clear();


					foreach (var columnName in columnNames)
					{
						gridAusruestung.Columns.Add(new DataGridTextColumn
						{
							Header = columnName,
							Binding = new System.Windows.Data.Binding(columnName)
						});
					}


					gridAusruestung.ItemsSource = dataTable.DefaultView;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void LoadDataIntoDataGrid(DatePicker StartDatePicker, DatePicker EndeDatePicker)
		{
			try
			{

				FileEncription fileEncription = new FileEncription();
				string connectionString = fileEncription.ConvertToPlainString(secureString);


				string value = ArtDerAusruestungCmbBox.SelectedItem.ToString();
				DateTime startDate = StartDatePicker.SelectedDate ?? DateTime.MinValue;
				DateTime endDate = EndeDatePicker.SelectedDate ?? DateTime.MaxValue;

				using (var connection = new MySqlConnection(connectionString))
				{
					connectionString = null;
					connection.Open();


					var columnNames = new List<string>();
					using (var command = new MySqlCommand("SELECT * FROM ausruestung LIMIT 0", connection))
					using (var reader = command.ExecuteReader(CommandBehavior.SchemaOnly))
					{
						var schemaTable = reader.GetSchemaTable();
						foreach (DataRow row in schemaTable.Rows)
						{
							columnNames.Add(row["ColumnName"].ToString());
						}
					}


					string fullQuery = @"
                SELECT a.*
                FROM ausruestung a
                WHERE a.ArtDerAusruestung = @Value
                  AND NOT EXISTS (
                      SELECT 1
                      FROM buchung b
                      WHERE b.AusruestungId = a.Id
                        AND (
                            (b.Buchungsbeginn <= @EndDate AND b.Buchungsende >= @StartDate)
                        )
                  )
                ORDER BY a.ArtDerAusruestung";


					var dataTable = new DataTable();
					using (var adapter = new MySqlDataAdapter(fullQuery, connection))
					{

						adapter.SelectCommand.Parameters.AddWithValue("@Value", value);
						adapter.SelectCommand.Parameters.AddWithValue("@StartDate", startDate);
						adapter.SelectCommand.Parameters.AddWithValue("@EndDate", endDate);


						adapter.Fill(dataTable);
					}


					gridAusruestung.AutoGenerateColumns = false;


					gridAusruestung.Columns.Clear();


					foreach (var columnName in columnNames)
					{
						gridAusruestung.Columns.Add(new DataGridTextColumn
						{
							Header = columnName,
							Binding = new System.Windows.Data.Binding(columnName)
						});
					}


					gridAusruestung.ItemsSource = dataTable.DefaultView;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void ArtDerAusruestungCmbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

			LoadDataIntoDataGrid(BuchungsBeginnDatePicker, BuchungsEndeDatePicker);
		}

		private void gridAusruestung_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (FlagWoBinIch.Equals("Meine Buchungen"))
			{

				if (gridAusruestung.SelectedItem != null)
				{

					btnBuchungLöschen.IsEnabled = true;
					VerlengerungDatenPicker.IsEnabled = true;



				}
				else
				{

					btnBuchen.IsEnabled = false;
				}

			}
			else
			if (gridAusruestung.SelectedItem != null)
			{

				btnBuchen.IsEnabled = true;



			}
			else
			{

				btnBuchen.IsEnabled = false;
			}
		}

		
		private void BuchungsBeginnDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			LoadDataIntoDataGrid(BuchungsBeginnDatePicker, BuchungsEndeDatePicker);
		}

		private int BenutzerId()
		{
			int benutzerid = 0;
			FileEncription fileEncription = new FileEncription();
			string connectionString = fileEncription.ConvertToPlainString(secureString);

			using (var connection = new MySqlConnection(connectionString))
			{
				connection.Open();


				string currentUserQuery = "SELECT CURRENT_USER();";

				using (var command = new MySqlCommand(currentUserQuery, connection))
				{
					string currentUser = command.ExecuteScalar()?.ToString();

					if (!string.IsNullOrEmpty(currentUser))
					{

						string username = currentUser.Split('@')[0];


						string fullQuery = "SELECT ID FROM benutzer WHERE Benutzername = @Benutzername LIMIT 1;";

						using (var userIdCommand = new MySqlCommand(fullQuery, connection))
						{

							userIdCommand.Parameters.AddWithValue("@Benutzername", username);

							var result = userIdCommand.ExecuteScalar();


							if (result != null)
							{
								benutzerid = Convert.ToInt32(result);
							}

						}
					}

				}
			}

			return benutzerid;
		}


		private void btnBuchen_Click(object sender, RoutedEventArgs e)
		{
			FileEncription fileEncription = new FileEncription();
			string connectionString = fileEncription.ConvertToPlainString(secureString);

			using (var connection = new MySqlConnection(connectionString))
			{
				connection.Open();

				// Benutzer-ID abrufen
				var benutzerID = BenutzerId();

				// Benutzer-E-Mail abrufen
				string benutzerEmail = string.Empty;

				string getUserEmailQuery = "SELECT Email FROM benutzer WHERE ID = @BenutzerID";
				using (var emailCommand = new MySqlCommand(getUserEmailQuery, connection))
				{
					emailCommand.Parameters.AddWithValue("@BenutzerID", benutzerID);

					var emailResult = emailCommand.ExecuteScalar();
					if (emailResult != null)
					{
						benutzerEmail = emailResult.ToString();
					}
					else
					{
						MessageBox.Show("Es wurde keine E-Mail für den angegebenen Benutzer gefunden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}
				}

				// Überprüfen, ob der Benutzer existiert
				string checkUserQuery = "SELECT COUNT(*) FROM benutzer WHERE ID = @BenutzerID";

				using (var checkUserCommand = new MySqlCommand(checkUserQuery, connection))
				{
					checkUserCommand.Parameters.AddWithValue("@BenutzerID", benutzerID);

					int userCount = Convert.ToInt32(checkUserCommand.ExecuteScalar());

					if (userCount == 0)
					{
						MessageBox.Show($"Der Benutzer mit der ID ({benutzerID}) existiert nicht.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}
				}

				// Überprüfen, ob ein Element im gridAusruestung ausgewählt wurde
				if (gridAusruestung.SelectedItem != null)
				{
					var selectedItem = gridAusruestung.SelectedItem;

					// ID der ausgewählten Ausrüstung abrufen
					int ausruestungID = 0;

					if (selectedItem is DataRowView row)
					{
						ausruestungID = Convert.ToInt32(row["ID"]);
					}
					else
					{
						MessageBox.Show("Bitte wählen Sie ein Element aus der Liste aus.", "Warnung", MessageBoxButton.OK, MessageBoxImage.Warning);
						return;
					}

					// Neue Buchung erstellen
					var buchung = new Buchung
					{
						BenutzerID = benutzerID,
						AusruestungID = ausruestungID,
						Buchungsbeginn = BuchungsBeginnDatePicker.SelectedDate.Value,
						Buchungsende = BuchungsEndeDatePicker.SelectedDate.Value,
						Erstellungsdatum_der_Buchung = DateTime.Now
					};

					// SQL-Befehl zum Einfügen einer neuen Buchung in die Datenbank
					string query = @"
                INSERT INTO Buchung (BenutzerID, AusruestungID, Buchungsbeginn, Buchungsende, Erstellungsdatum_der_Buchung, Geaendert_am, Geaendert_von)
                VALUES (@BenutzerID, @AusruestungID, @Buchungsbeginn, @Buchungsende, @Erstellungsdatum_der_Buchung, @Geaendert_am, @Geaendert_von)";

					using (var command = new MySqlCommand(query, connection))
					{
						command.Parameters.AddWithValue("@BenutzerID", buchung.BenutzerID);
						command.Parameters.AddWithValue("@AusruestungID", buchung.AusruestungID);
						command.Parameters.AddWithValue("@Buchungsbeginn", buchung.Buchungsbeginn);
						command.Parameters.AddWithValue("@Buchungsende", buchung.Buchungsende);
						command.Parameters.AddWithValue("@Erstellungsdatum_der_Buchung", buchung.Erstellungsdatum_der_Buchung);


						command.Parameters.AddWithValue("@Geaendert_am", DBNull.Value);
						command.Parameters.AddWithValue("@Geaendert_von", DBNull.Value);

						command.ExecuteNonQuery();
					}


					MessageBox.Show($"Die Buchung wurde erfolgreich gespeichert für den Benutzer mit der E-Mail: {benutzerEmail}", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				else
				{
					MessageBox.Show("Bitte wählen Sie ein Element aus der Liste aus.", "Warnung", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}


			LoadDataIntoDataGrid(BuchungsBeginnDatePicker, BuchungsEndeDatePicker);
		}

		private void btnMeineBuchungen_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				FileEncription fileEncription = new FileEncription();
				string connectionString = fileEncription.ConvertToPlainString(secureString);

				// Pobierz ID aktualnego użytkownika
				int benutzerID = BenutzerId();

				using (var connection = new MySqlConnection(connectionString))
				{
					connectionString = null;
					connection.Open();
					
					

					// Pobierz nazwy kolumn z tabeli wynikowej (łącznie z danymi z tabeli 'ausruestung')
					var columnNames = new List<string>
			{
				"ArtDerAusruestung",
				"Marke",
				"Model",
				"Beschreibung",
				"Zustand",
				"BuchungID",
				"BenutzerID",
				"Buchungsbeginn",
				"Buchungsende",
				"Erstellungsdatum_der_Buchung",
				"Geaendert_am",
				"Geaendert_von"
			};

					// Zapytanie SQL z połączeniem tabel (JOIN)
					string query = @"
                SELECT 
                    a.ArtDerAusruestung,
                    a.Marke,
                    a.Model,
                    a.Beschreibung,
                    a.Zustand,
                    b.ID AS BuchungID,
                    b.BenutzerID,
                    b.Buchungsbeginn,
                    b.Buchungsende,
                    b.Erstellungsdatum_der_Buchung,
                    b.Geaendert_am,
                    b.Geaendert_von
                FROM 
                    buchung b
                JOIN 
                    ausruestung a
                ON 
                    b.AusruestungID = a.ID
                WHERE 
                    b.BenutzerID = @BenutzerID";

					// Utwórz DataTable i załaduj dane
					var dataTable = new DataTable();
					using (var adapter = new MySqlDataAdapter(query, connection))
					{
						adapter.SelectCommand.Parameters.AddWithValue("@BenutzerID", benutzerID);
						adapter.Fill(dataTable);
					}

					// Wyłącz automatyczne generowanie kolumn
					gridAusruestung.AutoGenerateColumns = false;

					// Wyczyść istniejące kolumny
					gridAusruestung.Columns.Clear();

					// Dodaj nowe kolumny do DataGrid na podstawie listy columnNames
					foreach (var columnName in columnNames)
					{
						gridAusruestung.Columns.Add(new DataGridTextColumn
						{
							Header = columnName,
							Binding = new System.Windows.Data.Binding(columnName)
						});
					}

					// Przypisz dane do DataGrid
					gridAusruestung.ItemsSource = dataTable.DefaultView;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			FlagWoBinIch = "Meine Buchungen";
		}
		
		private void LoadMeineBuchungen()
		{
			try
			{
				FileEncription fileEncription = new FileEncription();
				string connectionString = fileEncription.ConvertToPlainString(secureString);

				// Pobierz ID aktualnego użytkownika
				int benutzerID = BenutzerId();

				using (var connection = new MySqlConnection(connectionString))
				{
					connectionString = null;
					connection.Open();

					/*MySqlCommand cmd = connection.CreateCommand();
					cmd.CommandText = "Delete..."; //sql
					cmd.ExecuteNonQuery();*/

					// Pobierz nazwy kolumn z tabeli wynikowej (łącznie z danymi z tabeli 'ausruestung')
					var columnNames = new List<string>
			{
				"ArtDerAusruestung",
				"Marke",
				"Model",
				"Beschreibung",
				"Zustand",
				"BuchungID",
				"BenutzerID",
				"Buchungsbeginn",
				"Buchungsende",
				"Erstellungsdatum_der_Buchung",
				"Geaendert_am",
				"Geaendert_von"
			};

					// Zapytanie SQL z połączeniem tabel (JOIN)
					string query = @"
                SELECT 
                    a.ArtDerAusruestung,
                    a.Marke,
                    a.Model,
                    a.Beschreibung,
                    a.Zustand,
                    b.ID AS BuchungID,
                    b.BenutzerID,
                    b.Buchungsbeginn,
                    b.Buchungsende,
                    b.Erstellungsdatum_der_Buchung,
                    b.Geaendert_am,
                    b.Geaendert_von
                FROM 
                    buchung b
                JOIN 
                    ausruestung a
                ON 
                    b.AusruestungID = a.ID
                WHERE 
                    b.BenutzerID = @BenutzerID";

					// Utwórz DataTable i załaduj dane
					var dataTable = new DataTable();
					using (var adapter = new MySqlDataAdapter(query, connection))
					{
						adapter.SelectCommand.Parameters.AddWithValue("@BenutzerID", benutzerID);
						adapter.Fill(dataTable);
					}

					// Wyłącz automatyczne generowanie kolumn
					gridAusruestung.AutoGenerateColumns = false;

					// Wyczyść istniejące kolumny
					gridAusruestung.Columns.Clear();

					// Dodaj nowe kolumny do DataGrid na podstawie listy columnNames
					foreach (var columnName in columnNames)
					{
						gridAusruestung.Columns.Add(new DataGridTextColumn
						{
							Header = columnName,
							Binding = new System.Windows.Data.Binding(columnName)
						});
					}

					// Przypisz dane do DataGrid
					gridAusruestung.ItemsSource = dataTable.DefaultView;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			FlagWoBinIch = "Meine Buchungen";
		}

		private void VerlengerungDatenPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			btnVerlenger.IsEnabled = true;
			btnBuchungLöschen.IsEnabled = false;
		}

		private void btnBuchungLöschen_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				FileEncription fileEncription = new FileEncription();
				string connectionString = fileEncription.ConvertToPlainString(secureString);

				
				int id = 0;
				if (gridAusruestung.SelectedItem is DataRowView rowView)
				{
					id = Convert.ToInt32(rowView["BuchungID"]);
					
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
						cmd.CommandText = "DELETE FROM buchung WHERE ID = @id";
						cmd.Parameters.AddWithValue("@id", id);
						cmd.ExecuteNonQuery();
					}
				}

				MessageBox.Show("Buchung storniert!", "Gut gemacht", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Fehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			btnBuchungLöschen.IsEnabled = false;
			VerlengerungDatenPicker.IsEnabled = false;
			LoadMeineBuchungen();
		} //btnDelete

		private void btnVerlenger_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				FileEncription fileEncription = new FileEncription();
				string connectionString = fileEncription.ConvertToPlainString(secureString);


				int id = 0;
				if (gridAusruestung.SelectedItem is DataRowView rowView)
				{
					id = Convert.ToInt32(rowView["BuchungID"]);

				}
				else
				{
					MessageBox.Show("Bitte Datensatz wählen!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
				DateTime dateTime = VerlengerungDatenPicker.SelectedDate.Value;
				using (var connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					using (var cmd = connection.CreateCommand())
					{
						cmd.CommandText = @"
						  UPDATE buchung 
						SET 
						Buchungsende = @dateTime,
						Geaendert_am = NOW(),
						Geaendert_Von = USER()
						WHERE ID = @id";
						cmd.Parameters.AddWithValue("@dateTime", dateTime);
						cmd.Parameters.AddWithValue("@id", id);
						cmd.ExecuteNonQuery();
					}

				}

				MessageBox.Show("Buchung geändert!", "Gut gemacht", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Fehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			btnBuchungLöschen.IsEnabled = false;
			VerlengerungDatenPicker.IsEnabled = false;
			btnVerlenger.IsEnabled = false;
			LoadMeineBuchungen();
		}
	} // class
}//namespace





