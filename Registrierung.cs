using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WebSocket.Helper;
using BCrypt.Net;

namespace WebSocket
{
	internal class Registrierung
	{
		// connection to database
		private Verbindung verbindung = new Verbindung("localhost", "root", "", "chat");

		public void Registrieren()
		{
			Console.Clear();
			try { verbindung.Verbinden(); }
			catch (Exception ex) { Print(ex.Message, ConsoleColor.Red); }
			string email, name = default;
			string passwort = null;
			string passwort2 = null;
			Print("Registrierung", ConsoleColor.Red);
			Print("-------------", ConsoleColor.Red);
			while (true)
			{
				try
				{
					email = Read<string>("Email: ");
					if (verbindung.check("users", "email", email)) throw new Exception("Diese Email ist bereits vergeben!");
					break;
				}
				catch (Exception ex) { Print(ex.Message, ConsoleColor.Red); }
			}
			while (true)
			{
				try
				{
					Print("Passwort: ", ConsoleColor.Green);
					while (true)
					{
						var key = Console.ReadKey(true);
						if (key.Key == ConsoleKey.Enter)
							break;
						passwort += key.KeyChar;
					}
					Print("Passwort wiederholen: ", ConsoleColor.Green);
					while (true)
					{
						var key = Console.ReadKey(true);
						if (key.Key == ConsoleKey.Enter)
							break;
						passwort2 += key.KeyChar;
					}
					if (passwort != passwort2)
					{
						passwort = null;
						passwort2 = null;
						throw new Exception("Passwörter stimmen nicht überein!");
					}
					break;
				}
				catch (Exception ex) { Print(ex.Message, ConsoleColor.Red); }
			}
			while (true)
			{
				try
				{
					name = Read<string>("Name: ");
					if (name.Trim() == "") throw new Exception("Name darf nicht leer sein!");
					break;
				}
				catch (Exception ex) { Print(ex.Message, ConsoleColor.Red); }
			}
			passwort = BCrypt.Net.BCrypt.HashPassword(passwort);
			try
			{
				int ergebnis = verbindung.Ausführen($"INSERT INTO users (email, password, name) VALUES ('{email}', '{passwort}', '{name}');");
				if (ergebnis == 1)
				{
					Console.WriteLine();
					Print("Registrierung erfolgreich!", ConsoleColor.Green);
					Console.ReadKey();
					new Login().Einloggen();
				}
				else
				{
					Console.WriteLine();
					Print("Etwas ist schiefgelaufen!", ConsoleColor.Red);
					Print("Drücke eine beliebige Taste um es erneut zu versuchen...", ConsoleColor.Red);
					Console.ReadKey();
					Registrieren();
				}
			}
			catch (Exception ex) { Print(ex.Message, ConsoleColor.Red); }
		}
	}
}