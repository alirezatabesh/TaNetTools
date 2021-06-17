using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TaNetTools.Tcp
{
    public class TcpClient
    {
        #region events
        public delegate void DataReceivedEvent(byte[] data);
        public event DataReceivedEvent DataReceived;
        public delegate void StartedEvent(string connectedIp, int connetedPort);
        public event StartedEvent Started;
        public delegate void StopedEvent(string ip, int port);
        public event StopedEvent Stoped;
        public delegate void DataSentEvent(string ip, int port, string data);
        public event DataSentEvent DataSent;
        #endregion

        private bool _terminated;
        private Socket _socket;
        private readonly string _serverIpAddress;
        private readonly int _serverPort;

        public TcpClient(string serverIpAddress, int serverPort)
        {
            _serverIpAddress = serverIpAddress;
            _serverPort = serverPort;
        }

        public void Start()
        {
            _terminated = false;
            ConnectToServer(_serverIpAddress, _serverPort);
            var process = new Thread(ThreadProcess);
            process.Start();
        }

        public void Stop()
        {
            _terminated = true;
            _socket.Disconnect(true);
            _socket.Close();
            Stoped?.Invoke(_serverIpAddress, _serverPort);
        }

        public void SendData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            if (!_socket.Connected)
                return;

            var bytes = Encoding.UTF8.GetBytes(data);

            var sentDataCount = _socket.Send(bytes, data.Length, SocketFlags.None);
            if (sentDataCount == data.Length)
                DataSent?.Invoke(_serverIpAddress, _serverPort, data);
        }

        private void ConnectToServer(string serverIpAddress, int serverPort)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var myIpAddress = IPAddress.Parse(serverIpAddress);
            var myEndpoint = new IPEndPoint(myIpAddress, serverPort);
            _socket.Connect(myEndpoint);
            Started?.Invoke(myEndpoint.Address.ToString(), myEndpoint.Port);
        }

        private void ThreadProcess()
        {
            while (!_terminated && _socket.Connected)
            {
                var dataCount = _socket.Available;
                if (dataCount == 0)
                    continue;

                var data = new byte[dataCount];
                _socket.Receive(data);
                DataReceived?.Invoke(data);

                if (_socket.Poll(1, SelectMode.SelectRead))
                    break;
            }
        }
    }
}
