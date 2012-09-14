using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Xml;
using System.Net;

namespace StrategyClient
{
    public partial class App : Application
    {
        internal Client Client { get; set; }
        internal LanguageCode LanguageCode { get; set; }
        internal Version Version { get; set; }

        private IPAddress serverAddress = IPAddress.Parse("0");
        private int serverPort = 0;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            LoadConfig();
            Version = Assembly.GetExecutingAssembly().GetName().Version;

            while (true)
            {
                Client = new Client();
                Client.ServerAddress = serverAddress;
                Client.ServerPort = serverPort;

                LoginWindow loginWindow = new LoginWindow();
                bool? goAhead = loginWindow.ShowDialog();
                if (goAhead != true)
                {
                    break;

                }
            }
            Shutdown();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Client.Dispose();
        }

        private void LoadConfig()
        {
            Console.WriteLine("Loading configuration...");
            try
            {
                XmlTextReader reader = new XmlTextReader("config.xml");
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "Port":
                                reader.Read();
                                serverPort = int.Parse(reader.Value);
                                break;
                            case "Address":
                                reader.Read();
                                serverAddress = IPAddress.Parse(reader.Value);
                                continue;
                            case "LanguageCode":
                                reader.Read();
                                LanguageCode = (LanguageCode)(short.Parse(reader.Value));
                                continue;

                        }
                    }
                }
                reader.Close();
                Console.WriteLine("Configuration file loaded successfully.");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Configuration file not found!"); //TODO: Create default file
            }
            catch (XmlException ex)
            {
                Console.WriteLine("Error in loading configuration: " + ex.Message + " ."); //TODO: Create default file
            }
            catch (Exception)
            {
                Console.WriteLine("Loading configuration failed!"); //TODO: Create default file
            }
        }

        private void SaveConfig() //TODO: Save configuration file with actual settings
        {
        }
    }
}