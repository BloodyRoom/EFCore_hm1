using Microsoft.Extensions.Configuration;
using SimpleDataBase.Models;
using SimpleDataBase.Services;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

DatabaseManager databaseManager = new DatabaseManager();

int action = 0;
do
{
    Console.WriteLine("Виберіть дію:");
    Console.WriteLine("0. Вийти");
    Console.WriteLine("1. Створити таблиці");
    Console.WriteLine("2. Видалити таблиці");
    Console.WriteLine("3. Показати таблиці");
    Console.WriteLine("4. Додати користувача");
    Console.Write("Введіть номер дії: ");

    if (!int.TryParse(Console.ReadLine(), out action))
    {
        Console.WriteLine("Будь ласка, введіть коректне число.");
        continue;
    }
    switch (action)
    {
        case 1:
            databaseManager.ExecuteCreateTables();
            break;
        case 2:
            databaseManager.DropTables();
            break;
        case 3:
            databaseManager.PrintTableList();
            break;
        case 4:
            string email = "ivan@gmail.com";
            var user = databaseManager.GetUserByEmail(email);
            if (user != null)
            {
                Console.WriteLine($"Користувач з email {email} вже існує.");
                break;
            }
            databaseManager.InsertUsers(new List<User>
            {
                new User
                {
                    FirstName = "Іван",
                    LastName = "Бобер",
                    Email = "ivan@gmail.com",
                    Phone = "+380501234567",
                    Password = "123456"
                }
            });
            break;
        case 0:
            Console.WriteLine("Вихід з програми.");
            break;
        default:
            Console.WriteLine("Невідома дія. Спробуйте ще раз.");
            break;
    }
} while (action != 0);
