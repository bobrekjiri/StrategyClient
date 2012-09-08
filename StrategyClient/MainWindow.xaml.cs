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
        private Client client;
        private Random random;

        public MainWindow()
        {
            InitializeComponent();
            random = new Random();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            client = new Client();
            byte[] buffer = new byte[46];
            random.NextBytes(buffer);
            string hash = new UTF8Encoding().GetString(buffer);
            client.Send(RequestType.Welcome, string.Empty);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            client.Dispose();
        }
    }
}
