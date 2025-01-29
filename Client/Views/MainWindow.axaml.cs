using System;
using System.Drawing;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Client.Views;
using TCPClient;
using XProtocol;
using XProtocol.Serializator;

namespace Client.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Show(); // ������� ���������� ������� ����
            ShowCustomMessageBox(); // ����� ��������� ���� ��� ����� �����
        }
        private async void ShowCustomMessageBox()
        {
            //var messageBox = new CustomMessageBox();
            //await messageBox.ShowDialog(this);
            var customMessageBox = new CustomMessageBox(this);  // �������� ������� MainWindow
            customMessageBox.ShowDialog(this);  // ���������� ����

        }

        // ����� ��� ���������� ������ � ��� ����� � ������
        //public void AddPlayerInfo(string playerName, string color)
        //{
        //    // ������� ����� TextBlock � ����������� � ������
        //    var playerInfoText = new TextBlock
        //    {
        //        Text = $"{playerName} - {color}",
        //        Foreground = Brushes.Black,
        //        FontWeight = FontWeight.Bold,
        //        FontSize = 16
        //    };

        //    // ��������� ���� ������� � PlayersListPanel
        //    PlayersListPanel.Children.Add(playerInfoText);
        //}
        public void AddPlayerInfo(string playerName, string color)
        {
            // ������� ����� StackPanel ��� ������� ������
            var playerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            // ������� TextBlock � ������ ������
            var playerInfoText = new TextBlock
            {
                Text = playerName,
                Foreground = Brushes.Black,
                FontSize = 18,
                FontWeight = FontWeight.Bold
            };

            // ������� Path ��� ����� ������ � ������ ������
            var playerColorPath = new Path
            {
                Width = 30,
                Height = 30,
                Data = Geometry.Parse("M 12,2 A 10,10 0 1,0 12,22 A 10,10 0 1,0 12,2 Z"), // ������ SVG
                Fill = new SolidColorBrush(Avalonia.Media.Color.Parse(color)),// ���������� Avalonia.Media.Color.Parse
                Stroke = Brushes.Black, // ������ �������
                StrokeThickness = 1 // ������� ������� 1 �������
            };

            // ��������� ����� � ����� � StackPanel ������
            playerPanel.Children.Add(playerColorPath);
            playerPanel.Children.Add(playerInfoText);

            // ��������� ��������� ������� � PlayersListPanel
            PlayersListPanel.Children.Add(playerPanel);
        }

        //private Button? _button;
        //private TextBox _textBox;

        //public MainWindow()
        //{
        //    InitializeComponent();
        //    _button = this.FindControl<Button>("Button");
        //    _textBox = this.FindControl<TextBox>("TextBox");
        //    _button.Click += OnClick;
        //}

        //private async void OnClick(object? sender, RoutedEventArgs e)
        //{
        //    _button.IsEnabled = false;
        //    Console.WriteLine(1);
        //    try
        //    {
        //        await ConnectAndSendAsync();
        //    }
        //    finally
        //    {
        //        _button.IsEnabled = true;
        //    }

        //    Console.WriteLine(2);
        //}

        //private async void OnClick2(object? sender, RoutedEventArgs e)
        //{
        //    Console.WriteLine("OnClick2");
        //}


        //public async Task ConnectAndSendAsync()
        //{
        //    var client = new XClient();
        //    client.OnPacketRecieve += OnPacketRecieve;
        //    await Task.Run(() => client.Connect("127.0.0.1", 4910));


        //    var pack = XPacketConverter.Serialize(
        //        XPacketType.NewPlayer,
        //        new XPacketPlayer
        //        {
        //            Count = 3,
        //            Name = _textBox.Text,
        //        }
        //        );
        //    var encryptedPacket = XProtocolEncryptor.Encrypt(pack.ToPacket());
        //    var test = pack.ToPacket();
        //    Console.WriteLine($"{test[0]:X2} {test[1]:X2} {test[2]:X2}");
        //    Console.WriteLine($"{encryptedPacket[0]:X2}  {encryptedPacket[1]:X2}  {encryptedPacket[2]:X2}");
        //    client.QueuePacketSend(encryptedPacket);
        //}

        //private static void OnPacketRecieve(byte[] packet)
        //{
        //    var parsed = XPacket.Parse(packet);

        //    if (parsed != null)
        //    {
        //        ProcessIncomingPacket(parsed);
        //    }
        //}

        //private static void ProcessIncomingPacket(XPacket packet)
        //{
        //    var type = XPacketTypeManager.GetTypeFromPacket(packet);

        //    switch (type)
        //    {
        //        case XPacketType.Unknown:
        //            break;
        //        case XPacketType.NewPlayer:
        //            ProcessPlayer(packet);
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException();
        //    }
        //}


        //private static void ProcessPlayer(XPacket packet)
        //{
        //    var player = XPacketConverter.Deserialize<XPacketPlayer>(packet);
        //    Console.WriteLine(player.Name);
        //    Console.WriteLine(player.Count);
        //}
    }
}