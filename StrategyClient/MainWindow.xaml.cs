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

namespace StrategyClient
{
    public partial class MainWindow : Window
    {
        App app;
        Client client;
        public MainWindow()
        {
            InitializeComponent();
            app = (App)App.Current;
            client = app.Client;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string data = string.Empty;
            data += (short)app.LanguageCode + "~";
            data += app.Version.Major + "~";
            data += app.Version.Minor + "~";
            data += app.Version.Build + "~";
            data += app.Version.Revision + "~";
            client.Send(RequestType.Welcome, data);
        }
    }
}