using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace TCPClient
{
    public class XClient
    {
        public string? Name { get; set; }
        private readonly ConcurrentQueue<byte[]> _packetSendingQueue = new ConcurrentQueue<byte[]>();
        private readonly ManualResetEventSlim _packetAvailable = new ManualResetEventSlim(false);

        // TODO: добавить идентификатор
        public Action<byte[]> OnPacketRecieve { get; set; }

        // private readonly Queue<byte[]> _packetSendingQueue = new Queue<byte[]>();

        private Socket _socket;
        private IPEndPoint _serverEndPoint;

        public void Connect(string ip, int port)
        {
            Connect(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        public void Connect(IPEndPoint server)
        {
            _serverEndPoint = server;

            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());  
            var ipAddress = ipHostInfo.AddressList[0];

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(_serverEndPoint);

            Task.Run((Action) RecievePackets);
            Task.Run((Action) SendPackets);
        }

        public void QueuePacketSend(byte[] packet)
        {
            if (packet.Length > 256)
            {
                throw new Exception("Максимальный размер пакета – 256 байт.");
            }

            _packetSendingQueue.Enqueue(packet);
            _packetAvailable.Set(); // Уведомляем поток, что есть новый пакет
        }

        private void RecievePackets()
        {
            try
            {
                while (_socket.Connected) // Проверяем, подключен ли сокет
                {
                    var buff = new byte[256];
                    int received = _socket.Receive(buff);

                    if (received == 0) // Если сокет закрылся — выходим из цикла
                    {
                        Console.WriteLine("Socket closed by remote host.");
                        break;
                    }

                    // Обрезаем данные, оставляя только полученные байты
                    var packet = buff.Take(received).ToArray();

                    // Фильтруем пакет, обрезая лишние данные
                    packet = FilterPacket(packet);

                    // Вызываем событие
                    OnPacketRecieve?.Invoke(packet);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Stopping packet receiving...");
                _socket.Close();
            }
        }

        /// <summary>
        /// Фильтрует пакет, удаляя ненужные байты.
        /// </summary>
        private byte[] FilterPacket(byte[] packet)
        {
            return packet.TakeWhile((b, i) =>
            {
                if (b != 0xFF) return true;
                return i + 1 < packet.Length && packet[i + 1] != 0;
            }).Concat(new byte[] { 0xFF, 0 }).ToArray();
        }


        // private void SendPackets()
        // {
        //     while (true)
        //     {
        //         if (_packetSendingQueue.Count == 0)
        //         {
        //             Thread.Sleep(100);
        //             continue;
        //         }
        //
        //         var packet = _packetSendingQueue.Dequeue();
        //         _socket.Send(packet);
        //
        //         Thread.Sleep(100);
        //     }
        // }
        private void SendPackets()
        {
            while (_socket.Connected)
            {
                _packetAvailable.Wait(); // Ждем появления пакетов в очереди
                _packetAvailable.Reset(); // Сбрасываем ожидание

                while (_packetSendingQueue.TryDequeue(out var packet)) // Забираем пакет
                {
                    try
                    {
                        _socket.Send(packet);
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"Ошибка отправки пакета: {ex.Message}");
                        return;
                    }
                }
            }
        }
    }
}
