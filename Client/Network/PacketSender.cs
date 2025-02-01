using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Client.Enums;
using Client.Views;
using XProtocol;
using XProtocol.Serializator;

namespace Client.Network ;

    public class PacketSender : PacketProcessor
    {
        public static async Task SendPlayersInfoRequest()
        {
            var pack = XPacket.Create(XPacketType.RequestPlayerInfo);
            Thread.Sleep(100);
            await Task.Run(() => Client!.QueuePacketSend(pack.ToPacket()));
        }

        public static async Task SendNewPlayerPacket(string playerName, int playerColor)
        {
            if (Storage.AvailibleColors[playerColor] == 0)
            {
                Console.WriteLine("Цвет занят");
                return;
            }
            
            Storage.Name = playerName;
            var pack = XPacketConverter.Serialize(
                XPacketType.NewPlayer,
                new XPacketPlayer
                {
                    Color = playerColor,
                    Name = playerName,
                });
            await Task.Delay(100);
            await Task.Run(() => Client!.QueuePacketSend(pack.ToPacket()));
        }

        public static async Task SendRollDice(string playerName, int playerScore)
        {
            var pack = XPacketConverter.Serialize(XPacketType.RollDice, new XPacketRollDice
            {
                Name = playerName,
                Score = playerScore,
            });
            await Task.Delay(100);
            await Task.Run(() => Client!.QueuePacketSend(pack.ToPacket()));
        }
    }