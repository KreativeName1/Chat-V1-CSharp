using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static WebSocket.Helper;

namespace WebSocket
{
	internal class Program
	{
		public static User Benutzer = new User();

		private static void Main(string[] args)
		{
			while (true)
			{
				Console.Clear();
				Print("Chat", ConsoleColor.Blue);
				Print("----", ConsoleColor.Blue);
				Print("1. Registrieren", ConsoleColor.Green);
				Print("2. Login", ConsoleColor.Green);
				ConsoleKeyInfo auswahl = Console.ReadKey();
				if (auswahl.Key == ConsoleKey.D1 || auswahl.Key == ConsoleKey.NumPad1)
				{
					Registrierung registrierung = new Registrierung();
					registrierung.Registrieren();
					break;
				}
				else if (auswahl.Key == ConsoleKey.D2 || auswahl.Key == ConsoleKey.NumPad2)
				{
					Login login = new Login();
					login.Einloggen();
					break;
				}
				else
				{
					Print("Ungültige Auswahl!", ConsoleColor.Red);
				}
			}
		}
	}
}