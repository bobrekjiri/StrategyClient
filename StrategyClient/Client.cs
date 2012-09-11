﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.IO;

namespace StrategyClient
{
    class Client : IDisposable
    {
        public event EventHandler Connected;
        public event EventHandler AnswerReceived;

        public AnswerType AnswerType { get; private set; }
        public List<string> Answer { get; private set; }
        public RequestType RequestType { get; set; }
        public string Request { get; set; }

        private TcpClient client;
        private NetworkStream clientStream;
        private Encryptor encryptor;

        public Client()
        {
        }

        public void Connect()
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("213.220.250.46"), 9020); //TODO: load both values from configuration file
            while (true)
            {
                try
                {
                    client = new TcpClient();
                    client.Connect(serverEndPoint);
                    clientStream = client.GetStream();
                    break;
                }
                catch
                {
                }
            }

            UTF8Encoding encoder = new UTF8Encoding();
            byte[] buffer = new byte[4096];

            int length = clientStream.Read(buffer, 0, buffer.Length);
            string key = encoder.GetString(buffer, 0, length);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(key);
            encryptor = new Encryptor();
            buffer = encryptor.ExportKey();
            buffer = rsa.Encrypt(buffer, true);
            clientStream.Write(buffer, 0, buffer.Length);

            OnConnected(new EventArgs());
        }

        public void Send()
        {
            string text = (short)RequestType + "~";
            text += Request;

            byte[] buffer = encryptor.Encrypt(text);
            clientStream.Write(buffer, 0, buffer.Length);

            buffer = new byte[4096];
            int length = clientStream.Read(buffer, 0, buffer.Length);
            byte[] buffer2 = new byte[length];
            Array.Copy(buffer, buffer2, length);
            string message = encryptor.Decrypt(buffer2);

            List<string> parameters = getParameters(message);
            AnswerType = (AnswerType)short.Parse(parameters[0]);
            parameters.RemoveAt(0);
            Answer = parameters;

            OnAnswerReceived(new EventArgs());
        }

        public void Update()
        {
            string text = (short)RequestType.Update + "~";
            byte[] buffer = encryptor.Encrypt(text);
            clientStream.Write(buffer, 0, buffer.Length);

            buffer = new byte[1024];
            int length = clientStream.Read(buffer, 0, buffer.Length);
            byte[] buffer2 = new byte[length];
            Array.Copy(buffer, buffer2, length);
            string message = encryptor.Decrypt(buffer2);
            List<string> parameters = getParameters(message);
            int size = int.Parse(parameters[1]);

            byte[] fileBuffer = new byte[size];
            clientStream.Read(fileBuffer, 0, size);

            File.WriteAllBytes("NewStrategyClient.exe", fileBuffer);
        }

        public void Dispose()
        {
            client.Close();
        }

        protected virtual void OnConnected(EventArgs e)
        {
            if (Connected != null)
            {
                Connected(this, e);
            }
        }
        protected virtual void OnAnswerReceived(EventArgs e)
        {
            if (AnswerReceived != null)
            {
                AnswerReceived(this, e);
            }
        }

        private List<string> getParameters(string message)
        {
            int lastIndex = 0;
            List<string> arguments = new List<string>();
            for (int i = 0; i < message.Length; i++)
            {
                if (message[i] == '~')
                {
                    arguments.Add(message.Substring(lastIndex, i - lastIndex));
                    lastIndex = i + 1;
                }
            }
            return arguments;
        }
    }
}