using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel.DataAnnotations;


namespace _31927Assignment1
{
    class Program
    {
        const int width = 50; //width of table
        static int originX;
        static int originY;
        static List<(int, int)> inputPos = new List<(int, int)>(); //store cursor positions for input fields when creating tables

        static void Main(string[] args)
        {
            Console.Title = "Bank Account Management Console";
            originX = Console.CursorLeft;
            originY = Console.CursorTop;
            bool login = true;
            while (true) //main program loop
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

    //HELPER FUNCTIONS
        static void Table(string title, string subtitle = "", string content = "") //dynamically draw tables, content = .txt file path for table body (folder: MenuTemplates)
        {
            inputPos.Clear(); //clear any previously stored positions
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
                    text = line.Remove(line.Length - 1); //remove delimiter, see LoginMenu.txt
                    x = line.Length + 1; //x position = end of string, + 1 for space
                    inputPos.Add((x, y)); //store input field's input position
                }
                Console.WriteLine($"║{text}{new String(' ', width - text.Length)}║");
                y++;
            }
            Console.WriteLine(lineBreak);
            Console.WriteLine($"╚{border}╝");
        }

        static string CentreText(string text) //centres text WITHIN table (not for centering in console window)
        {
            return String.Format("{0," + ((width / 2) + (text.Length / 2)) + "}" +
                                 "{1," + ((width / 2) - (text.Length / 2)) + "}",
                                 text, ""); //create padding before and after text
        }

        static void ClearAllFields(List<(int, int)> inputPos) //clears all input fields
        {
            foreach ((int, int) pos in inputPos) //for each input field pos
            {
                Console.SetCursorPosition(originX + pos.Item1, originY + pos.Item2); //set cursor pos to input field
                Console.Write(new String(' ', width - pos.Item1)); //replace here up to table width with whitespace
            }
        }

        static void ClearField(int x, int y) //clears an input field and maintains cursor pos
        {
            Console.SetCursorPosition(originX + x, originY + y);
            Console.Write(new String(' ', width - x)); 
            Console.SetCursorPosition(originX + x, originY + y);
        }

        static void WritePrompt((int, int) pos, string msg, bool newLine = true) //for writing prompts or error messages
        {
            Console.SetCursorPosition(((width - msg.Length) / 2) + 1, pos.Item2 + 1); //write at centre of console window
            if (newLine)
                Console.WriteLine(msg);
            else
                Console.Write(msg);
        }

        private static string ReadLineWithCancel() //returns null if esc is pressed during input (useful for going back to main menu)
        {
            string input;
            StringBuilder line = new StringBuilder();
            var key = Console.ReadKey(true);

            while (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Escape)
            {
                if (key.Key == ConsoleKey.Backspace && line.Length > 0)
                {
                    line.Remove(line.Length - 1, 1);
                    Console.CursorLeft--;
                    Console.Write(' ');
                    Console.CursorLeft--;
                }
                else if (key.Key != ConsoleKey.Backspace) //have to include this else if, otherwise the user can keep pressing backspace past the input field...
                {
                    Console.Write(key.KeyChar);
                    line.Append(key.KeyChar);
                }
                key = Console.ReadKey(true);
            }
            return input = key.Key == ConsoleKey.Enter ? line.ToString() : null;
        }

        static void ResizeWindow((int, int) pos) //resize window according to table height and width
        {
            try //try because set WindowHeight/Width is incompatible on OSX
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

    //MENU FUNCTIONS
        static void LoginMenu()
        {
            Table("Bank Account Management Console", "Login", "MenuTemplates/LoginMenu.txt");
            (int, int) errorMsgPos = Console.GetCursorPosition(); //store pos for error message, which at this time is below the table
            ResizeWindow(errorMsgPos); //use errorMsgPos as height for resize
            string[] credentials = new string[2]; //0 = username, 1 = password

            //username and password input loop
            while (true)
            {
                for (int i = 0; i < inputPos.Count; i++) //for each input field
                {
                    Console.SetCursorPosition(originX + inputPos[i].Item1, originY + inputPos[i].Item2); //go to its position
                    if (i == 0)
                        credentials[i] = Console.ReadLine(); //username input
                    else
                    {
                        //password input and masking loop
                        var key = Console.ReadKey(true); //read keystroke (true = doesnt display keystroke)
                        StringBuilder input = new StringBuilder();
                        while (key.Key != ConsoleKey.Enter)
                        {
                            if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                            {
                                input.Remove(input.Length - 1, 1);
                                Console.CursorLeft--; //on the display, go back to the last char typed
                                Console.Write(' '); //replace it with emptiness
                                Console.CursorLeft--; //have to go back again because Write() makes cursor go forward one
                            }
                            else if (key.Key != ConsoleKey.Backspace) //if any other key was pressed
                            {
                                Console.Write("*"); //display keystroke as asterik
                                input.Append(key.KeyChar); //append actual keystroke
                            }
                            key = Console.ReadKey(true); //read next keystroke
                        }
                        credentials[i] = input.ToString(); //enter was pressed, now pass it over to credentials array
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
                ClearAllFields(inputPos);
                WritePrompt(errorMsgPos, "Incorrect username or password.");
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
                Console.SetCursorPosition(originX + inputPos[0].Item1, originY + inputPos[0].Item2); //go to input field position
                choice = Console.ReadLine();
                if (!int.TryParse(choice, out _) || 1 > Int32.Parse(choice) || Int32.Parse(choice) > 7) //check if input isnt an integer; if it is, check if it isnt in range
                {
                    ClearAllFields(inputPos);
                    WritePrompt(errorMsgPos, "Invalid option; input a number between 1-7.");
                }
                else break;
            }
            return Int32.Parse(choice); //return choice 
        }

        static void CreateAccountMenu()
        {
            Table("Create New Account", "Enter details", "MenuTemplates/NewAccountMenu.txt");
            (int, int) errorMsgPos = Console.GetCursorPosition();
            Console.SetCursorPosition(2, 1);
            Console.Write("< Esc");
            ResizeWindow(errorMsgPos);
            string[] credentials = new string[5];
            string input;

            while (true)
            {
                for (int i = 0; i < inputPos.Count; i++)
                {
                    Console.SetCursorPosition(originX + inputPos[i].Item1, originY + inputPos[i].Item2);
                    input = ReadLineWithCancel(); //if esc is pressed, returns null
                    if (input == null) //menu is exited
                        return;
                    while (i == 3 || i == 4) //phone (3) and email (4) validation
                    {
                        if (i == 3)
                        {
                            if (!(int.TryParse(input, out _) && input.Length <= 10))
                                WritePrompt(errorMsgPos, "Invalid phone format.");
                            else break;
                        }
                        else
                        {
                            if (!new EmailAddressAttribute().IsValid(input)) //check for gmail.com, student email, outlook.com later
                                WritePrompt(errorMsgPos, "Invalid email format.");
                            else break;
                        }
                        ClearField(inputPos[i].Item1, inputPos[i].Item2);
                        input = ReadLineWithCancel();
                    }
                    credentials[i] = input;
                }
                WritePrompt(errorMsgPos, "Confirm details (Y/N)", false);
                while (true)
                {
                    ConsoleKeyInfo choice = Console.ReadKey(true);
                    if (choice.KeyChar.Equals('y'))
                    {
                        Random generator = new Random();
                        string id = generator.Next(0, 1000000).ToString("D6");
                        //check if id is unique (nest below in for loop < 1000000. if for loop becomes finished = max capacity)
                        ResizeWindow(Console.GetCursorPosition());
                        WritePrompt(errorMsgPos, $"Account created (id: {id})");
                        WritePrompt((errorMsgPos.Item1, errorMsgPos.Item2 + 1), "Details sent to email.", false);

                        Console.ReadKey(true);
                        //add details to id.txt file
                        return;
                    }
                    if (choice.KeyChar.Equals('n'))
                    {
                        credentials = new string[5];
                        ClearAllFields(inputPos);
                        WritePrompt(errorMsgPos, new String(' ', width));
                        break;
                    }
                    if (choice.Key == ConsoleKey.Escape)
                        return;
                }
            }
        }
    }
}
