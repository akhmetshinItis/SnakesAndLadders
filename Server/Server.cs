﻿using System.Net;
using System.Net.Sockets;
using XProtocol;
using XProtocol.Serializator;

namespace TCPServer
{
    internal class Server
    {
        private readonly Socket _socket;
        public List<ConnectedClient> _clients;

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
                if (_clients.Count == 1)
                {
                    c.IsFirst = true;
                }
            }
        }

        public async Task SendToClients(ConnectedClient? callingClient, XPacket packet, bool skipCaller = true)
        {
            foreach (var client in _clients)
            {
                if (callingClient != null && client == callingClient && skipCaller)
                    continue;
                
                await Task.Run(() => client.QueuePacketSend(packet.ToPacket()));
            }
        }

        public async Task ChangeTurn(ConnectedClient callingClient)
        {
            var index = _clients.IndexOf(callingClient);
            var client = _clients[(index + 1) % _clients.Count];
            var pack = XPacket.Create(XPacketType.ChangeTurn);
            await Task.Run(() => client.QueuePacketSend(pack.ToPacket()));
        }

        public async Task SendAllClientsToCaller(ConnectedClient callingClient, bool skipCaller = true)
        {
            foreach (var client in  _clients)
            {
                if (client == callingClient && skipCaller)
                    continue;
                var pack = XPacketConverter.Serialize(XPacketType.NewPlayer,
                    new XPacketPlayer
                    {
                        Name = client.Name,
                        Color = client.Color,
                    });
                callingClient.QueuePacketSend(pack.ToPacket());
                await Task.Delay(100);
            }
        }

        public List<string> GetClientsNames()
        {
            return _clients.Select(c => c.Name).ToList();
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

                        var pack = XPacketConverter.Serialize(XPacketType.DisconnectPlayer, new XPacketPlayer
                        {
                            Name = client.Name,
                            Color = client.Color,
                        });
                        await SendToClients(client, pack, false);
                        Storage.Names.Remove(client.Name);
                        Storage.AvalibleColors[client.Color] = client.Color;
                    }
                }

                await Task.Delay(5000);
            }
        }
        
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
