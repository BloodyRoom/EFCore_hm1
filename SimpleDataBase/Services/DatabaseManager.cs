
using Microsoft.Extensions.Configuration;
using Npgsql;
using SimpleDataBase.Models;

namespace SimpleDataBase.Services;

public class DatabaseManager : IDisposable
{
    private readonly NpgsqlConnection _conn;

    public DatabaseManager()
    {
        var conf = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string connStr = conf["ConnectionStrings:DefaultConnection"];
        _conn = new NpgsqlConnection(connStr);
        _conn.Open();
    }

    public void ExecuteCreateTables()
    {
        string dir = Path.Combine(Directory.GetCurrentDirectory(), "SqlScripts", "tables");
        if (!Directory.Exists(dir))
        {
            Console.WriteLine($"Директорія {dir} не існує.");
            return;
        }

        // Зчитування SQL скриптів з файлів у директорії
        var files = Directory.GetFiles(dir, "*.sql");
        foreach (var file in files)
        {
            string sql = File.ReadAllText(file);

            using (var cmd = new Npgsql.NpgsqlCommand(sql, _conn))
            {
                // Отримати результат першого рядку таблиці, яку прочитали
                var result = cmd.ExecuteNonQuery();
                Console.WriteLine($"{Path.GetFileName(file)} -- {result}");
            }
        }
    }

    public void DropTables()
    {
        string file = Path.Combine(Directory.GetCurrentDirectory(), "SqlScripts", "dropTables.sql");
        if (!File.Exists(file))
        {
            Console.WriteLine($"Файл {file} не існує.");
            return;
        }
        // Зчитування SQL скриптів з файлів у директорії
        var lines = File.ReadAllLines(file);
        foreach (var query in lines)
        {
            using (var cmd = new NpgsqlCommand(query, _conn))
            {
                // Отримати результат з першого рядку таблиці, яку прочитали
                var result = cmd.ExecuteNonQuery();
                Console.WriteLine($"--- {query} ---");
            }
        }
    }

    public void PrintTableList()
    {
        string sql = @"
            SELECT table_name as name
            FROM information_schema.tables
            WHERE table_schema = 'public'
                AND table_type = 'BASE TABLE';
        ";
        using var cmd = new NpgsqlCommand(sql, _conn);
        using var reader = cmd.ExecuteReader();
        Console.WriteLine("Tables in the database:");
        while (reader.Read())
        {
            Console.WriteLine(reader["name"]);
        }
    }

    public void InsertUsers(List<User> users)
    {
        string sql = @"
            INSERT INTO users 
            (first_name, last_name, phone, email, password, created_at, updated_at) 
            VALUES(@first_name, @last_name, @phone, @email, @password, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
        ";
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
            Console.WriteLine($"Inserted {user.LastName} {user.FirstName} -- {result}");
        }
    }

    public User? GetUserByEmail(string email)
    {
        string sql = @"
            SELECT * FROM users 
            WHERE email = @Email;
        ";
        using var cmd = new NpgsqlCommand(sql, _conn);
        cmd.Parameters.AddWithValue("Email", email);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                LastName = reader.GetString(reader.GetOrdinal("last_name")),
                Phone = reader.GetString(reader.GetOrdinal("phone")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                Password = reader.GetString(reader.GetOrdinal("password")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"))
            };
        }
        return null;
    }

    public void Dispose()
    {
        _conn.Dispose();
    }
}
