using MySql.Data.MySqlClient;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Avalonia_Test_1.Views;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Avalonia_Test_1.Models
{
    public class UserAuthentication
    {
		string server;
		string port;
		string database;
		string serverUsername;
		string serverPassword;
		string connectionString;

		public UserAuthentication()
		{
			//Connection string is separated into parts for easier modification if needed
			server = "ls-6618b77859d7dec5c46e49dc9053f758c6f22ca8.cz4uw4k284i0.us-east-1.rds.amazonaws.com";
			port = "3306";
			database = "slotmachine";
			serverUsername = "slotserveruser";
			serverPassword = "r@nD0m1ze";
			connectionString = $"Server={server};Port={port};Database={database};Uid={serverUsername};Pwd={serverPassword}";
		}

		public bool AddUser(string username, string password, int points)
		{
			//Adds a user to the list
			try
			{
				byte[] salt = GenerateSalt();

				string hashedPassword = HashPassword(password, salt);

				//Connects to the database
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					//Generates a query and runs it against the database
					string query = "INSERT INTO users (username, password, salt, points)" +
						"VALUES (@Username, @Password, @Salt, @Points)";
					using (MySqlCommand command = new MySqlCommand(query, connection))
					{
						command.Parameters.AddWithValue("@Username", username);
						command.Parameters.AddWithValue("@Password", hashedPassword);
						command.Parameters.AddWithValue("@Salt", salt);
						command.Parameters.AddWithValue("@Points", points);
						command.ExecuteNonQuery();
					}
					connection.Close();
				}
				return true;
			}
			catch (MySqlException ex)
			{
				ErrorWindow errorWindow = new ErrorWindow($"Error adding user: {ex.Message}");
				errorWindow.Show();
				return false;
			}
		}

		public int AuthenticateUser(string username, string password)
		{
			//This function returns an int instead of a bool because that's the easiest way to get the points back
			try
			{
				string? storedPasswordHash = null;
				byte[]? storedSalt = null;
				int points = -1;
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					string query = "SELECT password, salt, points FROM users WHERE username = @Username";
					using (MySqlCommand command = new MySqlCommand(query, connection))
					{
						command.Parameters.AddWithValue("@Username", username);
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								storedPasswordHash = reader.GetString("password");
								storedSalt = (byte[])reader["salt"];
								points = reader.GetInt32("points");
							}
						}
					}
					connection.Close();
				}

				if (storedPasswordHash == null)
				{
					ErrorWindow errorWindow = new ErrorWindow($"Error: Username Not Found");
					errorWindow.Show();
					return -1;
				}

				//Hash the given password and check if it matches the stored hashed password
				string hashedPassword = HashPassword(password, storedSalt);
				if (hashedPassword != storedPasswordHash)
				{
					ErrorWindow errorWindow = new ErrorWindow("Error: Incorrect Password");
					errorWindow.Show();
					return -1;
				}
				return points;
			}
			catch (MySqlException ex)
			{
				ErrorWindow errorWindow = new ErrorWindow($"Error authenticating user: {ex.Message}");
				errorWindow.Show();
				return -1;
			}
		}

		public bool UpdateUserPoints(string username, int points)
		{
			try
			{
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					string query = "UPDATE users SET points = @Points WHERE username = @Username";
					using (MySqlCommand command = new MySqlCommand(query, connection))
					{
						command.Parameters.AddWithValue("@Username", username);
						command.Parameters.AddWithValue("@Points", points);
						int rowsAffected = command.ExecuteNonQuery();
						if (rowsAffected > 0)
						{
							connection.Close();
							return true;
						}
						else
						{
							ErrorWindow errorWindow = new ErrorWindow("Username not found");
							errorWindow.Show();
							connection.Close();
							return false;
						}
					}
				}
			}
			catch (MySqlException ex)
			{
				ErrorWindow errorWindow = new ErrorWindow($"Error updating user points: {ex.Message}");
				errorWindow.Show();
				return false;
			}
		}

		private byte[] GenerateSalt()
		{
			//Generates a random salt
			byte[] salt = new byte[16];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(salt);
			}
			return salt;
		}

		private string HashPassword(string password, byte[] salt)
		{
			//Hashes the given password and salt combo
			using (var sha256 = SHA256.Create())
			{
				byte[] saltedPassword = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
				byte[] hashedBytes = sha256.ComputeHash(saltedPassword);
				return Convert.ToBase64String(hashedBytes);
			}
		}
	}
}
