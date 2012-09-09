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
        private TcpClient client;
        NetworkStream clientStream;
        private AesManaged aes;
        private ICryptoTransform encryptor;
        private ICryptoTransform decryptor;
        public Client()
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
            aes = new AesManaged();
            encryptor = aes.CreateEncryptor();
            decryptor = aes.CreateDecryptor();
            buffer = new byte[48];

            Array.Copy(aes.IV, buffer, 16);
            Array.Copy(aes.Key, 0, buffer, 16, 32);
            buffer = rsa.Encrypt(buffer, true);
            clientStream.Write(buffer, 0, buffer.Length);
        }

        private byte[] Encrypt(string text)
        {
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(text);
                    }
                }
                return msEncrypt.ToArray();
            }
        }
        private string Decrypt(byte[] buffer)
        {
            using (MemoryStream msDecrypt = new MemoryStream(buffer))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        public void Send(RequestType type, string data)
        {
            string text = (short)type + "~";
            text += data;

            byte[] buffer = Encrypt(text);
            string text2 = Decrypt(buffer);
            clientStream.Write(buffer, 0, buffer.Length);

            buffer = new byte[4096];
            int length = clientStream.Read(buffer, 0, buffer.Length);
            byte[] buffer2 = new byte[length];
            string messsage = Decrypt(buffer2);
            short answerType = (short)messsage[0];
            //TODO: handle answer
        }

        public void Dispose()
        {
            client.Close();
        }
    }
}