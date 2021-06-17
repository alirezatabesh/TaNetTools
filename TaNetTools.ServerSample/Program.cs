using System;
using System.Collections.Generic;
using System.Text;
using TaNetTools.Tcp;

namespace TaNetTools.ServerSample
{
    internal static class Program
    {
        static void Main()
        {
            TcpServer server = new TcpServer(new byte[] { 13, 10 });
            server.EngineStarted += Server_EngineStarted;
            server.EngineStoped += Server_EngineStoped;
            server.NewClientConnected += Server_NewClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;
            server.NewDataReceived += Server_NewDataReceived;

            server.Start("127.0.0.1", 40000);

            Console.WriteLine("Press any key top stop TCP server.");
            Console.ReadLine();
            server.Stop();

            Console.WriteLine("Press any key to exit from console.");
            Console.ReadLine();
        }

        private static void Server_NewDataReceived(string clientIp, int clientPort, List<byte> receivedBytes)
        {
            Console.WriteLine($"New data received from {clientIp}:{clientPort}, data: {Encoding.UTF8.GetString(receivedBytes.ToArray())}");
        }

        private static void Server_ClientDisconnected(string clientIp, int clientPort)
        {
            Console.WriteLine($"Client disconnected. {clientIp}:{clientPort}");
        }

        private static void Server_NewClientConnected(string clientIp, int clientPort)
        {
            Console.WriteLine($"New client connected. {clientIp}:{clientPort}");
        }

        private static void Server_EngineStoped()
        {
            Console.WriteLine($"TCP Server stopped");
        }

        private static void Server_EngineStarted(string serverIp, int serverPort)
        {
            Console.WriteLine($"TCP Server started on {serverIp}:{serverPort}");
        }
    }
}
