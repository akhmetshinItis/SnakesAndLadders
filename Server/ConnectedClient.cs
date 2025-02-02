﻿using System.Net.Sockets;
using System.Text.Json;
using XProtocol;
using XProtocol.Serializator;

namespace TCPServer
{
    internal class ConnectedClient
    {
        public string Name { get; set; }
        public bool IsFirst { get; set; } = false;
        public int Color { get; set; }
        public int Position { get; set; }
        public Server Server { get; set; }
        public Socket Client { get; }
        public bool IsConnected => Client != null && Client.Connected;
        
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
            try
            {
                while (IsConnected) // Проверяем, подключен ли клиент
                {
                    var buff = new byte[256]; // Выделяем память под буфер
                    int received = Client.Receive(buff); // Получаем данные

                    if (received == 0) // Если получили 0 байт, значит соединение закрыто
                    {
                        Console.WriteLine("Client disconnected.");
                        break;
                    }

                    // Обрезаем лишние данные
                    buff = buff.Take(received)
                        .TakeWhile((b, i) => i < received - 1 && !(b == 0xFF && buff[i + 1] == 0))
                        .Concat(new byte[] { 0xFF, 0 })
                        .ToArray();

                    var parsed = XPacket.Parse(buff);

                    if (parsed != null)
                    {
                        ProcessIncomingPacket(parsed);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in packet processing: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Stopping packet processing...");
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
                case XPacketType.RollDice:
                    ProcessRollDice(packet);
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

        private async Task ProcessNewPlayer(XPacket packet)
        {
            Console.WriteLine("Recieved new player packet.");
            var player = XPacketConverter.Deserialize<XPacketPlayer>(packet);
            Console.WriteLine("Player Name: " + player.Name);
            Console.WriteLine("Player Color " + player.Color);
            if (Storage.AvalibleColors[player.Color] == 0 || Storage.Names.Contains(player.Name))
            {
                var pack = XPacket.Create(XPacketType.PlayerInfoNotAvailable);
                QueuePacketSend(pack.ToPacket());
                Server._clients.RemoveAt(Server._clients.Count - 1);
                return;
            }
            if (IsFirst)
            {
                var pack = XPacket.Create(XPacketType.ChangeTurn);
                QueuePacketSend(pack.ToPacket());
            }
            Storage.AvalibleColors[player.Color] = 0;
            Storage.Names.Add(player.Name);
            Name = player.Name;
            Color = player.Color;
            await Task.Delay(50);
            // QueuePacketSend(XPacketConverter.Serialize(XPacketType.NewPlayer, player).ToPacket());
            Server.SendToClients(this, XPacketConverter.Serialize(XPacketType.NewPlayer, player));
        }

        private async Task ProcessRequestPlayerInfo(XPacket packet)
        {
            Console.WriteLine("Recieved request player info packet.");
            await Server.SendAllClientsToCaller(this, false);
        }

        private async Task ProcessRollDice(XPacket packet)
        {
            Console.WriteLine("Recieved roll dice packet.");
            await Server.SendToClients(this, packet, true);
            await Task.Delay(100);
            await Server.ChangeTurn(this);
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