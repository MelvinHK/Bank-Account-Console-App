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
        const int width = 50;
        const string accountsPath = @"..\..\..\Storage\BankAccounts\";
        const string templatesPath = @"..\..\..\MenuTemplates\";
        static List<(int, int)> inputPos = new List<(int, int)>(); //store cursor positions for input fields

        static void Main(string[] args)
        {
            Console.Title = "Bank Account Management Console";
            bool auth = false;
            while (true)
            {
                if (!auth)
                {
                    LoginMenu();
                    auth = true;
                }
                switch (MainMenu())
                {
                    case 1:
                        NewAccountMenu();
                        break;
                    case 2:
                        SearchMenu();
                        break;
                    case 3: //deposit
                        TransactionMenu(); 
                        break;
                    case 4: //withdraw
                        TransactionMenu(false); 
                        break;
                    case 5:
                        break;
                    case 6: //delete account
                        SearchMenu(true); 
                        break;
                    case 7: //exit
                        auth = false;
                        break;
                }
            }
        }

    //HELPER FUNCTIONS
        static void Table(string title, string subtitle = "", string file = "", bool esc = false)
        {
            inputPos.Clear();
            Console.Clear();
            int inputX, inputY; //cursor pos for input fields
            string path = templatesPath + file;

            //header
            string border = new('═', width);
            Console.WriteLine($"╔{border}╗");
            Console.WriteLine($"║{CentreText(title)}║");
            if (esc)
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
            foreach (string line in File.ReadLines(path))
            {
                string text = line;
                if (!string.IsNullOrWhiteSpace(line) && line[line.Length - 1] == '|') //check if line is an input field
                {
                    text = line.Remove(line.Length - 1);
                    inputX = line.Length;
                    inputPos.Add((inputX, inputY));
                }
                Console.WriteLine($"║{text}{new String(' ', width - text.Length)}║");
                inputY++;
            }
            Console.WriteLine(lineBreak);
            Console.WriteLine($"╚{border}╝");
        }

        static string CentreText(string text) //centering text within tables
        {
            return String.Format("{0," + ((width / 2) + (text.Length / 2)) + "}" +
                                 "{1," + ((width / 2) - (text.Length / 2)) + "}",
                                 text, ""); //create padding before and after text
        }

        static void ClearAllFields(List<(int, int)> inputPos)
        {
            Console.CursorVisible = false; //hide cursor to prevent any flickering as it jumps to new positions
            foreach ((int, int) pos in inputPos)
            {
                Console.SetCursorPosition(pos.Item1, pos.Item2);
                Console.Write(new String(' ', width - pos.Item1));
            }
            Console.CursorVisible = true;
        }

        static void ClearField(int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(new String(' ', width - x)); 
            Console.SetCursorPosition(x, y);
        }

        static void WritePrompt((int, int) pos, string msg, bool newLine = true)
        {
            Console.CursorVisible = false; 
            Console.SetCursorPosition(0, pos.Item2 + 1); //clear any previous message
            Console.WriteLine(new String(' ', width));
            Console.SetCursorPosition(((width - msg.Length) / 2) + 1, pos.Item2 + 1); //centre of console window
            if (newLine)
                Console.WriteLine(msg);
            else
                Console.Write(msg);
            Console.CursorVisible = true;
        }

        static void ClearPrompts((int, int) pos)
        {
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, pos.Item2 + 1);
            Console.WriteLine(new String(' ', width));
            Console.WriteLine(new String(' ', width));
            Console.CursorVisible = true;
        }

        private static string ReadLineWithCancel() //returns null if esc pressed during input
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

        static string[] GetAccount(string id) //0: id, 1: name, 2: last, 3: addr, 4: phone, 5: email, 6: balance
        {
            try
            {
                return File.ReadAllLines(accountsPath + $"{id}.txt");
            } 
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        static void UpdateLine(string id, int index, string newText) //update an account .txt file
        {
            string path = accountsPath + $"{id}.txt";
            string[] lines = File.ReadAllLines(path);
            lines[index] = newText;
            File.WriteAllLines(path, lines);
        }

        static void SendEmail(int choice, string id)
        {
            string path = accountsPath + $"{id}.txt";
            string[] credentials = GetAccount(id);
            
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential("bamc31927@gmail.com", "vlrmjsvaanlzpfwj");
            smtp.EnableSsl = true;

            MailMessage mail = new MailMessage(new MailAddress("bamc31927@gmail.com"),
                                               new MailAddress(credentials[5]));
            if (choice == 1)
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
            if (choice == 2)
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
                    var key = Console.ReadKey(true);
                    StringBuilder input = new StringBuilder();
                    while (key.Key != ConsoleKey.Enter)
                    {
                        if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                        {
                            input.Remove(input.Length - 1, 1);
                            Console.Write("\b \b");
                        }
                        else if (key.Key != ConsoleKey.Backspace && key.KeyChar != '\u0000')
                        {
                            Console.Write("*");
                            input.Append(key.KeyChar);
                        }
                        key = Console.ReadKey(true);
                    }
                    credentials[i] = input.ToString();
                }
            }
            //username and password input complete, attempt to find match
            foreach (string line in File.ReadLines(@"..\..\..\Storage\Login.txt"))
            {
                string[] check = line.Split('|');
                if (check[0].Trim().ToLower().Equals(credentials[0].ToLower()) && check[1].Trim().Equals(credentials[1]))
                {
                    return; //match found, break out of function
                }
            }
            credentials = new string[2]; //otherwise reset menu
            ClearAllFields(inputPos);
            WritePrompt(errorMsgPos, "Incorrect username or password.");
            goto Start;
        }

        static int MainMenu()
        {
            Table("Main Menu", "Options", "MainMenu.txt");
            (int, int) errorMsgPos = Console.GetCursorPosition();
            string choice;

            while (true)
            {
                Console.SetCursorPosition(inputPos[0].Item1, inputPos[0].Item2);
                choice = Console.ReadLine();
                if (!int.TryParse(choice, out _) || 1 > Int32.Parse(choice) || Int32.Parse(choice) > 7)
                {
                    ClearAllFields(inputPos);
                    WritePrompt(errorMsgPos, "Invalid option number.");
                }
                else break;
            }
            return Int32.Parse(choice);
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
                input = ReadLineWithCancel();
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
            //confirmation loop
            WritePrompt(errorMsgPos, "Confirm details (Y/N)", false);
            while (true)
            {
                ConsoleKeyInfo choice = Console.ReadKey(true);
                if (choice.KeyChar.Equals('y'))
                {
                    int cap = 1000000;
                    string id = new Random().Next(0, cap).ToString("D6"); //D6 = allow leading zeros
                    var fileSet = new HashSet<string>(Directory
                                                     .GetFiles(accountsPath, "*", SearchOption.AllDirectories)
                                                     .Select(f => Path.GetFileName(f))); //array of filenames, turn into hashset for faster lookup
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
                        id = new Random().Next(0, cap).ToString("D6");
                    }
                    //create {id}.txt file
                    using (StreamWriter sw = File.CreateText($"{accountsPath}{id}.txt"))
                    {
                        sw.WriteLine(id); //line 0: acc id
                        foreach (string line in credentials) //1: name, 2: last, 3: addr, 4: phone, 5: email
                        {
                            sw.WriteLine(line);
                        }
                        sw.WriteLine(0); //6: starting balance
                    }
                    //send email
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
            ClearPrompts(errorMsgPos);
            ClearAllFields(inputPos);
            goto Start;
        }

        static void SearchMenu(bool delete = false)
        {
            string title = delete ? "Delete Account" : "Search Account";
            Start:
            Table(title, "Enter 6 digits", "SearchMenu.txt", true);
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
            if (delete) //if delete menu
            {
                WritePrompt(errorMsgPos, "Confirm deletion (Y/N)", false);
                while (true)
                {
                    ConsoleKeyInfo choice = Console.ReadKey(true);
                    if (choice.KeyChar.Equals('y'))
                    {
                        File.Delete(accountsPath + $"{input}.txt");
                        WritePrompt(errorMsgPos, "Account deleted.", false);
                        errorMsgPos = (errorMsgPos.Item1, errorMsgPos.Item2 + 1);
                        break;
                    }
                    if (choice.KeyChar.Equals('n')) goto Start;
                }
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
                input = ReadLineWithCancel();
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
                            WritePrompt(errorMsgPos, "Account found.", false);
                            WritePrompt((errorMsgPos.Item1, errorMsgPos.Item2 + 1), $"Current Balance: ${credentials[6]}");
                            break;
                        }
                    }
                    else //amount validation
                    {
                        //check if amount = positive double && if not depositing, check if withdrawal amount < balance.
                        if (double.TryParse(input, out double amount) && amount > 0 && (deposit || amount <= double.Parse(credentials[6])))
                        {
                            input = !deposit ? (amount *= -1).ToString() : input; //convert to negative if withdrawing
                            break;
                        }
                        else WritePrompt(errorMsgPos, "Invalid amount.");
                    }
                    ClearField(inputPos[i].Item1, inputPos[i].Item2);
                    input = ReadLineWithCancel();
                }
                if (input == null) return;
            }
            //updating balance
            string updated = Math.Round(double.Parse(credentials[6]) + double.Parse(input), 2).ToString("0.00");
            UpdateLine(id, 6, updated);
            WritePrompt(errorMsgPos, $"Updated balance: ${updated}", false);
            WritePrompt((errorMsgPos.Item1, errorMsgPos.Item2 + 1), "Press any key to continue.", false);

            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key.Equals(ConsoleKey.Escape)) return;
            
            ClearPrompts(errorMsgPos);
            ClearAllFields(inputPos);
            goto Start;
        }
    }
}
