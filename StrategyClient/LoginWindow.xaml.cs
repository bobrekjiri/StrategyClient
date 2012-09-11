using System;
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

namespace StrategyClient
{
    public partial class LoginWindow : Window
    {
        App app;
        Client client;
        Thread clientThread;
        public LoginWindow()
        {
            InitializeComponent();
            app = (App)App.Current;
            client = app.Client;
            TitleLabel.Content = "Strategie";
            VersionLabel.Content = "ver. " + app.Version.ToString();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            client.Connected += new EventHandler(Client_Connected);
            client.AnswerReceived += new EventHandler(client_AnswerReceived);
            clientThread = new Thread(new ThreadStart(client.Connect));
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
                case AnswerType.UnknownRequestError:
                    break;
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
            }
        }

        void client_AnswerReceived(object sender, EventArgs e)
        {
            Console.WriteLine("AnswerReceived: " + client.AnswerType);
            Dispatcher.Invoke(new Action(answerReceived));
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            Update.IsEnabled = false;
            clientThread = new Thread(new ThreadStart(update));
            clientThread.Start();
        }

        private void update()
        {
            client.Update();
            Dispatcher.Invoke(new Action(executeUpdate));
        }

        private void executeUpdate()
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.WorkingDirectory = Directory.GetCurrentDirectory();
            info.FileName = "StrategyUpdater.exe";
            info.Arguments = "continue";
            Process process = Process.Start(info);
            app.Shutdown();
        }
    }
}