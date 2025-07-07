using Microsoft.Extensions.Configuration;
using Bogus;
using static Bogus.DataSets.Name;
using System.Diagnostics;
using Homework.Models;
using Npgsql;
using System.Security.Cryptography;
using System.Numerics;

namespace Homework;

public class DatabaseService
{
    private readonly Npgsql.NpgsqlConnection _conn;

    public DatabaseService()
    {
        var conf = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();

        string connStr = conf["ConnectionStrings:DefaultConnection"] ?? "";

        _conn = new Npgsql.NpgsqlConnection(connStr);
        _conn.Open();
        LoadAllTables();
    }

    public void LoadAllTables()
    {
        string dir = Path.Combine(Directory.GetCurrentDirectory(), "SqlScripts", "tables");
        if (!Directory.Exists(dir))
        {
            Console.WriteLine($"Директорії {dir} не існує.");
            return;
        }

        var files = Directory.GetFiles(dir, "*.sql");
        foreach (var file in files)
        {
            string sql = File.ReadAllText(file);

            using (var cmd = new Npgsql.NpgsqlCommand(sql, _conn))
            {
                var result = cmd.ExecuteNonQuery();
            }
        }
    }

    public void InsertRandomUsers(int count)
    {
        string sql = @"
            INSERT INTO users 
            (first_name, last_name, phone, email, password, created_at, updated_at) 
            VALUES(@first_name, @last_name, @phone, @email, @password, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
        ";

        var myUserFaker = new Faker<User>("uk")
            .CustomInstantiator(f => new User())
            .RuleFor(u => u.Gender, f => f.PickRandom<Gender>())
            .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName(u.Gender))
            .RuleFor(u => u.LastName, (f, u) => f.Name.LastName(u.Gender))
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
            .RuleFor(u => u.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(u => u.Password, f => f.Internet.Password());

        var users = myUserFaker.Generate(count);
     
        using var cmd = new NpgsqlCommand(sql, _conn);
        foreach (var user in users)
        {
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("first_name", user.FirstName);
            cmd.Parameters.AddWithValue("last_name", user.LastName);
            cmd.Parameters.AddWithValue("phone", user.Phone);
            cmd.Parameters.AddWithValue("email", user.Email);
            cmd.Parameters.AddWithValue("password", user.Password);
            var result = cmd.ExecuteNonQuery();
        }
    }

    public List<User> GetAllUsers()
    {
        var users = new List<User>();

        string sql = @"SELECT id, first_name, last_name, phone, email FROM users";

        using var cmd = new NpgsqlCommand(sql, _conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            users.Add(new User
            {
                Id = Convert.ToInt32(reader["id"]),
                FirstName = Convert.ToString(reader["first_name"]) ?? "",
                LastName = Convert.ToString(reader["last_name"]) ?? "",
                Phone = Convert.ToString(reader["phone"]) ?? "",
                Email = Convert.ToString(reader["email"]) ?? ""
            });
        }

        return users;
    }
    public List<User> SearchUsers(string searchInfo)
    {
        var users = new List<User>();
        string sql;
        var isNumeric = int.TryParse(searchInfo, out int id);

        if (isNumeric)
        {
            sql = @"SELECT id, first_name, last_name, phone, email FROM users WHERE id = @searchInfo";
        }
        else
        {
            sql = @" SELECT id, first_name, last_name, phone, email FROM users 
            WHERE first_name = @searchInfo OR last_name = @searchInfo OR phone = @searchInfo OR email = @searchInfo";
        }

        using var cmd = new NpgsqlCommand(sql, _conn);
        cmd.Parameters.AddWithValue("searchInfo", isNumeric ? id : searchInfo);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new User
            {
                Id = Convert.ToInt32(reader["id"]),
                FirstName = Convert.ToString(reader["first_name"]) ?? "",
                LastName = Convert.ToString(reader["last_name"]) ?? "",
                Phone = Convert.ToString(reader["phone"]) ?? "",
                Email = Convert.ToString(reader["email"]) ?? ""
            });
        }

        return users;
    }

    public void UpdateUser(int id, User user)
    {
        string sql = "UPDATE users SET " +
            (!string.IsNullOrEmpty(user.FirstName) ? "first_name = @new_first_name, " : "") +
            (!string.IsNullOrEmpty(user.LastName) ? "last_name = @new_last_name, " : "") +
            (!string.IsNullOrEmpty(user.Email) ? "email = @new_email, " : "") +
            (!string.IsNullOrEmpty(user.Phone) ? "phone = @new_phone, " : "") +
            (!string.IsNullOrEmpty(user.Password) ? "password = @new_password, " : "");

        if (sql.EndsWith(", ")) sql = sql.Substring(0, sql.Length - 2);
        sql += " WHERE id = @id";

        using var cmd = new NpgsqlCommand(sql, _conn);

        cmd.Parameters.AddWithValue("id", id);
        if (!string.IsNullOrEmpty(user.FirstName)) cmd.Parameters.AddWithValue("new_first_name", user.FirstName);
        if (!string.IsNullOrEmpty(user.LastName)) cmd.Parameters.AddWithValue("new_last_name", user.LastName);
        if (!string.IsNullOrEmpty(user.Email)) cmd.Parameters.AddWithValue("new_email", user.Email);
        if (!string.IsNullOrEmpty(user.Phone)) cmd.Parameters.AddWithValue("new_phone", user.Phone);
        if (!string.IsNullOrEmpty(user.Password)) cmd.Parameters.AddWithValue("new_password", user.Password);

        cmd.ExecuteNonQuery();
    }
    public void DeleteUser(int id)
    {
        string sql = @"DELETE FROM users WHERE id = @id";

        using var cmd = new NpgsqlCommand(sql, _conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.ExecuteNonQuery();
    }

    public void Dispose()
    {
        _conn.Dispose();
    }
}
