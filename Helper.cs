using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace WebSocket
{
	internal class Helper
	{
		[DebuggerStepThrough]
		public static void Print(string text, ConsoleColor color, bool newLine = true)
		{
			Console.ForegroundColor = color;
			if (newLine) Console.WriteLine(text);
			else Console.Write(text);
			Console.ResetColor();
		}

		[DebuggerStepThrough]
		public static T Read<T>(string prompt, ConsoleColor color = ConsoleColor.Green)
		{
			Print(prompt, color, false);
			string input = Console.ReadLine();
			return (T)Convert.ChangeType(input, typeof(T));
		}

		public static bool TryParseInput(string input, out int number, out char choice)
		{
			// split input into number and choice with regex

			string pattern = @"(\d+)([a-z])";
			Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
			Match match = regex.Match(input);
			if (match.Success)
			{
				number = int.Parse(match.Groups[1].Value);
				choice = match.Groups[2].Value[0];
				return true;
			}
			else
			{
				Print("Ungültige Auswahl!", ConsoleColor.Red);

				number = 0;
				choice = Convert.ToChar("z");
				return false;
			}
		}
	}
}