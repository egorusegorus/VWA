using MySqlConnector;
using NPoco;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
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
            secureString = mainWindow.secureString;
            LoadDate();
            LoadAusruestung();
            LoadBuchungen();



        }
        public void LoadBuchungen()
        {
            try
            {
                FileEncription fileEncription = new FileEncription();
                string connectionString = fileEncription.ConvertToPlainString(secureString);

                using (var connection = new MySqlConnection(connectionString))
                {
                    connectionString = null;
                    connection.Open();

                   
                    var columnNames = new List<string>
            {
                "ArtDerAusruestung",
                "Marke",
                "Model",
                "Beschreibung",
                "Zustand",
                "BuchungID",
                "Email",
                "Buchungsbeginn",
                "Buchungsende",
                "Erstellungsdatum_der_Buchung",
                "Geaendert_am",
                "Geaendert_von"
            };

                    
                    string query = @"
                SELECT 
                    a.ArtDerAusruestung,
                    a.Marke,
                    a.Model,
                    a.Beschreibung,
                    a.Zustand,
                    b.ID AS BuchungID,
                    u.Email, -- Pobieramy adres email z tabeli benutzer
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
                JOIN 
                    benutzer u
                ON 
                    b.BenutzerID = u.ID;"; 

              
                    var dataTable = new DataTable();
                    using (var adapter = new MySqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }

                    
                    B_DataGrid.AutoGenerateColumns = false;

                   
                    B_DataGrid.Columns.Clear();

                    
                    foreach (var columnName in columnNames)
                    {
                        B_DataGrid.Columns.Add(new DataGridTextColumn
                        {
                            Header = columnName,
                            Binding = new System.Windows.Data.Binding(columnName)
                        });
                    }

                    
                    B_DataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

                    
                    var dataTable = new DataTable();
                    using (var adapter = new MySqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }

                   
                    gridBenutzer.AutoGenerateColumns = true;

          
                    gridBenutzer.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private List<Benutzer> selectedBenutzers = new List<Benutzer>();
        private List<Ausruestung> selectedAusruestung = new List<Ausruestung>();
        private List<Buchung> selectedBuchung = new List<Buchung>();

        private void SafeInDB(SecureString secureString, List<Benutzer> selectedBenutzers)
        {
            try
            {
               
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
                
                        if (benutzer.ID == 0)
                        {
                            Console.WriteLine($"Benutzer o ID 0 został pominięty (nie można zaktualizować rekordu bez ID).");
                            continue;
                        }

                        benutzer.Nachname = txtBoxNachname.Text;
                        benutzer.Vorname = txtBoxVorname.Text;
                        benutzer.Email = txtBoxEmail.Text;
                        benutzer.Benutzername = txtBoxBenutzername.Text;

                        
                        benutzer.Geaendert_Von = temp; 
                        benutzer.Geaendert_Am = DateTime.Now;

                      
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

           }
            catch (Exception ex)
            {
               
                MessageBox.Show($"Fehler: {ex.Message}\n{ex.StackTrace}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
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
        private void SafeInDB_A(SecureString secureString, List<Ausruestung> selectedAusruestung)
        {
            try
            {
      
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

             
                if (selectedAusruestung == null || !selectedAusruestung.Any())
                {
                    MessageBox.Show("Fehler!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("Połączono z bazą danych.");

                    foreach (var ausruestung in selectedAusruestung)
                    {
                        // Walidacja danych
                        if (ausruestung.ID == 0)
                        {
                            Console.WriteLine($"Ausruestung o ID 0 został pominięty (nie można zaktualizować rekordu bez ID).");
                            continue;
                        }

                  
                        ausruestung.ArtDerAusruestung = txtArtDerAusreustung.Text;
                        ausruestung.Marke = txtMarke.Text;
                        ausruestung.Model = txtModel.Text;
                        ausruestung.Zustand = txtZustand.Text;
                        ausruestung.Beschreibung = txtBeschreibung.Text;
                        ausruestung.Geaendert_Von = temp;
                        ausruestung.Geaendert_Am = DateTime.Now;

                       
                        string query = @"
                UPDATE ausruestung
                SET 
                    ArtDerAusruestung = @artDerAusruestung,
                    Marke = @marke,
                    Model = @model,
                    Zustand = @zustand,
                    Beschreibung = @beschreibung,
                    Geaendert_Von = @Geaendert_Von,
                    Geaendert_Am = @Geaendert_Am
                WHERE 
                    ID = @Id;";

                        try
                        {
                            using (var cmd = new MySqlCommand(query, connection))
                            {
                                cmd.Parameters.AddWithValue("@Id", ausruestung.ID);
                                cmd.Parameters.AddWithValue("@artDerAusruestung", ausruestung.ArtDerAusruestung);
                                cmd.Parameters.AddWithValue("@marke", ausruestung.Marke);
                                cmd.Parameters.AddWithValue("@model", ausruestung.Model);
                                cmd.Parameters.AddWithValue("@zustand", ausruestung.Zustand);
                                cmd.Parameters.AddWithValue("@beschreibung", ausruestung.Beschreibung);
                                cmd.Parameters.AddWithValue("@Geaendert_Von", ausruestung.Geaendert_Von);
                                cmd.Parameters.AddWithValue("@Geaendert_Am", ausruestung.Geaendert_Am);

                            
                                int rowsAffected = cmd.ExecuteNonQuery();

                               
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                    }
                }

                // Komunikat o sukcesie
                MessageBox.Show("Well done", "Gut gemacht", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                
                MessageBox.Show($"Error: {ex.Message}\n{ex.StackTrace}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            txtZustand.Text = null;
            txtBeschreibung.Text = null;
            txtArtDerAusreustung.Text = null;
            txtMarke.Text = null;
            txtModel.Text = null;
            A_Hinzufuegen.IsEnabled = false;
            A_loeschen.IsEnabled = false;
            A_Speichern.IsEnabled = false;
            LoadAusruestung();
        }
       
        private void SafeInFelder()
        {
         
            if (gridBenutzer.SelectedItem != null)
            {
               
                if (gridBenutzer.SelectedItem is DataRowView rowView)
                {
                  
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

                    
                    selectedBenutzers.Clear(); 
                    selectedBenutzers.Add(selectedBenutzer);

                    
                    txtBoxNachname.Text = selectedBenutzer.Nachname;
                    txtBoxVorname.Text = selectedBenutzer.Vorname;
                    txtBoxEmail.Text = selectedBenutzer.Email;
                    txtBoxBenutzername.Text = selectedBenutzer.Benutzername;
                }
            }
            else
            {
               
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
            txtBoxEmail.Text = null;
            txtBoxBenutzername.Text = null;
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
            SafeInDB(secureString, selectedBenutzers);

        }
        void EnableHinzufuegen()
        {
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

        private void InsertAusruestungToDatabase(string connectionString, Ausruestung ausruestung)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO ausruestung (ArtDerAusruestung, Marke, Model, Zustand, Beschreibung, Geaendert_Von, Geaendert_Am)
                         VALUES (@artDerAusruestung, @marke, @model, @zustand, @beschreibung, @Geaendert_Von, @Geaendert_Am);";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@artDerAusruestung", ausruestung.ArtDerAusruestung);
                    cmd.Parameters.AddWithValue("@marke", ausruestung.Marke);
                    cmd.Parameters.AddWithValue("@model", ausruestung.Model);
                    cmd.Parameters.AddWithValue("@zustand", ausruestung.Zustand);
                    cmd.Parameters.AddWithValue("@beschreibung", ausruestung.Beschreibung);
                    cmd.Parameters.AddWithValue("@Geaendert_Von", ausruestung.Geaendert_Von);
                    cmd.Parameters.AddWithValue("@Geaendert_Am", ausruestung.Geaendert_Am);
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
            GRANT SELECT, INSERT, UPDATE, DELETE ON vwa.* TO @Benutzername@localhost;
            ";//GRANT benutzer_role TO @Benutzername@localhost;";
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadAusruestung();
            txtArtDerAusreustung.Clear();
            txtBeschreibung.Clear();
            txtMarke.Clear();
            txtModel.Clear();
            txtZustand.Clear();
            A_loeschen.IsEnabled = false;
            A_Speichern.IsEnabled = false;

        }
        private void LoadAusruestung()
        {
            try
            {
                FileEncription fileEncription = new FileEncription();
                string connectionString = fileEncription.ConvertToPlainString(secureString);
                A_Hinzufuegen.IsEnabled = true;
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();


                    
                    string query = @"
                SELECT 
                    ID,
                    ArtDerAusruestung,
                    Marke,
                    Model,
					Zustand,
					Beschreibung,
					Foto
					Geaendert_Von,
					Geaendert_Am
                FROM 
                    ausruestung";

                   
                    var dataTable = new DataTable();
                    using (var adapter = new MySqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }

                 
                    AusruestungDataGrid.AutoGenerateColumns = true;

                   
                    AusruestungDataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        private void AusruestungDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AusruestungDataGrid.SelectedItem != null)
            {
                A_loeschen.IsEnabled = true;
                A_Speichern.IsEnabled = true;
                A_Hinzufuegen.IsEnabled = false;

            }
            A_SafeINFelder();


        }
        private void A_SafeINFelder()
        {
          
            if (AusruestungDataGrid.SelectedItem != null)
            {
               
                if (AusruestungDataGrid.SelectedItem is DataRowView rowView)
                {
                
                    var selectedAusruestun = new Ausruestung
                    {
                        ID = rowView["ID"] != DBNull.Value ? Convert.ToInt32(rowView["ID"]) : 0,
                        ArtDerAusruestung = rowView["ArtDerAusruestung"] != DBNull.Value ? rowView["ArtDerAusruestung"].ToString() : string.Empty,
                        Marke = rowView["Marke"] != DBNull.Value ? rowView["Marke"].ToString() : string.Empty,
                        Model = rowView["Model"] != DBNull.Value ? rowView["Model"].ToString() : string.Empty,
                        Zustand = rowView["Zustand"] != DBNull.Value ? rowView["Zustand"].ToString() : string.Empty,
                        Beschreibung = rowView["Beschreibung"] != DBNull.Value ? rowView["Beschreibung"].ToString() : string.Empty,
                        Geaendert_Von = rowView["Geaendert_Von"] != DBNull.Value ? rowView["Geaendert_Von"].ToString() : string.Empty,
                        Geaendert_Am = rowView["Geaendert_Am"] != DBNull.Value ? Convert.ToDateTime(rowView["Geaendert_Am"]) : DateTime.MinValue
                    };

                  
                    selectedAusruestung.Clear(); 
                    selectedAusruestung.Add(selectedAusruestun);

                
                    txtArtDerAusreustung.Text = selectedAusruestun.ArtDerAusruestung;
                    txtMarke.Text = selectedAusruestun.Marke;
                    txtModel.Text = selectedAusruestun.Model;
                    txtZustand.Text = selectedAusruestun.Zustand;
                    txtBeschreibung.Text = selectedAusruestun.Beschreibung;
                  
                }
            }
            else
            {
               
                selectedBenutzers.Clear();

            }
        }

        private void A_loeschen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileEncription fileEncription = new FileEncription();
                string connectionString = fileEncription.ConvertToPlainString(secureString);


                int id = 0;
                if (AusruestungDataGrid.SelectedItem is DataRowView rowView)
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
                        cmd.CommandText = "DELETE FROM ausruestung WHERE ID = @id";
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                    A_loeschen.IsEnabled = false;

                    A_Speichern.IsEnabled = false;
                    LoadAusruestung();
                }
            }
            catch (Exception ex) { }
        }
        Ausruestung ausruestung = new Ausruestung();
        private void A_Hinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileEncription fileEncription = new FileEncription();
                string connectionString = fileEncription.ConvertToPlainString(secureString);



                ausruestung.ArtDerAusruestung = txtArtDerAusreustung.Text.Trim();
                ausruestung.Marke = txtMarke.Text.Trim();
                ausruestung.Model = txtModel.Text.Trim();
                ausruestung.Beschreibung = txtBeschreibung.Text.Trim();
                ausruestung.Zustand = txtZustand.Text.Trim();
                string geaendert_von = "system";
                DateTime dateTime = DateTime.Now;



                InsertAusruestungToDatabase(connectionString, ausruestung);


                MessageBox.Show("Benutzer erfolgreich hinzugefügt!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler: {ex.Message}\n{ex.StackTrace}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                Clipboard.SetText(ex.ToString());
            }

            ClearFields();

            LoadAusruestung();
        }

        private void A_Speichern_Click(object sender, RoutedEventArgs e)
        {
            SafeInDB_A(secureString, selectedAusruestung);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LoadBuchungen();
        }

        private void B_DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnBuchungAendern.IsEnabled = true;
            btnBuchungLoeschen.IsEnabled = true;

            if (B_DataGrid.SelectedItem != null)
            {
                
                if (B_DataGrid.SelectedItem is DataRowView rowView)
                {
                    
                    var selectedBuchung1 = new Buchung
                    {
                        ID = rowView["BuchungID"] != DBNull.Value ? Convert.ToInt32(rowView["BuchungID"]) : 0,
                        Buchungsbeginn = rowView["Buchungsbeginn"] != DBNull.Value
                            ? Convert.ToDateTime(rowView["Buchungsbeginn"]) : DateTime.MinValue,
                        Buchungsende = rowView["Buchungsende"] != DBNull.Value
                            ? Convert.ToDateTime(rowView["Buchungsende"]) : DateTime.MinValue
                    };

                  
                    selectedBuchung.Clear();
                    selectedBuchung.Add(selectedBuchung1);

                    
                    DatePickerBuchungsBegin.SelectedDate = selectedBuchung1.Buchungsbeginn;
                    DatePickerBuchungsEnde.SelectedDate = selectedBuchung1.Buchungsende;
                }
            }
            else
            {
         
                selectedBuchung.Clear();
                btnBuchungAendern.IsEnabled = false;
                btnBuchungLoeschen.IsEnabled = false;
            }
        }

        private void btnBuchungAendern_Click(object sender, RoutedEventArgs e)
        {
            try
            {
            
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

              
                if (selectedBuchung == null || !selectedBuchung.Any())
                {
                    MessageBox.Show("Lista Buchung jest pusta!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("OK.You are conneceted");

                    foreach (var buchung in selectedBuchung)
                    {
                     
                        if (buchung.ID == 0)
                        {
                            Console.WriteLine($"Buchung o ID 0 został pominięty (nie można zaktualizować rekordu bez ID).");
                            continue;
                        }

                     
                        buchung.Buchungsbeginn = DatePickerBuchungsBegin.DisplayDate;
                        buchung.Buchungsende = DatePickerBuchungsEnde.DisplayDate;

                        buchung.Geaendert_am = DateTime.Now;

                  
                        string query = @"
                UPDATE buchung
                SET 
                    Buchungsbeginn = @buchungsbeginn,
                    Buchungsende = @buchungsende,
                    Geaendert_Von = @Geaendert_Von,
                    Geaendert_Am = @Geaendert_Am
                WHERE 
                    ID = @Id;";

                        try
                        {
                            using (var cmd = new MySqlCommand(query, connection))
                            {
                                cmd.Parameters.AddWithValue("@Id", buchung.ID);
                                cmd.Parameters.AddWithValue("@buchungsbeginn", buchung.Buchungsbeginn);
                                cmd.Parameters.AddWithValue("@buchungsende", buchung.Buchungsende);
                                cmd.Parameters.AddWithValue("@Geaendert_Von", buchung.Geaendert_von);
                                cmd.Parameters.AddWithValue("@Geaendert_Am", buchung.Geaendert_am);

                       
                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    Console.WriteLine($"Buchung o ID {buchung.ID} został pomyślnie zapisany.");
                                }
                                else
                                {
                                    Console.WriteLine($"Nie znaleziono rekordu o ID {ausruestung.ID}.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Błąd podczas zapisywania Ausruestung o ID {ausruestung.ID}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBuchungLoeschen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileEncription fileEncription = new FileEncription();
                string connectionString = fileEncription.ConvertToPlainString(secureString);

                int id = 0;

          
                if (B_DataGrid.SelectedItem is DataRowView rowView)
                {
                    if (int.TryParse(rowView["BuchungID"].ToString(), out id) && id > 0)
                    {
             
                    }
                    else
                    {
                        MessageBox.Show("Nieprawidłowy identyfikator rekordu!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
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
                        
                        cmd.CommandText = "DELETE FROM buchung WHERE ID = @id";
                        cmd.Parameters.AddWithValue("@id", id);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Gut gemacht.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show($"UPS.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    
                  
                    A_loeschen.IsEnabled = false;
                    A_Speichern.IsEnabled = false;
                    LoadBuchungen();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}

	
	

