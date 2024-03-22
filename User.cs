using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocket
{
	internal class User
	{
		public int Id { get; set; }
		public string Email { get; set; }
		public string Passwort { get; set; }
		public string Name { get; set; }

		public User(int id, string email, string passwort, string name)
		{
			this.Id = id;
			this.Email = email;
			this.Passwort = passwort;
			this.Name = name;
		}

		public User()
		{ }

		public override string ToString()
		{
			return $"ID: {Id}\nEmail: {Email}\nPasswort: {Passwort}\nName: {Name}";
		}
	}
}