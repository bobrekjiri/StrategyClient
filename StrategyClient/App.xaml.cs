using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Reflection;

namespace StrategyClient
{
    public partial class App : Application
    {
        internal Client Client { get; set; }
        internal LanguageCode LanguageCode { get; set; }
        internal Version Version { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Client = new Client();
            LanguageCode = LanguageCode.Czech; //TODO: load from configuration file
            Version = Assembly.GetExecutingAssembly().GetName().Version;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Client.Dispose();
        }
    }
}