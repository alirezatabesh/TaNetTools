using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TaNetTools.Tcp
{
    public class TcpServer
    {
        #region events
        public delegate void EngineStartedEvent(string serverIp, int serverPort);
        public event EngineStartedEvent EngineStarted;
        public delegate void EnginStopedEvent();
        public event EnginStopedEvent EngineStoped;
        public delegate void NewClientConnectedEvent(string clientIp, int clientPort);
        public event NewClientConnectedEvent NewClientConnected;
        public delegate void ClientDisconnectedEvent(string clientIp, int clientPort);
        public event ClientDisconnectedEvent ClientDisconnected;
        public delegate void NewDataReceivedEvent(string clientIp, int clientPort, List<byte> receivedBytes);
        public event NewDataReceivedEvent NewDataReceived;
        #endregion

        private TcpListener _listener;
        private bool _terminated;
        private bool _clientTerminated;
        private readonly byte[] _delimiter;
        private readonly List<Socket> _clients = new List<Socket>();

        /// <summary>
        /// class constractor
        /// </summary>
        /// <param name="delimiter">when received this delimiter send NewDataReceived event</param>
        public TcpServer(byte[] delimiter)
        {
            _delimiter = delimiter;
        }

        public void Start(string serverIpAddress, int serverPort)
        {
            _terminated = false;
            var ip = IPAddress.Parse(serverIpAddress);
            _listener = new TcpListener(ip, serverPort);
            _listener.Start();
            var listenerThread = new Thread(TrdAcceptSocket);
            listenerThread.Start();
            EngineStarted?.Invoke(serverIpAddress, serverPort);
        }

        public void Stop()
        {
            _terminated = true;
            _clientTerminated = true;
            _listener.Stop();
            EngineStoped?.Invoke();
        }

        public void SendMessage(string message, string ip, int port)
        {
            IPAddress address = IPAddress.Parse(ip);
            var socket = _clients.FirstOrDefault(x => Equals(((IPEndPoint)x.RemoteEndPoint).Address, address) && ((IPEndPoint)x.RemoteEndPoint).Port == port);
            if (socket == null)
                return;
            byte[] bytes = new byte[message.Length];
            for (int i = 0; i < message.Length; i++)
                bytes[i] = (byte)message[i];
            socket.Send(bytes, message.Length, SocketFlags.None);
        }

        private void TrdAcceptSocket()
        {
            while (!_terminated)
            {
                try
                {
                    var tcpSocket = _listener.AcceptSocket();
                    _clientTerminated = false;
                    var clientThread = new Thread(ClientThreadProcess);
                    clientThread.Start(tcpSocket);
                }
                catch
                {
                    break;
                }
            }
        }

        private void ClientThreadProcess (object clientSocket)
        {
            var socket = (Socket)clientSocket;
            var endPoint = (IPEndPoint)socket.RemoteEndPoint;
            _clients.Add(socket);
            NewClientConnected?.Invoke(endPoint.Address.ToString(), endPoint.Port);

            int isDelimiter = 0;
            List<byte> clientReceivedBytes = new List<byte>();

            while (!_clientTerminated)
            {
                if (socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0)
                    break;

                var receivedDataSize = socket.Available;
                if (receivedDataSize == 0)
                    continue;

                var receiveBuf = new byte[receivedDataSize];
                int receiveBytes = socket.Receive(receiveBuf);
                if (receiveBytes == 0)
                    continue;

                clientReceivedBytes.AddRange(receiveBuf);

                if (clientReceivedBytes.Count < _delimiter.Length)
                    continue;

                for (int i = 0; i < _delimiter.Length; i++)
                {
                    var x = _delimiter[i];
                    var y = clientReceivedBytes[clientReceivedBytes.Count - _delimiter.Length + i];
                    if (x == y)
                        isDelimiter++;
                    else
                        break;
                }

                if (isDelimiter != _delimiter.Length)
                    continue;

                for (int i = 0; i < _delimiter.Length; i++)
                    clientReceivedBytes.RemoveAt(clientReceivedBytes.Count - 1);

                NewDataReceived?.Invoke(endPoint.Address.ToString(), endPoint.Port, clientReceivedBytes);
                isDelimiter = 0;
                clientReceivedBytes.Clear();
            }

            if (socket.Connected)
                socket.Disconnect(false);
            ClientDisconnected?.Invoke(endPoint.Address.ToString(), endPoint.Port);
            _clients.Remove(socket);
        }
    }
}