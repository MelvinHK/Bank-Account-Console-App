﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;


namespace _31927Assignment1
{
    class Program
    {
        const int width = 50; //width of table
        static List<(int, int)> inputPos = new List<(int, int)>(); //store cursor positions for input fields when creating tables

        static void Main(string[] args)
        {
            Console.Title = "Bank Account Management Console";
            bool auth = false;
            while (true) //main program loop
            {
                if (!auth)
                {
                    LoginMenu(); //login menu first
                    auth = true;
                }
                switch (MainMenu()) //after successful login, load main menu
                {
                    case 1:
                        NewAccountMenu();
                        break;
                    case 2:
                        SearchMenu();
                        break;
                    case 3:
                        TransactionMenu(); //deposit
                        break;
                    case 4:
                        TransactionMenu(false); //withdraw
                        break;
                    case 5:
                        break;
                    case 6:
                        break;
                    case 7: //Exit
                        auth = false; //login menu will show in next loop
                        break;
                }
                //repeat until console is closed
            }
        }

    //HELPER FUNCTIONS
        static void Table(string title, string subtitle = "", string file = "", bool esc = false) //dynamically draw tables, file = .txt file path for table body (folder: MenuTemplates)
        {
            inputPos.Clear(); //clear any previously stored positions
            Console.Clear(); //clear any previous display
            int inputX, inputY; //initialise cursor pos for input fields
            string path = @"..\..\..\MenuTemplates\" + file;

            //header
            string border = new('═', width);
            Console.WriteLine($"╔{border}╗");
            Console.WriteLine($"║{CentreText(title)}║");
            if (esc) //show escape indicator for going back to main menu
            {
                Console.SetCursorPosition(2, 1);
                Console.WriteLine("< Esc");
            }
            Console.WriteLine($"╠{border}╣");

            //subtitle
            string lineBreak = $"║{new String(' ', width)}║";
            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                Console.WriteLine($"║{CentreText(subtitle)}║");
                inputY = 5;
            }
            else
                inputY = 4;
            Console.WriteLine(lineBreak);

            //body
            foreach (string line in File.ReadLines(path)) //read menu template
            {
                string text = line;
                if (!string.IsNullOrWhiteSpace(line) && line[line.Length - 1] == '|') //check if line is an input field
                {
                    text = line.Remove(line.Length - 1); //remove delimiter (|)
                    inputX = line.Length; //x position at the end of the string
                    inputPos.Add((inputX, inputY)); //store position
                }
                Console.WriteLine($"║{text}{new String(' ', width - text.Length)}║");
                inputY++;
            }
            Console.WriteLine(lineBreak);
            Console.WriteLine($"╚{border}╝");
            ResizeWindow(Console.GetCursorPosition()); //resize console window to table size
        }

        static string CentreText(string text) //centres text WITHIN table (not for centering in console window)
        {
            return String.Format("{0," + ((width / 2) + (text.Length / 2)) + "}" +
                                 "{1," + ((width / 2) - (text.Length / 2)) + "}",
                                 text, ""); //create padding before and after text
        }

        static void ClearAllFields(List<(int, int)> inputPos) //clears all input fields
        {
            Console.CursorVisible = false; //hide cursor to prevent any flickering as it jumps to new positions
            foreach ((int, int) pos in inputPos) //for each input field pos
            {
                Console.SetCursorPosition(pos.Item1, pos.Item2); //set cursor pos to input field
                Console.Write(new String(' ', width - pos.Item1)); //replace here up to table width with whitespace
            }
            Console.CursorVisible = true;
        }

        static void ClearField(int x, int y) //clears an input field and maintains cursor pos
        {
            Console.SetCursorPosition(x, y);
            Console.Write(new String(' ', width - x)); 
            Console.SetCursorPosition(x, y);
        }

        static void WritePrompt((int, int) pos, string msg, bool newLine = true) //for writing prompts or error messages
        {
            Console.CursorVisible = false; 
            Console.SetCursorPosition(0, pos.Item2 + 1); //clear any previous message that is in the same Y position
            Console.WriteLine(new String(' ', width));
            Console.SetCursorPosition(((width - msg.Length) / 2) + 1, pos.Item2 + 1); //write at centre of console window
            if (newLine)
                Console.WriteLine(msg);
            else
                Console.Write(msg);
            Console.CursorVisible = true;
        }

        private static string ReadLineWithCancel() //Console.ReadLine(), except returns null if esc is pressed during input (useful for going back to main menu)
        {
            string input;
            StringBuilder line = new StringBuilder();
            var key = Console.ReadKey(true);

            while ((key.Key != ConsoleKey.Enter) && key.Key != ConsoleKey.Escape)
            {
                if (key.Key == ConsoleKey.Backspace && line.Length > 0)
                {
                    line.Remove(line.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else if (key.Key != ConsoleKey.Backspace && key.KeyChar != '\u0000' && key.Key != ConsoleKey.Tab) //u0000 = arrow keys
                {
                    Console.Write(key.KeyChar);
                    line.Append(key.KeyChar);
                }
                key = Console.ReadKey(true);
            }
            return input = key.Key == ConsoleKey.Enter ? line.ToString().Trim() : null;
        }

        static void ResizeWindow((int, int) pos) //resize window according to table height and width
        {
            try //try because set WindowHeight/Width is incompatible on OSX
            {
                Console.WindowHeight = pos.Item2 + 3;
                Console.WindowWidth = width + 3;
                Console.SetCursorPosition(0, 0); //scroll to top
                Console.SetCursorPosition(pos.Item1, pos.Item2); //then set cursor back to former position
            }
            catch (System.NotSupportedException)
            {
                return;
            }
        }

        static string[] GetAccount(string id) //0: id, 1: name, 2: last, 3: addr, 4: phone, 5: email, 6: balance
        {
            try
            {
                return File.ReadAllLines(@"..\..\..\Storage\BankAccounts\" + $"{id}.txt");
            } 
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        static void UpdateLine(string id, int index, string newText)
        {
            string path = @"..\..\..\Storage\BankAccounts\" + $"{id}.txt";
            string[] lines = File.ReadAllLines(path);
            lines[index] = newText;
            File.WriteAllLines(path, lines);
        }

        static void SendEmail(int choice, string id) //send welcome details or bank account statement to acc id's email (assumes id exists)
        {
            string path = @"..\..\..\Storage\BankAccounts\" + $"{id}.txt";
            string[] credentials = GetAccount(id);
            
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential("bamc31927@gmail.com", "vlrmjsvaanlzpfwj");
            smtp.EnableSsl = true;

            MailMessage mail = new MailMessage(new MailAddress("bamc31927@gmail.com"),
                                               new MailAddress(credentials[5]));

            if (choice == 1) //welcome
            {
                mail.Subject = "Welcome to BAMC";
                mail.Body = $"Hi {credentials[1]}, welcome to BAMC. Your account details are as follows: \n\n" +
                            $"Account ID: {credentials[0]} \n" +
                            $"First name: {credentials[1]} \n" +
                            $"Last name: {credentials[2]} \n" +
                            $"Address: {credentials[3]} \n" +
                            $"Phone: {credentials[4]} \n" +
                            $"Email: {credentials[5]} \n\n" +
                            $"BAMC";
            }
            if (choice == 2) //account statment
            {
                mail.Subject = "";
            }
            smtp.Send(mail);
        }

    //MENU FUNCTIONS
        static void LoginMenu()
        {
            Table("Bank Account Management Console", "Login", "LoginMenu.txt");
            (int, int) errorMsgPos = Console.GetCursorPosition(); //store pos for error message, which at this time is below the table
            string[] credentials = new string[2]; //0 = username, 1 = password

            Start: //begin username and password input
            for (int i = 0; i < inputPos.Count; i++) //for each input field
            {
                Console.SetCursorPosition(inputPos[i].Item1, inputPos[i].Item2); //go to its position
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
                            input.Remove(input.Length - 1, 1); //remove last char
                            Console.Write("\b \b");
                        }
                        else if (key.Key != ConsoleKey.Backspace && key.KeyChar != '\u0000') //if any other key was pressed (and disable arrow keys)
                        {
                            Console.Write("*"); //display keystroke as asterik
                            input.Append(key.KeyChar); //append actual keystroke
                        }
                        key = Console.ReadKey(true); //read next keystroke
                    }
                    credentials[i] = input.ToString(); //enter was pressed, pass it over to credentials array
                }
            }
            //username and password input complete, attempt to find match
            foreach (string line in File.ReadLines(@"..\..\..\Storage\Login.txt"))
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
            goto Start; //...so go again
        }

        static int MainMenu()
        {
            Table("Main Menu", "Options", "MainMenu.txt");
            (int, int) errorMsgPos = Console.GetCursorPosition();
            string choice;
            //keep looping until valid option is inputted
            while (true)
            {
                Console.SetCursorPosition(inputPos[0].Item1, inputPos[0].Item2); //go to input field position
                choice = Console.ReadLine();
                if (!int.TryParse(choice, out _) || 1 > Int32.Parse(choice) || Int32.Parse(choice) > 7) //check if input isnt an integer; if it is, check if it isnt in range
                {
                    ClearAllFields(inputPos);
                    WritePrompt(errorMsgPos, "Invalid option number.");
                }
                else break;
            }
            return Int32.Parse(choice); //return choice 
        }

        static void NewAccountMenu()
        {
            Table("Create New Account", "Enter details", "NewAccountMenu.txt", true);
            (int, int) errorMsgPos = Console.GetCursorPosition();
            var domains = new HashSet<string> { "@gmail.com", "@outlook.com", "@uts.edu.au", "@student.uts.edu.au"}; //valid email domains
            string[] credentials = new string[5];
            string input;

            Start:
            for (int i = 0; i < inputPos.Count; i++)
            {
                Console.SetCursorPosition(inputPos[i].Item1, inputPos[i].Item2);
                input = ReadLineWithCancel(); //if esc is pressed, returns null

                //phone and email validation loop
                while ((i == 3 || i == 4) && input != null)
                {
                    if (i == 3)
                    {
                        if (!(Int64.TryParse(input, out _) && input.Length == 10))
                            WritePrompt(errorMsgPos, "Invalid phone number.");
                        else break;
                    }
                    else
                    {
                        if (!new EmailAddressAttribute().IsValid(input) || !domains.Contains(input.Substring(input.IndexOf('@'))))
                            WritePrompt(errorMsgPos, "Invalid email address.");
                        else break;
                    }
                    ClearField(inputPos[i].Item1, inputPos[i].Item2);
                    input = ReadLineWithCancel();
                }
                if (input == null) return; //exit menu if esc was pressed
                credentials[i] = input;
            }
            //confirm loop (keep reading keystroke until y or n is pressed)
            WritePrompt(errorMsgPos, "Confirm details (Y/N)", false);
            while (true)
            {
                ConsoleKeyInfo choice = Console.ReadKey(true);
                if (choice.KeyChar.Equals('y'))
                {
                    int cap = 1000000;
                    string id = new Random().Next(0, cap).ToString("D6"); //generate account id (D6 = with leading zeros)
                    string path = @"..\..\..\Storage\BankAccounts";
                    var fileSet = new HashSet<string>(Directory
                                                     .GetFiles(path, "*", SearchOption.AllDirectories)
                                                     .Select(f => Path.GetFileName(f))); //get array of filenames, turn into hashset for fast lookup
                    Stopwatch timer = new();
                    timer.Start();
                    WritePrompt(errorMsgPos, "Loading...", false);
                    //loop while id isnt unique
                    while (fileSet.Contains(id + ".txt"))
                    {
                        if (timer.ElapsedMilliseconds > 2000) //abort if it takes longer than 2 seconds, max accounts likely reached
                        {
                            WritePrompt(errorMsgPos, "Unable to create new accounts.", false);
                            Console.ReadKey(true);
                            return;
                        }
                        id = new Random().Next(0, cap).ToString("D6"); //try again with new id
                    }
                    //create {id}.txt file
                    using (StreamWriter sw = File.CreateText($"{path}/{id}.txt"))
                    {
                        sw.WriteLine(id); //line 0: acc id
                        foreach (string line in credentials) //1: name, 2: last, 3: addr, 4: phone, 5: email
                        {
                            sw.WriteLine(line);
                        }
                        sw.WriteLine(0); //6: starting balance
                    }
                    //send email
                    ResizeWindow(Console.GetCursorPosition()); //resize window to look good for two prompt lines
                    WritePrompt(errorMsgPos, $"Account created (id: {id})");
                    WritePrompt((errorMsgPos.Item1, errorMsgPos.Item2 + 1), "Sending details to email, please wait...", false);
                    SendEmail(1, id);
                    WritePrompt((errorMsgPos.Item1, errorMsgPos.Item2 + 1), "Details sent. Press any key to continue.", false);
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key.Equals(ConsoleKey.Escape)) return;
                    else break;
                }
                if (choice.KeyChar.Equals('n')) break;
            }
            credentials = new string[5];
            WritePrompt(errorMsgPos, ""); //clear prompt messages
            Console.Write(new String(' ', width));
            ClearAllFields(inputPos);
            goto Start;
        }

        static void SearchMenu()
        {
            Start:
            Table("Search Account", "Enter 6 digits", "SearchMenu.txt", true);
            (int, int) errorMsgPos = Console.GetCursorPosition();
            string[] credentials;
            string input;

            while (true)
            {
                Console.SetCursorPosition(inputPos[0].Item1, inputPos[0].Item2);
                input = ReadLineWithCancel();
                if (input == null) return;
                credentials = GetAccount(input);
                if (credentials == null)
                {
                    ClearAllFields(inputPos);
                    WritePrompt(errorMsgPos, "Account does not exist.");
                }
                else break;
            }
            Table("Account Found", $"ID: {input}", "AccountFoundMenu.txt", true);
            errorMsgPos = Console.GetCursorPosition();
            for (int i = 1; i < inputPos.Count + 1; i++)
            {
                Console.SetCursorPosition(inputPos[i-1].Item1, inputPos[i-1].Item2);
                Console.Write(credentials[i]);
            }
            WritePrompt(errorMsgPos, "Press any key to continue.", false);
            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key.Equals(ConsoleKey.Escape)) return;
            else goto Start;
        }

        static void TransactionMenu(bool deposit = true)
        {
            string title = deposit ? "Deposit" : "Withdraw";
            Table(title, "Enter details", "TransactionMenu.txt", true);
            (int, int) errorMsgPos = Console.GetCursorPosition();
            string[] credentials = Array.Empty<string>();
            string input = "";
            string id = "";

            Start:
            for (int i = 0; i < inputPos.Count; i++)
            {
                Console.SetCursorPosition(inputPos[i].Item1, inputPos[i].Item2);
                input = ReadLineWithCancel(); //if esc is pressed, returns null
                while (input != null)
                {
                    if (i == 0) //account validation
                    {
                        credentials = GetAccount(input);
                        if (credentials == null)
                            WritePrompt(errorMsgPos, "Account does not exist.");
                        else
                        {
                            id = input;
                            WritePrompt(errorMsgPos, $"Account found.", false);
                            ResizeWindow(Console.GetCursorPosition());
                            WritePrompt((errorMsgPos.Item1, errorMsgPos.Item2 + 1),$"Current Balance: ${credentials[6]}");
                            break;
                        }
                    }
                    else //amount validation
                    {
                        //first check if amount = positive double. then, if not depositing, check if withdrawal amount < balance.
                        if (double.TryParse(input, out double amount) && amount > 0 && (deposit || double.Parse(credentials[6]) - amount >= 0))
                        {
                            input = !deposit ? (amount *= -1).ToString() : input; //convert to negative if withdrawing
                            break;
                        }
                        else WritePrompt(errorMsgPos, $"Invalid amount.");
                    }
                    ClearField(inputPos[i].Item1, inputPos[i].Item2);
                    input = ReadLineWithCancel();
                }
                if (input == null) return;
            }

            string updated = Math.Round(double.Parse(credentials[6]) + double.Parse(input), 2).ToString("0.00");
            UpdateLine(id, 6, updated);
            WritePrompt(errorMsgPos, $"Updated balance: ${updated}", false);
            WritePrompt((errorMsgPos.Item1, errorMsgPos.Item2 + 1), "Press any key to continue.", false);
            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.Key.Equals(ConsoleKey.Escape)) return;

            ClearAllFields(inputPos);
            WritePrompt(errorMsgPos, "");
            Console.WriteLine(new String(' ', width));
            goto Start;
        }
    }
}
