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
            while (true) //the main loop
            {
                if (login)
                {
                    LoginMenu(); //login menu first
                    login = false; //prevent login menu from showing after every loop
                }
                switch (MainMenu()) //after LoginMenu function is complete, load main menu
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
                    case 7: //Exit
                        login = true; //login menu will show in next loop
                        break;
                }
                //repeat until console is closed
            }
        }

        static void Table(string title, string subtitle = "", string content = "") //dynamically draw tables, content = .txt file path for table body
        {
            pos.Clear(); //clear any previously stored positions
            Console.Clear(); //clear any previous display
            int x, y; //initialise cursor pos for input fields

            //header
            string border = new('═', width);
            Console.WriteLine($"╔{border}╗");
            Console.WriteLine($"║{CentreText(title)}║");
            Console.WriteLine($"╠{border}╣");

            //subtitle
            string lineBreak = $"║{new String(' ', width)}║";
            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                Console.WriteLine($"║{CentreText(subtitle)}║");
                y = 5;
            }
            else
                y = 4;
            Console.WriteLine(lineBreak);

            //body
            foreach (string line in File.ReadLines(content)) //read menu template
            {
                string text = line;
                if (!string.IsNullOrWhiteSpace(line) && line[line.Length - 1] == '|') //check if line is an input field
                {
                    text = line.Remove(line.Length - 1); //remove delimiter
                    x = line.Length + 1; //increase x by 1 because of removing delimiter
                    pos.Add((x, y)); //store input field's input position
                }
                Console.WriteLine($"║{text}{new String(' ', width - text.Length)}║");
                y++;
            }
            Console.WriteLine(lineBreak);
            Console.WriteLine($"╚{border}╝");
        }

        static string CentreText(string text) //centres text within table
        {
            return String.Format("{0," + ((width / 2) + (text.Length / 2)) + "}" +
                                 "{1," + ((width / 2) - (text.Length / 2)) + "}",
                                 text, ""); //create padding before and after text
        }

        static void ClearAllFields(List<(int, int)> pos) //clears all input fields
        {
            for (int i = 0; i < pos.Count; i++) //for each input field pos
            {
                Console.SetCursorPosition(originX + pos[i].Item1, originY + pos[i].Item2); //set cursor pos to input field
                Console.Write(new String(' ', width - pos[i].Item1)); //replace here up to table width with whitespace
            }
        }

        static void ClearField((int, int) pos) 
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
            (int, int) errorMsgPos = Console.GetCursorPosition(); //store pos for error message, which at this time is below the table
            ResizeWindow(errorMsgPos); //use errorMsgPos as height for resize
            string[] credentials = new string[2]; //0 = username, 1 = password

            //username and password input loop
            while (true)
            {
                for (int i = 0; i < pos.Count; i++) //for each input field
                {
                    Console.SetCursorPosition(originX + pos[i].Item1, originY + pos[i].Item2); //go to its position
                    if (i == 0)
                        credentials[i] = Console.ReadLine(); //username input
                    else
                    {
                        //password input and masking loop
                        StringBuilder pw = new StringBuilder();
                        while (true)
                        {
                            var key = Console.ReadKey(true); //hide default typing
                            if (key.Key == ConsoleKey.Enter) 
                                break;
                            if (key.Key == ConsoleKey.Backspace && pw.Length > 0)
                            {
                                pw.Remove(pw.Length - 1, 1);
                                Console.CursorLeft--; //on the display, go back to the last char typed
                                Console.Write(' '); //replace it with emptiness
                                Console.CursorLeft--; //have to go back again because Write() makes cursor go forward one
                            }
                            else if (key.Key != ConsoleKey.Backspace) //if any other key was pressed
                            {
                                Console.Write("*"); //display keystroke as asterik
                                pw.Append(key.KeyChar); //append actual keystroke
                            }
                            //go again
                        }
                        credentials[i] = pw.ToString(); //enter was pressed, now pass it over to credentials array
                    }    
                }
                //attempt to find match
                foreach (string line in File.ReadLines("Storage/Login.txt"))
                {
                    string[] check = line.Split('|'); //split username and password by delimiter
                    if (check[0].Trim().ToLower().Equals(credentials[0].ToLower()) && check[1].Trim().Equals(credentials[1]))
                    {
                        return; //break out of function if found match...
                    }
                }
                credentials = new string[2]; //...otherwise they were invalid credentials...
                ClearAllFields(pos);
                WriteErrorMsg(errorMsgPos, "Incorrect username or password.");
                //...so go again
            }
        }

        static int MainMenu()
        {
            Table("Main Menu", "Options", "MenuTemplates/MainMenu.txt");
            (int, int) errorMsgPos = Console.GetCursorPosition();
            ResizeWindow(errorMsgPos);
            string choice;

            //keep looping until valid option is inputted
            while (true)
            {
                Console.SetCursorPosition(originX + pos[0].Item1, originY + pos[0].Item2); //go to input field position
                choice = Console.ReadLine();
                if (!int.TryParse(choice, out _) || 1 > Int32.Parse(choice) || Int32.Parse(choice) > 7) //check if input isnt an integer, otherwise then if it isnt in range
                {
                    ClearAllFields(pos);
                    WriteErrorMsg(errorMsgPos, "Invalid option; input a number between 1-7.");
                }
                else
                    break;
            }
            return Int32.Parse(choice); //return choice 
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
            try //try because WindowHeight/Width is incompatible on OSX
            {
                Console.WindowHeight = pos.Item2 + 3;
                Console.WindowWidth = width + 3;
                Console.SetCursorPosition(originX, originY); //scroll to top
            }
            catch (System.NotSupportedException) 
            {
                return;
            }
        }
    }
}
