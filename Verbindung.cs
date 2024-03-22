using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace WebSocket
{
	internal class Verbindung
	{
		public MySqlConnectionStringBuilder builder { get; set; } = default;
		public MySqlConnection conn { get; set; } = default;

		[DebuggerStepThrough]
		public Verbindung(string server, string user, string password, string database)
		{
			builder = new MySqlConnectionStringBuilder();
			builder.Server = server;
			builder.UserID = user;
			builder.Password = password;
			builder.Database = database;
		}

		// Verbindung zur Datenbank herstellen
		[DebuggerStepThrough]
		public void Verbinden()
		{
			conn = new MySqlConnection(builder.ConnectionString);
			conn.Open();
		}

		// Verbindung schließen
		[DebuggerStepThrough]
		public void Schliessen()
		{
			conn.Close();
		}

		// SQl-Befehl ausführen
		[DebuggerStepThrough]
		public int Ausführen(string sql)
		{
			int result = default;
			using (MySqlCommand command = new MySqlCommand(sql, conn))
			{
				result = command.ExecuteNonQuery();
			}
			return result;
		}

		// Führt ein SQL-Befehl aus und gibt die Zeilen zurück
		[DebuggerStepThrough]
		public List<object[]> Zurückgeben(string sql)
		{
			// Befehl erstellen
			using (MySqlCommand command = new MySqlCommand(sql, conn))
			{
				List<object[]> results = new List<object[]>();

				// Führt den Befehl aus und speichert das Ergebnis
				MySqlDataReader reader = command.ExecuteReader();

				while (reader.Read())
				{
					object[] row = new object[reader.FieldCount];

					for (int i = 0; i < reader.FieldCount; i++)
					{
						row[i] = reader[i];
					}

					results.Add(row);
				}

				reader.Close();
				return results;
			}
		}

		// Testet ob ein Wert existiert
		[DebuggerStepThrough]
		public bool check(string table, string column, string value)
		{
			string sql = $"select * from {table} where {column} = '{value}'";
			List<object[]> result = Zurückgeben(sql);

			if (result.Count > 0) return true;
			else return false;
		}

		// get user with email
		[DebuggerStepThrough]
		public User getUserMitEmailOderId(string email = null, int id = -1)
		{
			string sql = default;
			if (email == null && id == -1) return null;
			if (!string.IsNullOrEmpty(email)) sql = $"select * from users where email = '{email}'";
			else sql = $"select * from users where id = '{id}'";
			List<object[]> result = Zurückgeben(sql);
			if (result.Count > 0)
			{
				object[] row = result[0];
				User user = new User(Convert.ToInt32(row[0]), Convert.ToString(row[3]), Convert.ToString(row[2]), Convert.ToString(row[1]));
				return user;
			}
			else return null;
		}

		[DebuggerStepThrough]
		public int getId(string sql)
		{
			List<object[]> result = Zurückgeben(sql);
			if (result.Count > 0)
			{
				object[] row = result[0];
				return Convert.ToInt32(row[0]);
			}
			else return 0;
		}

		// get contacts from user
		[DebuggerStepThrough]
		public List<User> getContacts(int id)
		{
			string sql = $"select * from friends where user_id = {id} and state = 'accepted'";

			List<object[]> result = Zurückgeben(sql);

			List<User> contacts = new List<User>();

			foreach (object[] row in result)
			{
				User user = getUserMitEmailOderId(null, Convert.ToInt32(row[2]));
				contacts.Add(user);
			}

			return contacts;
		}

		public List<User> getRequests(int id)
		{
			string sql = $"select u.* from friends f INNER JOIN users u ON f.user_id = u.id WHERE f.friend_id = {id} AND f.state = 'sent'";

			List<object[]> result = Zurückgeben(sql);

			List<User> contacts = new List<User>();

			foreach (object[] row in result)
			{
				User user = getUserMitEmailOderId(null, Convert.ToInt32(row[0]));
				contacts.Add(user);
			}

			return contacts;
		}
	}
}