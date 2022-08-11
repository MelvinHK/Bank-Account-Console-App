using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace _31927Assignment1
{
    class Program
    {
        const int width = 50; //width of table
        static int originX;
        static int originY;
        static List<(int, int)> pos = new List<(int, int)>(); //store cursor positions for input fields when creating tables

        static void Main(string[] args)
        {
            Console.Title = "Bank Account Management Console";
            Console.Clear();
            originX = Console.CursorLeft;
            originY = Console.CursorTop;
            bool login = true;
            while (true)
            {
                if (login)
                {
                    LoginMenu();
                    login = false;
                }
                switch (MainMenu())
                {
                    case 1:
                        CreateAccountMenu();
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    case 4:
                        break;
                    case 5:
                        break;
                    case 6:
                        break;
                    case 7:
                        login = true;
                        break;
                }
            }
        }

        static void Table(string title, string subtitle = "", string content = "") //dynamically draw tables
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
            int x; //initialise cursor pos for input fields
            int y = string.IsNullOrWhiteSpace(subtitle) ? 3 : 5; //start at 5 if there's a subtitle, otherwise 3
            foreach (string line in File.ReadLines(content))
            {
                string text = line;
                if (!string.IsNullOrWhiteSpace(line) && line[line.Length - 1] == '|') //check line is an input field
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

        static void ClearAllFields(List<(int, int)> pos) //clears input fields
        {
            for (int i = 0; i < pos.Count; i++)
            {
                Console.SetCursorPosition(originX + pos[i].Item1, originY + pos[i].Item2); //set cursor pos to input fields
                Console.Write(new String(' ', width - pos[i].Item1)); //replace up to the width of the table with whitespace
            }
        }

        static void ClearField((int, int) pos) //clears an input field
        {
            Console.SetCursorPosition(originX + pos.Item1, originY + pos.Item2);
            Console.Write(new String(' ', width - pos.Item1));
        }

        static void WriteErrorMsg((int, int) pos, string msg)
        {
            Console.SetCursorPosition(pos.Item1 + 1, pos.Item2 + 1);
            Console.WriteLine(CentreText(msg));
        }

        static void LoginMenu()
        {
            Table("Bank Account Management Console", "Login", "MenuTemplates/LoginMenu.txt");
            (int, int) errorMsgPos = Console.GetCursorPosition(); //store cursor pos for error message, which at this time is below the table
            ResizeWindow(errorMsgPos);
            string[] credentials = new string[2];
            while (true)
            {
                for (int i = 0; i < pos.Count; i++)
                {
                    Console.SetCursorPosition(originX + pos[i].Item1, originY + pos[i].Item2);
                    if (i == 1)
                    {
                        StringBuilder pw = new StringBuilder();
                        while (true)
                        {
                            var key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.Enter) 
                                break;
                            if (key.Key == ConsoleKey.Backspace && pw.Length > 0)
                            {
                                pw.Remove(pw.Length - 1, 1);
                                Console.CursorLeft--;
                                Console.Write(' ');
                                Console.CursorLeft--;
                            }
                            else if (key.Key != ConsoleKey.Backspace)
                            {
                                Console.Write("*");
                                pw.Append(key.KeyChar);
                            }
                        }
                        credentials[i] = pw.ToString();
                    } 
                    else
                        credentials[i] = Console.ReadLine();
                }
                foreach (string line in File.ReadLines("Storage/Login.txt"))
                {
                    string[] check = line.Split('|');
                    if (check[0].Trim().ToLower().Equals(credentials[0].ToLower()) && check[1].Trim().Equals(credentials[1]))
                    {
                        return;
                    }
                }
                credentials = new string[2];
                ClearAllFields(pos);
                WriteErrorMsg(errorMsgPos, "Incorrect username or password.");
            }
        }

        static int MainMenu()
        {
            Table("Main Menu", "Options", "MenuTemplates/MainMenu.txt");
            (int, int) errorMsgPos = Console.GetCursorPosition();
            ResizeWindow(errorMsgPos);
            string choice;
            while (true)
            {
                Console.SetCursorPosition(originX + pos[0].Item1, originY + pos[0].Item2);
                choice = Console.ReadLine();
                if (!int.TryParse(choice, out _) || 1 > Int32.Parse(choice) || Int32.Parse(choice) > 7)
                {
                    ClearAllFields(pos);
                    WriteErrorMsg(errorMsgPos, "Invalid option; input a number between 1-7.");
                }
                else
                    break;
            }
            return Int32.Parse(choice);
        }

        static void CreateAccountMenu()
        {
            Table("Create New Account", "Enter details", "MenuTemplates/NewAccountMenu.txt");
            (int, int) errorMsgPos = Console.GetCursorPosition();
            ResizeWindow(errorMsgPos);
            string[] credentials = new string[5];
            for (int i = 0; i < pos.Count; i++)
            {
                Console.SetCursorPosition(originX + pos[i].Item1, originY + pos[i].Item2);
                credentials[i] = Console.ReadLine();
            }
        }

        static void ResizeWindow((int, int) pos) //resize window according to table height and width
        {
            try
            {
                Console.WindowHeight = pos.Item2 + 3;
                Console.WindowWidth = width + 3;
                Console.SetCursorPosition(originX, originY); //scroll to top
            }
            catch (System.NotSupportedException) //WindowHeight/Width incompatible on OSX
            {
                return;
            }
        }
    }
}
