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
                    case 0:
                        break;
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

        static void Table(string title, string subtitle = "", string content = "") //draw table
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
                if (!string.IsNullOrWhiteSpace(line) && line[line.Length-1] == '|') //check if the line is an input field
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
            Console.SetCursorPosition(pos.Item1, pos.Item2 + 1);
            Console.WriteLine(msg);
        }

        static void LoginMenu()
        {
            Table("Bank Account Management Console", "Login", "MenuTemplates/LoginMenu.txt");
            (int, int) errorMsgPos = Console.GetCursorPosition(); //store cursor pos for error message, which at this time is below the table
            string[] credentials = new string[2];
            while (true)
            {
                for (int i = 0; i < pos.Count; i++)
                {
                    Console.SetCursorPosition(originX + pos[i].Item1, originY + pos[i].Item2);
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
                ClearAllFields(pos);
                WriteErrorMsg(errorMsgPos, "Incorrect username or password.");
            }
        }

        static int MainMenu()
        {
            Table("Main Menu", "Options", "MenuTemplates/MainMenu.txt");
            (int, int) errorMsgPos = Console.GetCursorPosition();
            string choice;
            while (true)
            {
                Console.SetCursorPosition(originX + pos[0].Item1, originY + pos[0].Item2);
                choice = Console.ReadLine();
                if (!int.TryParse(choice, out _) || !(Int32.Parse(choice) >= 1 && Int32.Parse(choice) <= 7))
                {
                    {
                        ClearAllFields(pos);
                        WriteErrorMsg(errorMsgPos, "Invalid option; please input a number between 1-7.");
                    }
                }
                else
                    break;
            }
            return Int32.Parse(choice);
        }

        static void CreateAccountMenu()
        {
            string[] credentials = new string[5];
            Table("Create New Account", "Enter details", "MenuTemplates/NewAccountMenu.txt");
            for (int i = 0; i < pos.Count; i++)
            {
                Console.SetCursorPosition(originX + pos[i].Item1, originY + pos[i].Item2);
                credentials[i] = Console.ReadLine();
            }
        }
    }
}
