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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace StrategyClient
{
    public partial class MainWindow : Window
    {
        App app;
        Client client;
        Thread clientThread;
        UTF8Encoding utf8Encoder;
        Map map;
        int day;

        public MainWindow()
        {
            InitializeComponent();
            app = (App)App.Current;
            client = app.Client;
            initialize();
        }

        private void initialize()
        {
            map = new Map(MapGrid);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            client.AnswerReceived += new EventHandler(client_AnswerReceived);
            client.Request = string.Empty;
            send(RequestType.EnterGame);
        }

        private void send(RequestType type)
        {
            client.RequestType = type;

            clientThread = new Thread(new ThreadStart(client.Send));
            clientThread.IsBackground = true;
            clientThread.Start();
        }

        void client_AnswerReceived(object sender, EventArgs e)
        {
            Console.WriteLine("AnswerReceived: " + client.AnswerType);
            Dispatcher.Invoke(new Action(answerReceived));
        }

        void answerReceived()
        {
            switch (client.AnswerType)
            {
                case AnswerType.Map:
                    day = int.Parse(client.Answer[0]);
                    client.Answer.RemoveRange(0, 1);
                    map.SetMap(client.Answer);
                    break;
                case AnswerType.Village:
                    break;
            }
        }
    }
}