using static WebSocket.Helper;
using static WebSocket.Program;

namespace WebSocket
{
	internal class Kontakte
	{
		private Verbindung verbindung = new Verbindung("localhost", "root", "", "chat");
		private List<User> kontakte = new List<User>();
		private List<User> anfragen = new List<User>();

		public void Menu()
		{
			{
				Console.Clear();
				// Verbindung zur Datenbank herstellen
				try { verbindung.Verbinden(); }
				catch (Exception ex) { Print(ex.Message, ConsoleColor.Red); }

				// Menü anzeigen
				Print("Kontakte", ConsoleColor.Blue);
				Print("--------", ConsoleColor.Blue);
				Print("1. Anfrage schicken", ConsoleColor.Green);
				Print("2. Anfragenliste anzeigen", ConsoleColor.Green);
				Print("3. Kontaktliste anzeigen", ConsoleColor.Green);
				Print("4. Kontakt löschen", ConsoleColor.Green);
				Print("5. Beenden", ConsoleColor.Green);

				// Auswahl abfragen
				ConsoleKeyInfo auswahl = Console.ReadKey();
				switch (auswahl.Key)
				{
					case ConsoleKey.D1:
						AnfrageSchicken();
						break;

					case ConsoleKey.D2:
						AnfragenAnzeigen();
						break;

					case ConsoleKey.D3:
						KontakteAnzeigen();
						break;

					case ConsoleKey.D4:
						KontaktLöschen();
						break;

					case ConsoleKey.D5:
						Environment.Exit(0);
						break;

					default:
						Menu();
						break;
				}
			}
		}

		private void KontakteLaden()
		{
			kontakte = verbindung.getContacts(Program.Benutzer.Id);
		}

		private void AnfragenLaden()
		{
			anfragen = verbindung.getRequests(Program.Benutzer.Id);
		}

		private void KontaktLöschen()
		{
			Console.Clear();
			KontakteLaden();

			// Menü anzeigen
			Print("Kontakte", ConsoleColor.Blue);
			Print("--------", ConsoleColor.Blue);
			Print("ESC: zurück", ConsoleColor.Green);
			Console.WriteLine();

			// Kontakte anzeigen
			int zähler = 0;
			foreach (User kontakt in kontakte)
			{
				zähler++;
				Print($"{zähler}: {kontakt.Name}", ConsoleColor.Green);
			}

			// Auswahl abfragen
			ConsoleKeyInfo input = Console.ReadKey();

			// Wenn ESC gedrückt wurde, zurück zum Menü
			if (input.Key == ConsoleKey.Escape) Menu();
			else
			{
				// Umwandeln in int
				int auswahl = Convert.ToInt32(input.KeyChar.ToString());

				if (auswahl > 0 && auswahl <= zähler)
				{
					string sql = $"DELETE FROM friends WHERE friend_id = {kontakte[auswahl - 1].Id} AND user_id = {Benutzer.Id}";
					verbindung.Ausführen(sql);
					sql = $"DELETE FROM friends WHERE friend_id = {Benutzer.Id} AND user_id = {kontakte[auswahl - 1].Id}";
					verbindung.Ausführen(sql);
					Console.WriteLine();
					Print("Kontakt gelöscht!", ConsoleColor.Cyan);
				}
				else Print("Ungültige Eingabe!", ConsoleColor.Red);

				Console.WriteLine();
				Print("Drücke eine beliebige Taste um fortzufahren...", ConsoleColor.Red);
				Console.ReadKey();
				KontakteAnzeigen();
			}
		}

		private async void KontakteAnzeigen()
		{
			Console.Clear();
			KontakteLaden();

			// Menü anzeigen
			Print("Kontakte", ConsoleColor.Blue);
			Print("--------", ConsoleColor.Blue);
			Print("ESC: zurück", ConsoleColor.Green);
			Console.WriteLine();

			// Kontakte anzeigen
			int zähler = 0;
			foreach (User kontakt in kontakte)
			{
				zähler++;
				Print($"{zähler}: {kontakt.Name}", ConsoleColor.Green);
			}

			// Auswahl abfragen
			ConsoleKeyInfo input = Console.ReadKey();

			// Wenn ESC gedrückt wurde, zurück zum Menü
			if (input.Key == ConsoleKey.Escape) Menu();
			else
			{
				// Umwandeln in int
				int auswahl = Convert.ToInt32(input.KeyChar.ToString());

				if (auswahl > 0 && auswahl <= zähler)
				{
					new Chat(kontakte[auswahl - 1].Id).Chatten().Wait();
					Print("Chat beendet!", ConsoleColor.Cyan);
				}
				else Print("Ungültige Eingabe!", ConsoleColor.Red);

				Print("Drücke eine beliebige Taste um fortzufahren...", ConsoleColor.Green);
				Console.ReadKey();
				KontakteAnzeigen();
			}
		}

		private void AnfragenAnzeigen()
		{
			Console.Clear();
			AnfragenLaden();

			Print("Anfragen", ConsoleColor.Blue);
			Print("--------", ConsoleColor.Blue);
			Print("zahl+a = Annehmen; zahl+b = Ablehnen", ConsoleColor.Green);
			Console.WriteLine();
			string eingabe;
			int nummer;
			char wahl;

			int zähler = 0;
			foreach (User u in anfragen)
			{
				zähler++;
				Print($"{zähler}: {u.Name}", ConsoleColor.Green);
			}

			while (true)
			{
				do
				{
					Console.WriteLine();
					Print("Bitte wählen: ", ConsoleColor.Green, false);
					eingabe = Console.ReadLine();
					if (string.IsNullOrWhiteSpace(eingabe)) Menu();
				} while (!TryParseInput(eingabe, out nummer, out wahl));
				if (nummer > 0 && nummer <= zähler)
				{
					if (wahl == Convert.ToChar("a"))
					{
						string sql = $"UPDATE friends SET state = 'accepted' WHERE user_id = {anfragen[nummer - 1].Id} AND friend_id = {Benutzer.Id}";
						verbindung.Ausführen(sql);
						sql = $"INSERT INTO friends (friend_id, user_id, state) VALUES ({anfragen[nummer - 1].Id}, {Benutzer.Id}, 'accepted')";
						verbindung.Ausführen(sql);
						Console.WriteLine();
						Print("Anfrage angenommen!", ConsoleColor.Cyan);
						break;
					}
					else if (wahl == Convert.ToChar("b"))
					{
						string sql = $"UPDATE friends SET state = 'rejected' WHERE friend_id = {Benutzer.Id} AND user_id = {anfragen[nummer - 1].Id}";
						verbindung.Ausführen(sql);
						Console.WriteLine();
						Print("Anfrage abgelehnt!", ConsoleColor.Cyan);
						break;
					}
					else Print("Falsche Auswahl!", ConsoleColor.Red);
				}
				else Print("Falsche Auswahl", ConsoleColor.Red);
			}

			Console.WriteLine();
			Print("Drücke eine beliebige Taste um fortzufahren...", ConsoleColor.Red);
			Console.ReadKey();
			Menu();
		}

		private void AnfrageSchicken()
		{
			Console.Clear();
			Print("Anfragen", ConsoleColor.Blue);
			Print("--------", ConsoleColor.Blue);
			Console.WriteLine();
			while (true)
			{
				try
				{
					string name = Read<string>("Benutzername eingeben: ");
					if (Benutzer.Name == name) throw new Exception("Du kannst dich nicht selbst hinzufügen!");

					string sql = $"SELECT id FROM users WHERE name = '{name}'";
					int friend_id = verbindung.getId(sql);
					if (friend_id == 0) throw new Exception("Der Benutzer wurde nicht gefunden!");
					sql = $"INSERT INTO friends (user_id, friend_id) VALUES ({Benutzer.Id},{friend_id})";
					verbindung.Ausführen(sql);
					Console.WriteLine();
					Print("Anfrage geschickt!", ConsoleColor.Cyan);
					break;
				}
				catch (Exception ex)
				{
					Print(ex.Message, ConsoleColor.Red);
				}
			}
			Console.WriteLine();
			Print("Drücke eine beliebige Taste um fortzufahren...", ConsoleColor.Red);
			Console.ReadKey();
			Menu();
		}
	}
}