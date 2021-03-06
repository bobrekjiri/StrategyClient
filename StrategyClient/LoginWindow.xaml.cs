﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Net;

namespace StrategyClient
{
    public partial class LoginWindow : Window
    {
        App app;
        Client client;
        Thread clientThread;
        UTF8Encoding utf8Encoder;
        EventHandler answerHandler, connectionHandler;

        ImageSource exit, exitMouseOver;

        public LoginWindow()
        {
            InitializeComponent();
            app = (App)App.Current;
            client = app.Client;
            ServerIP.Text = client.ServerAddress.ToString();
            ServerPort.Text = client.ServerPort.ToString();
            utf8Encoder = new UTF8Encoding();
            TitleLabel.Content = "Strategie";
            VersionLabel.Content = "verze: " + app.Version.ToString();
            exit = new BitmapImage(new Uri("/StrategyClient;component/Graphics/exit.png", UriKind.RelativeOrAbsolute));
            exitMouseOver = new BitmapImage(new Uri("/StrategyClient;component/Graphics/exit2.png", UriKind.RelativeOrAbsolute));
            SaveLogin.IsChecked = client.Login != string.Empty;
            if (SaveLogin.IsChecked == true)
            {
                LoginLogin.Text = client.Login;
            }
#if DEBUG
            LoginPassword.Password = "majzlik";
#endif
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            connectionHandler = new EventHandler(Client_Connected);
            answerHandler = new EventHandler(client_AnswerReceived);
            client.Connected += connectionHandler;
            client.AnswerReceived += answerHandler;
            clientThread = new Thread(new ThreadStart(client.Connect));
            clientThread.IsBackground = true;
            clientThread.Start();
        }

        void Client_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("Connected");
            Dispatcher.Invoke(new Action(connected));
            string data = (short)app.LanguageCode + "~";
            data += app.Version.Major + "~";
            data += app.Version.Minor + "~";
            data += app.Version.Build + "~";
            data += app.Version.Revision + "~";
            client.RequestType = RequestType.Welcome;
            client.Request = data;
            client.Send();
        }

        void connected()
        {
            ServerStatus.Content = "Načítání...";
        }

        void answerReceived()
        {
            switch (client.AnswerType)
            {
                case AnswerType.Welcome:
                    ServerStatus.Content = "Připojen k " + client.Answer[1];
                    WelcomeMessage.Text = client.Answer[2];
                    bool isVersionSupported = bool.Parse(client.Answer[0]);
                    if (isVersionSupported)
                    {
                        StandardLoginButtons.Height = 37;
                    }
                    else
                    {
                        UpdateLoginButtons.Height = 37;
                    }
                    break;

                case AnswerType.Registration:
                    int errorCode = int.Parse(client.Answer[0]);
                    RegistrationRegisterButton.IsEnabled = errorCode != 0;
                    switch (errorCode)
                    {
                        case 0:
                            RegistrationStatus.Content = "Registrace úspěšná! Nyní vyčkejte než bude potvrzena";
                            RegistrationStatus.Foreground = Brushes.Green;
                            break;
                        case 1:
                            RegistrationStatus.Content = "Tento login má v plánu použít jiný hráč";
                            RegistrationStatus.Foreground = (Brush)Resources["blue"];
                            break;
                        case 2:
                            RegistrationStatus.Content = "Toto jméno má v plánu použít jiný hráč";
                            RegistrationStatus.Foreground = (Brush)Resources["blue"];
                            break;
                        case 3:
                            RegistrationStatus.Content = "Z této IP adresy se v současné době nemůžete zaregistrovat";
                            RegistrationStatus.Foreground = Brushes.Orange;
                            break;
                        case 4:
                            RegistrationStatus.Content = "Tento login je již obsazený";
                            RegistrationStatus.Foreground = Brushes.Purple;
                            break;
                        case 5:
                            RegistrationStatus.Content = "Toto jméno je již obsazené";
                            RegistrationStatus.Foreground = Brushes.Purple;
                            break;
                        case 6:
                            RegistrationStatus.Content = "Bohužel, fronta nevyřízených registrací je plná";
                            RegistrationStatus.Foreground = Brushes.Orange;
                            break;
                    }
                    break;

                case AnswerType.Login:
                    errorCode = int.Parse(client.Answer[0]);
                    LoginLoginButton.IsEnabled = errorCode != 0;
                    EnterGame.IsEnabled = errorCode == 0;
                    Settings.IsEnabled = errorCode == 0;
                    Settings.IsExpanded = errorCode == 0;
                    switch (errorCode)
                    {
                        case 0:
                            LoginStatus.Content = "Přihlášení úspěšné";
                            LoginStatus.Foreground = Brushes.Green;
                            break;
                        case 1:
                            LoginStatus.Content = "Špatný login nebo heslo";
                            LoginStatus.Foreground = Brushes.Purple;
                            break;
                    }
                    break;

                case AnswerType.ChangeLogin:
                    errorCode = int.Parse(client.Answer[0]);
                    switch (errorCode)
                    {
                        case 0:
                            LoginStatus.Content = "Změna úspěšná";
                            LoginStatus.Foreground = Brushes.Green;
                            break;
                        case 1:
                            LoginStatus.Content = "Tento login má v plánu použít jiný hráč";
                            LoginStatus.Foreground = (Brush)Resources["blue"];
                            break;
                        case 2:
                            LoginStatus.Content = "Tento login je již obsazený";
                            LoginStatus.Foreground = Brushes.Purple;
                            break;
                    }
                    break;
                case AnswerType.ChangeName:
                    errorCode = int.Parse(client.Answer[0]);
                    switch (errorCode)
                    {
                        case 0:
                            LoginStatus.Content = "Změna úspěšná";
                            LoginStatus.Foreground = Brushes.Green;
                            break;
                        case 1:
                            LoginStatus.Content = "Toto jméno má v plánu použít jiný hráč";
                            LoginStatus.Foreground = (Brush)Resources["blue"];
                            break;
                        case 2:
                            LoginStatus.Content = "Toto jméno je již obsazené";
                            LoginStatus.Foreground = Brushes.Purple;
                            break;
                    }
                    break;
                case AnswerType.ChangePassword:
                    LoginStatus.Content = "Změna úspěšná";
                    LoginStatus.Foreground = Brushes.Green;
                    break;
            }
        }

        void client_AnswerReceived(object sender, EventArgs e)
        {
            Console.WriteLine("AnswerReceived: " + client.AnswerType);
            Dispatcher.Invoke(new Action(answerReceived));
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            UpdateAvailable.Content = "Stahování...";
            Update.IsEnabled = false;
            clientThread = new Thread(new ThreadStart(update));
            clientThread.IsBackground = true;
            clientThread.Start();
        }

        private void send(RequestType type)
        {
            client.RequestType = type;

            clientThread = new Thread(new ThreadStart(client.Send));
            clientThread.IsBackground = true;
            clientThread.Start();
        }

        private void update()
        {
            client.Update();
            Dispatcher.Invoke(new Action(executeUpdate));
        }

        private void executeUpdate()
        {
            UpdateAvailable.Content = "Aktualizování...";
            ProcessStartInfo info = new ProcessStartInfo();
            info.WorkingDirectory = Directory.GetCurrentDirectory();
            info.FileName = "StrategyUpdater.exe";
            info.Arguments = "continue";
            Process process = Process.Start(info);
            app.Shutdown();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            WelcomeScreen.Visibility = Visibility.Hidden;
            RegistrationScreen.Visibility = Visibility.Visible;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            WelcomeScreen.Visibility = Visibility.Hidden;
            LoginScreen.Visibility = Visibility.Visible;
        }

        private void ExitImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RegistrationScreen.Visibility = Visibility.Hidden;
            WelcomeScreen.Visibility = Visibility.Visible;
        }

        private void ExitImage_MouseEnter(object sender, MouseEventArgs e)
        {
            Image image = sender as Image;
            image.Source = exitMouseOver;
        }

        private void ExitImage_MouseLeave(object sender, MouseEventArgs e)
        {
            Image image = sender as Image;
            image.Source = exit;
        }

        private void RegistrationRegister_Click(object sender, RoutedEventArgs e)
        {
            if (RegistrationLogin.Text.Length == 0 || RegistrationName.Text.Length == 0 || RegistrationPassword.Password.Length == 0 || RegistrationPasswordConfirm.Password.Length == 0 || RegistrationDescription.Text.Length == 0)
            {
                RegistrationStatus.Content = "Musíte vyplnit všechna pole";
                RegistrationStatus.Foreground = Brushes.Red;
                return;
            }
            if (RegistrationLogin.Text.Contains('~') || RegistrationName.Text.Contains('~') || RegistrationDescription.Text.Contains('~'))
            {
                RegistrationStatus.Content = "Login, jméno nebo popis obsahují nepovolené znaky";
                RegistrationStatus.Foreground = Brushes.Red;
                return;
            }
            if (RegistrationPassword.Password != RegistrationPasswordConfirm.Password)
            {
                RegistrationStatus.Content = "Hesla se neshodují";
                RegistrationStatus.Foreground = Brushes.Red;
                return;
            }
            RegistrationStatus.Content = "";
            RegistrationRegisterButton.IsEnabled = false;

            byte[] buffer = utf8Encoder.GetBytes(RegistrationPassword.Password);
            SHA256 sha256 = new SHA256Managed();
            client.PasswordBuffer = sha256.ComputeHash(buffer);

            client.Request = string.Format("{0}~{1}~{2}~", RegistrationLogin.Text, RegistrationName.Text, RegistrationDescription.Text);
            send(RequestType.Registration);
        }

        private void Settings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ConnectionSettings.Visibility == Visibility.Hidden)
            {
                ConnectionSettings.Visibility = Visibility.Visible;
            }
            else
            {
                ConnectionSettings.Visibility = Visibility.Hidden;
                IPAddress ip;
                int port;
                if (IPAddress.TryParse(ServerIP.Text, out ip) && int.TryParse(ServerPort.Text, out port))
                {
                    client.ServerAddress = ip;
                    client.ServerPort = port;
                }
            }
        }

        private void LoginLogin_Click(object sender, RoutedEventArgs e)
        {
            if (LoginLogin.Text.Length == 0 || LoginPassword.Password.Length == 0)
            {
                LoginStatus.Content = "Musíte vyplnit všechna pole";
                LoginStatus.Foreground = Brushes.Red;
                return;
            }
            if (LoginLogin.Text.Contains('~'))
            {
                LoginStatus.Content = "Login obsahuje nepovolené znaky";
                LoginStatus.Foreground = Brushes.Red;
                return;
            }
            LoginLoginButton.IsEnabled = false;

            byte[] buffer = utf8Encoder.GetBytes(LoginPassword.Password);
            SHA256 sha256 = new SHA256Managed();
            client.PasswordBuffer = sha256.ComputeHash(buffer);

            client.Request = string.Format("{0}~", LoginLogin.Text);
            send(RequestType.Login);
        }

        private void SettingsLoginChange_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsLogin.Text.Length == 0)
            {
                LoginStatus.Content = "Musíte vyplnit pole";
                LoginStatus.Foreground = Brushes.Red;
                return;
            }
            client.Request = string.Format("{0}~", SettingsLogin.Text);
            send(RequestType.ChangeLogin);
        }

        private void SettingsNameChange_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsName.Text.Length == 0)
            {
                LoginStatus.Content = "Musíte vyplnit pole";
                LoginStatus.Foreground = Brushes.Red;
                return;
            }
            client.Request = string.Format("{0}~", SettingsName.Text);
            send(RequestType.ChangeName);
        }

        private void SettingsPasswordChange_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsPassword.Password.Length == 0 || SettingsConfirmPassword.Password.Length == 0)
            {
                LoginStatus.Content = "Musíte vyplnit obě pole";
                LoginStatus.Foreground = Brushes.Red;
                return;
            }
            if (SettingsPassword.Password != SettingsConfirmPassword.Password)
            {
                LoginStatus.Content = "Hesla se neshodují";
                LoginStatus.Foreground = Brushes.Red;
                return;
            }

            byte[] buffer = utf8Encoder.GetBytes(SettingsPassword.Password);
            SHA256 sha256 = new SHA256Managed();
            client.PasswordBuffer = sha256.ComputeHash(buffer);

            client.Request = string.Empty;
            send(RequestType.ChangePassword);
        }

        private void EnterGame_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            client.Login = (SaveLogin.IsChecked == true) ? LoginLogin.Text : string.Empty;
            client.Connected -= connectionHandler;
            client.AnswerReceived -= answerHandler;
        }
    }
}