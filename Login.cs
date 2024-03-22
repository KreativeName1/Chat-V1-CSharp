using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WebSocket.Helper;
using BCrypt.Net;

namespace WebSocket
{
	internal class Login
	{
		private Verbindung verbindung = new Verbindung("localhost", "root", "", "chat");

		public void Einloggen()
		{
			Console.Clear();
			// Verbindung zur Datenbank herstellen
			try { verbindung.Verbinden(); }
			catch (Exception ex) { Print(ex.Message, ConsoleColor.Red); }

			Print("Login", ConsoleColor.Blue);
			Print("-----", ConsoleColor.Blue);

			// Email und Passwort abfragen
			string email = Read<string>("Email: ");
			string passwort = null;
			Print("Passwort: ", ConsoleColor.Green, false);
			while (true)
			{
				var key = Console.ReadKey(true);
				if (key.Key == ConsoleKey.Enter)
					break;
				passwort += key.KeyChar;
				Console.Write("*");
			}

			// Überprüfen ob Email existiert
			if (verbindung.check("users", "email", email))
			{
				// Benutzerdaten abfragen
				var user = verbindung.getUserMitEmailOderId(email);
				verbindung.Schliessen();

				// Passwort überprüfen
				if (BCrypt.Net.BCrypt.Verify(passwort, user.Passwort))
				{
					// Benutzerdaten speichern und weiterleiten zu Kontakte
					Program.Benutzer = user;
					new Kontakte().Menu();
				}
				// Passwort falsch
				else
				{
					Print("Falsches Passwort!", ConsoleColor.Red);
					Console.ReadKey();
					Einloggen();
				}
			}
			// Email existiert nicht
			else
			{
				Print("Diese Email ist nicht registriert!", ConsoleColor.Red);
				Console.ReadKey();
				Einloggen();
			}
		}
	}
}