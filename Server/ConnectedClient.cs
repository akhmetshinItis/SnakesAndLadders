using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using XProtocol;
using XProtocol.Serializator;

namespace TCPServer
{
    internal class ConnectedClient
    {
        public string Name { get; set; }
        public Server Server { get; set; }
        public Socket Client { get; }
        
        private readonly Queue<byte[]> _packetSendingQueue = new Queue<byte[]>();

        public ConnectedClient(Socket client, Server server)
        {
            Client = client;
            Server = server;
            Task.Run((Action) ProcessIncomingPackets);
            Task.Run((Action) SendPackets);
        }

        private void ProcessIncomingPackets()
        {
            while (true) // Слушаем пакеты, пока клиент не отключится.
            {
                var buff = new byte[256]; // Максимальный размер пакета - 256 байт.
                Client.Receive(buff);
                
                buff = buff.TakeWhile((b, i) =>
                {
                    if (b != 0xFF) return true;
                    return buff[i + 1] != 0;
                }).Concat(new byte[] {0xFF, 0}).ToArray();
                
                var parsed = XPacket.Parse(buff);

                if (parsed != null)
                {
                    ProcessIncomingPacket(parsed);
                }
            }
        }

        private void ProcessIncomingPacket(XPacket packet)
        {
            var type = XPacketTypeManager.GetTypeFromPacket(packet);

            switch (type)
            {
                case XPacketType.Handshake:
                    ProcessHandshake(packet);
                    break;
                case XPacketType.Unknown:
                    break;
                case XPacketType.NewPlayer:
                    ProcessNewPlayer(packet);
                    break;
                case XPacketType.RequestPlayerInfo:
                    ProcessRequestPlayerInfo(packet);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessHandshake(XPacket packet)
        {
            Console.WriteLine("Recieved handshake packet.");

            var handshake = XPacketConverter.Deserialize<XPacketHandshake>(packet);
            handshake.MagicHandshakeNumber -= 15;
            
            Console.WriteLine("Answering..");

            QueuePacketSend(XPacketConverter.Serialize(XPacketType.Handshake, handshake).ToPacket());
        }

        private void ProcessNewPlayer(XPacket packet)
        {
            Console.WriteLine("Recieved new player packet.");
            var player = XPacketConverter.Deserialize<XPacketPlayer>(packet);
            Console.WriteLine("Player name: " + player.Name);
            Console.WriteLine("Player Count " + player.Count);
            Name = player.Name;
            
            // QueuePacketSend(XPacketConverter.Serialize(XPacketType.NewPlayer, player).ToPacket());
            Server.SendToClients(this, XPacketConverter.Serialize(XPacketType.NewPlayer, player));
        }

        private async void ProcessRequestPlayerInfo(XPacket packet)
        {
            Console.WriteLine("Recieved request player info packet.");
            QueuePacketSend(XPacketConverter.Serialize(XPacketType.PlayersInfo, Server.GetClientsNames()).ToPacket()); 
        }

        public void QueuePacketSend(byte[] packet)
        {
            if (packet.Length > 256)
            {
                throw new Exception("Max packet size is 256 bytes.");
            }

            _packetSendingQueue.Enqueue(packet);
        }

        private void SendPackets()
        {
            while (true)
            {
                if (_packetSendingQueue.Count == 0)
                {
                    Thread.Sleep(100);
                    continue;
                }

                var packet = _packetSendingQueue.Dequeue();
                Client.Send(packet);

                Thread.Sleep(100);
            }
        }
    }
}
