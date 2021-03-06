﻿using System;
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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version;

            while (true)
            {
                Client = new Client();
                LoadConfig();

                LoginWindow loginWindow = new LoginWindow();
                bool? goAhead = loginWindow.ShowDialog();
                SaveConfig(Client.ServerAddress, Client.ServerPort, LanguageCode.Czech, Client.Login);
                if (goAhead != true)
                {
                    break;
                }
                MainWindow mainWindow = new MainWindow();
                mainWindow.ShowDialog();
                Client.Dispose();
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
            if (File.Exists("config.xml"))
            {
                XmlTextReader reader = new XmlTextReader("config.xml");
                try
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            switch (reader.Name)
                            {
                                case "Port":
                                    reader.Read();
                                    Client.ServerPort = int.Parse(reader.Value);
                                    break;
                                case "Address":
                                    reader.Read();
                                    Client.ServerAddress = IPAddress.Parse(reader.Value);
                                    break;
                                case "LanguageCode":
                                    reader.Read();
                                    LanguageCode = (LanguageCode)(short.Parse(reader.Value));
                                    break;
                                case "Login":
                                    reader.Read();
                                    Client.Login = reader.Value;
                                    break;
                            }
                        }
                    }
                    reader.Close();
                    Console.WriteLine("Configuration file loaded successfully.");
                }
                catch (XmlException ex)
                {
                    reader.Close();
                    Console.WriteLine("Error in loading configuration: " + ex.Message + " .");
                    Console.WriteLine("Repairing configuration file.");
                    SaveConfig(IPAddress.Parse("0"), 0, LanguageCode.Czech, string.Empty);
                }
                catch (Exception)
                {
                    reader.Close();
                    Console.WriteLine("Loading configuration failed!");
                    Console.WriteLine("Creating configuration file.");
                    SaveConfig(IPAddress.Parse("0"), 0, LanguageCode.Czech, string.Empty);
                }
            }
            else
            {
                Console.WriteLine("Configuration file not found!");
                Console.WriteLine("Creating configuration file.");
                SaveConfig(IPAddress.Parse("0"), 0, LanguageCode.Czech, string.Empty);
            }
        }

        private void SaveConfig(IPAddress ip, int port, LanguageCode language, string login)
        {
            using (XmlWriter writer = XmlWriter.Create("config.xml"))
            {
                writer.WriteStartDocument();
                writer.WriteWhitespace("\n");
                writer.WriteStartElement("Document");
                writer.WriteWhitespace("\n\t");
                writer.WriteComment("This file is automatically generated by Client application. Changes to this file may cause its incorrect behavior.");
                writer.WriteWhitespace("\n\t");
                writer.WriteStartElement("Header");
                writer.WriteAttributeString("Autor", "InfinityGames2012");
                writer.WriteAttributeString("Version", "1.0");
                writer.WriteEndElement();
                writer.WriteWhitespace("\n\t");
                writer.WriteStartElement("Data");

                createElement(writer, "\n\t\t", "Address", ip.ToString());
                createElement(writer, "\n\t\t", "Port", port.ToString());
                createElement(writer, "\n\t\t", "LanguageCode", ((short)language).ToString());
                createElement(writer, "\n\t\t", "Login", login);

                writer.WriteWhitespace("\n\t");
                writer.WriteEndElement();
                writer.WriteWhitespace("\n");
                writer.WriteEndElement();
            }
            Console.WriteLine("Configuration file saved successfully.");
        }

        private void createElement(XmlWriter writer, string whitespace, string name, string value)
        {
            writer.WriteWhitespace(whitespace);
            writer.WriteStartElement(name);
            writer.WriteString(value);
            writer.WriteEndElement();
        }
    }
}