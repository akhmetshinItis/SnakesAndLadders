using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using TCPClient;
using XProtocol;
using XProtocol.Serializator;

namespace Client
{
    public partial class MainWindow : Window
    {
        private Button? _button;
        private TextBox _textBox;

        public MainWindow()
        {
            InitializeComponent();
            _button = this.FindControl<Button>("Button");
            _textBox = this.FindControl<TextBox>("TextBox");
            _button.Click += OnClick;
        }

        private async void OnClick(object? sender, RoutedEventArgs e)
        {
            _button.IsEnabled = false;
            Console.WriteLine(1);
            try
            {
                await ConnectAndSendAsync();
            }
            finally
            {
                _button.IsEnabled = true;
            }

            Console.WriteLine(2);
        }

        private async void OnClick2(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("OnClick2");
        }


        public async Task ConnectAndSendAsync()
        {
            var client = new XClient();
            client.OnPacketRecieve += OnPacketRecieve;
            await Task.Run(() => client.Connect("127.0.0.1", 4910));


            var pack = XPacketConverter.Serialize(
                XPacketType.NewPlayer,
                new XPacketPlayer
                {
                    Count = 3,
                    Name = _textBox.Text,
                }
                );
            var encryptedPacket = XProtocolEncryptor.Encrypt(pack.ToPacket());
            var test = pack.ToPacket();
            Console.WriteLine($"{test[0]:X2} {test[1]:X2} {test[2]:X2}");
            Console.WriteLine($"{encryptedPacket[0]:X2}  {encryptedPacket[1]:X2}  {encryptedPacket[2]:X2}");
            client.QueuePacketSend(encryptedPacket);
        }

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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private static void ProcessPlayer(XPacket packet)
        {
            var player = XPacketConverter.Deserialize<XPacketPlayer>(packet);
            Console.WriteLine(player.Name);
            Console.WriteLine(player.Count);
        }
    }
}