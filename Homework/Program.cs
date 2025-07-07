using Homework.Models;
using System.Diagnostics;
using System.Text;

namespace Homework;

internal class Program
{
    static void Main(string[] args)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        DatabaseService databaseService = new DatabaseService();

        Stopwatch stopWatch = new Stopwatch();
        TimeSpan ts = stopWatch.Elapsed;
        string elapsedTime = "";

        int action = -1;
        List<string> errors = new List<string>();
        List<string> success = new List<string>();

        List<User> users = new List<User>();

        do
        {
            Console.Clear();
            VisualHelper.PrintTitle("Виберіть дію");
            VisualHelper.PrintMenu(["Зрегенрувати випадкових користувачів", "Вивести всіх користувачів", "Пошук користувачів", "Редагування користувачів", "Вихід"], null, errors, success);
            errors.Clear();
            success.Clear();

            Console.Write("|   Вибір: ");
            if (!int.TryParse(Console.ReadLine(), out action))
            {
                errors.Add("Невірний пункт меню");
                continue;
            }

            string str = "";

            switch (action)
            {
                case 1:
                    do
                    {
                        Console.Clear();

                        int count = -1;

                        VisualHelper.PrintTitle("Генерація");
                        VisualHelper.PrintMenu(null, ["Введіть кількість користувачів,", "яку ви хочете згенерувати", "(0 щоб повернутися)"], errors, null);
                        errors.Clear();

                        Console.Write("|   Кількість: ");
                        if (!int.TryParse(Console.ReadLine(), out count))
                        {
                            errors.Add("Невірний пункт меню");
                            continue;
                        }

                        if (count >= 1)
                        {
                            stopWatch.Reset();
                            stopWatch.Start();

                            databaseService.InsertRandomUsers(count);

                            stopWatch.Stop();

                            ts = stopWatch.Elapsed;
                            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                                ts.Hours, ts.Minutes, ts.Seconds,
                                ts.Milliseconds / 10);

                            success.Add($"{count} користувачів створено за {elapsedTime}");
                            break;
                        }
                        else
                        {
                            errors.Add("Кількість не може бути меньше 1");
                        }
                    }
                    while (true);
                    break;
                case 2:
                    Console.Clear();

                    stopWatch.Reset();
                    stopWatch.Start();

                    users = databaseService.GetAllUsers();

                    stopWatch.Stop();
                    ts = stopWatch.Elapsed;
                    elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);


                    VisualHelper.PrintTitle("Всі користувачі");
                    VisualHelper.PrintMenu(null, ["Enter щоб повернутися"], null, [$"Всіх користувачів отримано за {elapsedTime}"]);

                    foreach (var user in users)
                    {
                        Console.WriteLine($"|   #{user.Id} {user.LastName} {user.FirstName} ->\t{user.Phone}\t{user.Email}");
                    }

                    Console.ReadLine();

                    break;
                case 3:
                    do
                    {
                        Console.Clear();
                        VisualHelper.PrintTitle("Пошук");
                        VisualHelper.PrintMenu(null, ["Введіть будь-яку інформації", "про користувача", "", "(Індетифікатор, Ім'я, Прізвище,", "Телефон, Електронну пошту)", "", "(0 для виходу)"], errors, null);
                        errors.Clear();

                        Console.Write("|\n|   Інформація для пошуку: ");

                        str = Console.ReadLine() ?? string.Empty;
                        if (str == "0")
                        {
                            break;
                        }


                        Console.Clear();

                        stopWatch.Reset();
                        stopWatch.Start();

                        users = databaseService.SearchUsers(str);

                        stopWatch.Stop();
                        ts = stopWatch.Elapsed;
                        elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            ts.Hours, ts.Minutes, ts.Seconds,
                            ts.Milliseconds / 10);


                        VisualHelper.PrintTitle("Найдені користувачі");
                        VisualHelper.PrintMenu(null, ["Enter щоб повернутися"], null, [$"Користувачів отримано за {elapsedTime}"]);

                        foreach (var user in users)
                        {
                            Console.WriteLine($"|   #{user.Id} {user.LastName} {user.FirstName} ->\t{user.Phone}\t{user.Email}");
                        }

                        Console.ReadLine();
                    }
                    while (true);
                    break;
                case 4:
                    int subAction;
                    do
                    {
                        Console.Clear();
                        VisualHelper.PrintTitle("Редагування користувачів");
                        VisualHelper.PrintMenu(
                            ["Змінити користувача", "Видалити користувача"],
                            ["0 щоб повернутися"],
                            errors, success);
                        errors.Clear();
                        success.Clear();

                        Console.Write("|   Вибір: ");
                        if (!int.TryParse(Console.ReadLine(), out subAction))
                        {
                            errors.Add("Невірний пункт меню");
                            continue;
                        }

                        switch (subAction)
                        {
                            case 1:
                                Console.Write("\n|   ID користувача: ");
                                if (!int.TryParse(Console.ReadLine(), out int updateId))
                                {
                                    errors.Add("ID має бути числом");
                                    break;
                                }

                                var updateUser = new User();

                                Console.Write("|   Нове ім'я (Enter – без змін): ");
                                string? tmp = Console.ReadLine();
                                if (!string.IsNullOrEmpty(tmp)) updateUser.FirstName = tmp;

                                Console.Write("|   Нове прізвище (Enter – без змін): ");
                                tmp = Console.ReadLine();
                                if (!string.IsNullOrEmpty(tmp)) updateUser.LastName = tmp;

                                Console.Write("|   Новий e‑mail (Enter – без змін): ");
                                tmp = Console.ReadLine();
                                if (!string.IsNullOrEmpty(tmp)) updateUser.Email = tmp;

                                Console.Write("|   Новий телефон (Enter – без змін): ");
                                tmp = Console.ReadLine();
                                if (!string.IsNullOrEmpty(tmp)) updateUser.Phone = tmp;

                                Console.Write("|   Новий пароль (Enter – без змін): ");
                                tmp = Console.ReadLine();
                                if (!string.IsNullOrEmpty(tmp)) updateUser.Password = tmp;

                                try
                                {
                                    databaseService.UpdateUser(updateId, updateUser);
                                    success.Add("Користувача оновлено");
                                }
                                catch (Exception ex)
                                {
                                    errors.Add($"Невідома помилка: {ex.Message}");
                                }
                                break;

                            case 2:
                                Console.Write("\n|   ID користувача: ");
                                if (!int.TryParse(Console.ReadLine(), out int delId))
                                {
                                    errors.Add("ID має бути числом");
                                    break;
                                }
                                Console.Write("|   Дійсно видалити? (y/n): ");
                                if (Console.ReadLine()?.Trim().ToLower() == "y")
                                {
                                    try
                                    {
                                        databaseService.DeleteUser(delId);
                                        success.Add("Користувача видалено");
                                    }
                                    catch (Exception ex)
                                    {
                                        errors.Add($"Невідома помилка: {ex.Message}");
                                    }
                                }
                                break;

                            case 0:
                                break;

                            default:
                                errors.Add("Невірний пункт меню");
                                break;
                        }
                    }
                    while (subAction != 0);
                    break;
                case 5:
                    break;
                default:
                    errors.Add("Невірний пункт меню");
                    break;
            }
        }
        while (action != 5);
    }
}
