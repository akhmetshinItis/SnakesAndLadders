using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using XProtocol;
using XProtocol.Serializator;

namespace TCPServer
{
    internal class Server
    {
        private readonly Socket _socket;
        private readonly List<ConnectedClient> _clients;

        private bool _listening;
        private bool _stopListening;
        
        public Server()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clients = new List<ConnectedClient>();
        }

        public void Start()
        {
            if (_listening)
            {
                throw new Exception("Server is already listening incoming requests.");
            }

            _socket.Bind(new IPEndPoint(IPAddress.Any, 4910));
            _socket.Listen(10);

            _listening = true;
        }

        public void Stop()
        {
            if (!_listening)
            {
                throw new Exception("Server is already not listening incoming requests.");
            }

            _stopListening = true;
            _socket.Shutdown(SocketShutdown.Both);
            _listening = false;
        }

        public void AcceptClients()
        {
            while (true)
            {
                if (_stopListening)
                {
                    return;
                }

                Socket client;

                try
                {
                    client = _socket.Accept();
                } catch { return; }

                Console.WriteLine($"[!] Accepted client from {(IPEndPoint) client.RemoteEndPoint}");

                var c = new ConnectedClient(client, this);
                _clients.Add(c);
            }
        }

        public void SendToClients()
        {
            foreach (var client in _clients)
            {
                var pack = XPacketConverter.Serialize(
                    XPacketType.NewPlayer,
                    new XPacketPlayer
                    {
                        Count = 0,
                        Name = GetClientsNames(),
                    }).ToPacket();
                client.QueuePacketSend(pack);
            }
        }

        private string GetClientsNames()
        {
            return string.Join(" ", _clients.Select(x => x.Name).ToArray());
        }

        public async Task CheckClients()
        {
            while (_listening)
            {
                for (int i = _clients.Count - 1; i >= 0; i--)
                {
                    var client = _clients[i];

                    if (!IsClientConnected(client.Client))
                    {
                        Console.WriteLine($"[-] Клиент {client.Name} отключен.");
                        _clients.RemoveAt(i);
                    }
                }

                await Task.Delay(5000);
            }
        }
        
        // переделать надо на отправку пакета какого нибудь хотя хз
        private bool IsClientConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException)
            {
                return false;
            }
        }

        
    }
}
