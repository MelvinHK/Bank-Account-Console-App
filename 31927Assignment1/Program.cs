using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace _31927Assignment1
{
    class Program
    {
        const int width = 70; //width of table
        static int originX;
        static int originY;
        static List<(int, int)> pos = new List<(int, int)>(); //store cursor positions for input fields when creating tables

        static void Main(string[] args)
        {
            Console.Title = "Bank Account Management Console";
            Console.Clear();
            originX = Console.CursorLeft;
            originY = Console.CursorTop;
            LoginMenu();
        }

        static void Table(string title, string subtitle = "", string content = "") //dynamic table drawing
        {
            pos.Clear(); //clear any previously stored positions
            Console.Clear(); //clear any previous display

            string border = new('═', width);
            Console.WriteLine($"╔{border}╗");
            Console.WriteLine($"║{CentreText(title)}║");
            Console.WriteLine($"╠{border}╣");

            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                Console.WriteLine($"║{CentreText(subtitle)}║");
                Console.WriteLine($"║{new String(' ', width)}║");
            }

            int x; //initialise cursor positions for input fields
            int y = string.IsNullOrWhiteSpace(subtitle) ? 3 : 5;
            foreach (string line in File.ReadLines(content))
            {
                string text = line;
                if (line.Contains('|')) //check if the line is an input field
                {
                    text = line.Remove(line.Length - 1); //remove delimiter
                    x = line.Length + 1;
                    pos.Add((x, y));
                }
                Console.WriteLine($"║{text}{new String(' ', width - text.Length)}║");
                y++;
            }

            Console.WriteLine($"╚{border}╝");
        }

        static string CentreText(string text) //centres text within table
        {
            return String.Format("{0," + ((width / 2) + (text.Length / 2)) + "}" +
                                 "{1," + ((width / 2) - (text.Length / 2)) + "}",
                                 text, ""); //create padding before and after text
        }

        static void LoginMenu(bool err = false, string msg = "")
        {
            string[] credentials = new string[2];
            Table("Bank Account Management Console", "Login", "MenuTemplates/LoginMenu.txt");
            if (err)
                Console.WriteLine("\n" + msg);
            for (int i = 0; i < pos.Count; i++)
            {
                Console.SetCursorPosition(originX + pos[i].Item1, originY + pos[i].Item2);
                credentials[i] = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(credentials[i]))
                    if (i == 0)
                        LoginMenu(true, "The username field is required.");
                    else
                        LoginMenu(true, "The password field is required.");
            }
            foreach (string line in File.ReadLines("Storage/Login.txt"))
            {
                string[] check = line.Split('|');
                Console.WriteLine(check[0] + " " + check[1]);
                if (check[0].Trim().ToLower().Equals(credentials[0].ToLower()) && check[1].Trim().Equals(credentials[1]))
                {
                    MainMenu();
                    return;
                }
            }
            LoginMenu(true, "Incorrect username or password.");
        }

        static void MainMenu(bool err = false, string msg = "")
        {
            Table("Main Menu", "Options:", "MenuTemplates/MainMenu.txt");
            if (err)
                Console.WriteLine("\n" + msg);
            Console.SetCursorPosition(originX + pos[0].Item1, originY + pos[0].Item2);
            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    break;
                case "2":
                    break;
                case "3":
                    break;
                case "4":
                    break;
                case "5":
                    break;
                case "6":
                    break;
                case "7":
                    LoginMenu();
                    break;
                default:
                    MainMenu(true, "Invalid option; please a input a number between 1 - 7.");
                    break;
            }
        }
    }
}
