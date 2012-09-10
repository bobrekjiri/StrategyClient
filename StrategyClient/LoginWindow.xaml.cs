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
            string data = string.Empty;
            data += (short)app.LanguageCode + "~";
            data += app.Version.Major + "~";
            data += app.Version.Minor + "~";
            data += app.Version.Build + "~";
            data += app.Version.Revision + "~";
            client.Send(RequestType.Welcome, data);
        }

        void client_AnswerReceived(object sender, EventArgs e)
        {
            Console.WriteLine("AnswerReceived");
        }
    }
}