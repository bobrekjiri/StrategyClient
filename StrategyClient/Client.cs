using System;
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

        private TcpClient client;
        private NetworkStream clientStream;
        private Encryptor encryptor;

        public Client()
        {
        }

        public void Connect()
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("213.220.250.46"), 9020); //TODO: load both values from configuration file
            client = new TcpClient();
            client.Connect(serverEndPoint);
            clientStream = client.GetStream();

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

        public void Send(RequestType type, string data)
        {
            string text = (short)type + "~";
            text += data;

            byte[] buffer = encryptor.Encrypt(text);
            clientStream.Write(buffer, 0, buffer.Length);

            buffer = new byte[4096];
            int length = clientStream.Read(buffer, 0, buffer.Length);
            byte[] buffer2 = new byte[length];
            Array.Copy(buffer, buffer2, length);
            string messsage = encryptor.Decrypt(buffer2);

            //TODO: handle answer

            OnAnswerReceived(new EventArgs());
        }

        public void Dispose()
        {
            client.Close();
        }
    }
}