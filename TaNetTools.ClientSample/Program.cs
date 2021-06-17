using System;
using System.Text;
using TaNetTools.Tcp;

namespace TaNetTools.ClientSample
{
    internal static class Program
    {
        static void Main()
        {   
            TcpClient client = new TcpClient("127.0.0.1", 40000);
            client.Started += Client_Started;
            client.Stoped += Client_Stoped;
            client.DataReceived += Client_DataReceived;
            client.DataSent += Client_DataSent; ;

            client.Start();

            while (true)
            {
                var input = Console.ReadLine();
                if (input == "q")
                    break;

                client.SendData(input + "\r\n");
            }

            client.Stop();
            Console.ReadLine();
        }

        private static void Client_DataSent(string ip, int port, string data)
        {
            Console.WriteLine($"[{data.Trim()}] message sent to {ip}:{port}");
        }

        private static void Client_DataReceived(byte[] data)
        {
            Console.WriteLine($"New data received : {Encoding.UTF8.GetString(data)}");
        }

        private static void Client_Stoped(string ip, int port)
        {
            Console.WriteLine($"Disconnected from server {ip}:{port}");
        }

        private static void Client_Started(string connectedIp, int connetedPort)
        {
            Console.WriteLine($"Connected to server {connectedIp}:{connetedPort}");
        }
    }
}
