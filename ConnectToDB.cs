using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VWA
{
	internal class ConnectToDB
	{
		public string ShowRole(string connectionString)
		{
			string role = null;
			try
			{
				using (var connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Welche Role hat Benutzer
					string query = @"
                SELECT ROLE_NAME, IS_GRANTABLE
				FROM information_schema.APPLICABLE_ROLES
				WHERE GRANTEE = CURRENT_USER();";

					using (var command = new MySqlCommand(query, connection))
					{
						using (var reader = command.ExecuteReader())
						{
							string roles = "Rolle des Benutzers:\n";
							while (reader.Read())
							{
								role=$"{reader["role_name"]}\n";
								roles += $"{reader["role_name"]}\n";
								
							}

							if (roles == "Rolle des Benutzers:\n")
							{
								roles = "Benutzer hat keine Rolle";
							}


						}
					}

					connection.Close();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Fehler während Verbindung mit DB. {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			return role;
		}

		public void ConnectToDB1(string connectionString)
		{
			try
			{
				// Erstellung die Verbindung mit DB
				using (var connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					MessageBox.Show("Sie sind in System!", "Gut gemacht!", MessageBoxButton.OK, MessageBoxImage.Information);
					MainWindow mainWindow = new MainWindow();
					
					connection.Close();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Fehler während Verbindung mit DB. {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
	}

