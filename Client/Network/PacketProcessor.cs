using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using TCPClient;
using XProtocol;
using XProtocol.Serializator;

namespace Client.Network ;

    public class PacketProcessor
    {
        private static XClient? Client { get; set; }
        private static int _handshakeMagic;

        private static void OnPacketRecieve(byte[] packet)
        {
            var parsed = XPacket.Parse(packet);

            if (parsed != null)
            {
                ProcessIncomingPacket(parsed);
            }
        }
        
        private static void ProcessIncomingPacket(XPacket packet)
        {
            var type = XPacketTypeManager.GetTypeFromPacket(packet);

            switch (type)
            {
                case XPacketType.Unknown:
                    break;
                case XPacketType.NewPlayer:
                    ProcessPlayer(packet);
                    break;
                case XPacketType.Handshake:
                    ProcessHandshake(packet);
                    break;
                case XPacketType.PlayersInfo:
                    ProcessPlayersInfo(packet);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static async Task ConnectAndSendHandshakeAsync()
        {
            Client = new XClient();
            Client.OnPacketRecieve += OnPacketRecieve;
            await Task.Run(() => Client.Connect("127.0.0.1", 4910));
            var rand = new Random();
            _handshakeMagic = rand.Next();
            
            Console.WriteLine("Sending handshake packet..");

            Client.QueuePacketSend(
                XPacketConverter.Serialize(
                    XPacketType.Handshake,
                    new XPacketHandshake
                    {
                        MagicHandshakeNumber = _handshakeMagic
                    })
                    .ToPacket());
        }
        
        private static void ProcessPlayer(XPacket packet)
        {
            var player = XPacketConverter.Deserialize<XPacketPlayer>(packet);
            Console.WriteLine(player.Name);
            Console.WriteLine(player.Count);
        }
        
        // TODO: обработка на клиенте случая когда хэндшейк не прошел 
        private static void ProcessHandshake(XPacket packet) 
        {
            var handshake = XPacketConverter.Deserialize<XPacketHandshake>(packet);

            if (_handshakeMagic - handshake.MagicHandshakeNumber == 15)
            {
                Console.WriteLine("Handshake successful!");
            }
            else
            {
                Console.WriteLine("Handshake failed!");
            }
        }

        private static void ProcessPlayersInfo(XPacket packet)
        {
            var packetInfo = XPacketConverter.Deserialize<XPacketPlayersInfo>(packet);
            var players = JsonSerializer.Deserialize<List<string>>(packetInfo.InformationJson);
            foreach (var player in players!)
            {
                Console.WriteLine(player);
            }
        }

        public static void SendPlayersInfoRequest()
        {
            var pack = XPacket.Create(XPacketType.RequestPlayerInfo);
            Client.QueuePacketSend(pack.ToPacket());
        }
    }