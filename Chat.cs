using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;
using static WebSocket.Helper;
using static WebSocket.Program;

namespace WebSocket
{
	internal class Chat
	{
		private ClientWebSocket client = new ClientWebSocket();
		private Verbindung verbindung = new Verbindung("localhost", "root", "", "chat");
		public int Id { get; set; }
		public User Freund { get; set; } // Chatpartner
		public int Raum { get; set; } = 0;

		public Chat(int freund_id)
		{
			Console.Clear();
			try
			{
				verbindung.Verbinden();
				Freund = verbindung.getUserMitEmailOderId(null, freund_id);
				Id = verbindung.getId($"SELECT id FROM chats WHERE user_1 = {Benutzer.Id} AND user_2 = {Freund.Id} OR user_1 = {Freund.Id} AND user_2 = {Benutzer.Id}");
				if (Id == 0)
				{
					verbindung.Ausführen($"INSERT INTO chats (user_1, user_2) VALUES ({Benutzer.Id}, {Freund.Id})");
					Id = verbindung.getId($"SELECT id FROM chats WHERE user_1 = {Benutzer.Id} AND user_2 = {Freund.Id} OR user_1 = {Freund.Id} AND user_2 = {Benutzer.Id}");
				}
				verbindung.Schliessen();
			}
			catch (Exception ex) { Print(ex.Message, ConsoleColor.Red); Environment.Exit(5); }
		}

		public async Task Chatten()
		{
			Print("Chat", ConsoleColor.Blue);
			Print("----", ConsoleColor.Blue);
			Uri serverUri = new Uri("ws://localhost:8080");
			try
			{
				// Mit WebSocket verbinden
				Print("Verbinden...", ConsoleColor.DarkGreen);
				await client.ConnectAsync(serverUri, CancellationToken.None);
				Print("Verbunden!", ConsoleColor.DarkGreen);

				Task msgErhalten = NachrichtenErhalten();

				// Beitrittsnachricht senden
				BeitrittNachricht beitritt = new BeitrittNachricht()
				{
					room = Id,
					user_id = Benutzer.Id,
					user_name = Benutzer.Name,
					type = "join",
				};
				string json = JsonConvert.SerializeObject(beitritt);
				await SendWebSocketMessage(json);

				// Nachrichten senden
				while (client.State == WebSocketState.Open)
				{
					string input = Console.ReadLine();

					if (input == "exit")
					{
						await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Benutzer hat verlassen", CancellationToken.None);
						break;
					}
					if (string.IsNullOrWhiteSpace(input)) continue;

					Nachricht nachricht = new Nachricht()
					{
						message = input ?? "",
						name = Benutzer.Name,
						date = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"),
						user_id = Benutzer.Id,
						room = Raum,
						type = "message",
					};
					json = JsonConvert.SerializeObject(nachricht);
					await SendWebSocketMessage(json);

					// Nachricht in der Konsole anzeigen
					Print($"{nachricht.name} ({nachricht.date}): {nachricht.message}", ConsoleColor.Magenta);
				}

				// Auf Nachrichten warten
				await msgErhalten;
			}
			catch (Exception ex) { Console.WriteLine($"Fehler: {ex.Message}"); }
			finally { client.Dispose(); }
		}

		// Methode zum Senden einer Nachricht
		private async Task SendWebSocketMessage(string message)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(message);
			await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
		}

		// Methode zum Empfangen der Nachrichten
		private async Task NachrichtenErhalten()
		{
			byte[] buffer = new byte[1024];
			try
			{
				while (client.State == WebSocketState.Open)
				{
					WebSocketReceiveResult ergebnis = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
					if (ergebnis.MessageType == WebSocketMessageType.Text)
					{
						BeitrittAntwort antwort = JsonConvert.DeserializeObject<BeitrittAntwort>(Encoding.UTF8.GetString(buffer, 0, ergebnis.Count));
						if (antwort.type == "room") Raum = antwort.room;
						else if (antwort.type == "db_message")
						{
							Nachricht nachricht = JsonConvert.DeserializeObject<Nachricht>(Encoding.UTF8.GetString(buffer, 0, ergebnis.Count));
							if (nachricht.user_id == Benutzer.Id) Print($"{nachricht.name} ({nachricht.date}): {nachricht.message}", ConsoleColor.Magenta);
							else Print($"{nachricht.name} ({nachricht.date}): {nachricht.message}", ConsoleColor.Green);
						}
						else if (antwort.type == "message")
						{
							Nachricht nachricht = JsonConvert.DeserializeObject<Nachricht>(Encoding.UTF8.GetString(buffer, 0, ergebnis.Count));
							Print($"{nachricht.name} ({nachricht.date}): {nachricht.message}", ConsoleColor.Green);
						}
					}
				}
			}
			catch (Exception ex) { Console.WriteLine($"Fehler: {ex.Message}"); }
		}
	}

	public class BeitrittNachricht
	{
		public string type;
		public int room;
		public int user_id;
		public string user_name;
	}

	public class BeitrittAntwort
	{
		public string type;
		public int room;
	}

	public class Nachricht
	{
		public string type;
		public string message;
		public string name;
		public string date;
		public int user_id;
		public int? room;
	}
}