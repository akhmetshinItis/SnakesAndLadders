using System.Threading;
using System.Threading.Tasks;
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
            Client.Name = playerName;
            var pack = XPacketConverter.Serialize(
                XPacketType.NewPlayer,
                new XPacketPlayer
                {
                    Color = playerColor,
                    Name = playerName,
                });

            Thread.Sleep(100);
            await Task.Run(() => Client!.QueuePacketSend(pack.ToPacket()));
        }
    }