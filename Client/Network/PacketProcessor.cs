using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using Client.Enums;
using Client.Views;
using XProtocol;
using XProtocol.Serializator;

namespace Client.Network ;

    public class PacketProcessor
    {
        public static MainWindow? MainWindow { get; set; }
        public static CustomMessageBox? CustomMessageBox { get; set; }
        private static TaskCompletionSource<bool> _handshakeCompletionSource;
        internal static XClient? Client { get; set; }

        private static void OnPacketRecieve(byte[] packet)
        {
            var parsed = XPacket.Parse(packet);

            if (parsed != null)
            {
                ProcessIncomingPacket(parsed);
            }
        }
        
        private static async Task ProcessIncomingPacket(XPacket packet)
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
                case XPacketType.DisconnectPlayer:
                    ProcessDisconnectPlayer(packet);
                    break;
                case XPacketType.PlayerInfoNotAvailable:
                    ProcessPlayerInfoNotAvailable(packet);
                    break;
                case XPacketType.ChangeTurn:
                    ProcessChangeTurn(packet);
                    break;
                case XPacketType.RollDice:
                    await ProcessRollDice(packet);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static async Task ConnectAndSendHandshakeAsync()
        {
            _handshakeCompletionSource = new TaskCompletionSource<bool>();

            Client = new XClient();
            Client.OnPacketRecieve += OnPacketRecieve; // Подписываемся на событие получения пакетов

            await Task.Run(() => Client.Connect("127.0.0.1", 4910));

            var rand = new Random();
            Storage.HandshakeMagic = rand.Next();

            Console.WriteLine("Sending handshake packet..");

            // Отправляем handshake-пакет
            Client.QueuePacketSend(
                XPacketConverter.Serialize(
                    XPacketType.Handshake,
                    new XPacketHandshake
                    {
                        MagicHandshakeNumber = Storage.HandshakeMagic
                    })
                    .ToPacket());

            // Ждем завершения handshake
            await _handshakeCompletionSource.Task;

            Console.WriteLine("Handshake completed successfully!");
        }
        
        private static void ProcessPlayer(XPacket packet)
        {
            var player = XPacketConverter.Deserialize<XPacketPlayer>(packet);
            Console.WriteLine(player.Name);
            Console.WriteLine(player.Color);
            Storage.AvailibleColors[player.Color] = 0;
            bool flag = player.Name.Equals(Storage.Name);
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                MainWindow.AddPlayerInfo(player.Name, Colors.GetColor(player.Color), flag);
            });
        }
        
        // TODO: обработка на клиенте случая когда хэндшейк не прошел 
        private static void ProcessHandshake(XPacket packet) 
        {
            var handshake = XPacketConverter.Deserialize<XPacketHandshake>(packet);

            if (Storage.HandshakeMagic - handshake.MagicHandshakeNumber == 15)
            {
                Console.WriteLine("Handshake successful!");
                _handshakeCompletionSource.SetResult(true);
            }
            else
            {
                Console.WriteLine("Handshake failed!");
            }
        }

        private static void ProcessDisconnectPlayer(XPacket packet)
        {
            var player = XPacketConverter.Deserialize<XPacketPlayer>(packet);
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                MainWindow.DeletePlayer(player.Name, color: Colors.GetColor(player.Color));
            });
        }

        private static void ProcessPlayerInfoNotAvailable(XPacket packet)
        {
            Storage.CorrectInf = false;
        }

        private static void ProcessChangeTurn(XPacket packet)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                MainWindow!.RollDiceButton.IsEnabled = true;
            });
        }

        private static async Task ProcessRollDice(XPacket packet)
        {
            var rollDice = XPacketConverter.Deserialize<XPacketRollDice>(packet);
            var token = Storage.PlayerTokens[rollDice.Name];
            var currentPosition = Storage.TokenPositions[token];
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await MainWindow.MoveToken(token, currentPosition, currentPosition + rollDice.Score);
            });
        }
    }