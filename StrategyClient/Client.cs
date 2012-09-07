using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace StrategyClient
{
    class Client : IDisposable
    {
        private TcpClient client;
        private AesManaged aes;
        public Client()
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.100"), 9050); //TODO: load both values from configuration file
            client = new TcpClient();
            client.Connect(serverEndPoint);

            NetworkStream clientStream = client.GetStream();

            UTF8Encoding encoder = new UTF8Encoding();
            byte[] buffer = new byte[4096];

            int length = clientStream.Read(buffer, 0, buffer.Length);
            string key = encoder.GetString(buffer, 0, length);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(key);
            aes = new AesManaged();
            buffer = new byte[48];
            Array.Copy(aes.IV, buffer, 16);
            Array.Copy(aes.Key, 0, buffer, 16, 32);
            buffer = rsa.Encrypt(buffer, true);
            clientStream.Write(buffer, 0, buffer.Length);
        }

        private byte[] Encrypt(string text)
        {
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            return null;
        }

        public void Dispose()
        {
            client.Close();
        }
    }
}