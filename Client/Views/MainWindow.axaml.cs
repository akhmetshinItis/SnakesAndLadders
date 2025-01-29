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
            this.Show(); // Сначала показываем главное окно
            ShowCustomMessageBox(); // Затем открываем окно для ввода имени
        }
        private async void ShowCustomMessageBox()
        {
            //var messageBox = new CustomMessageBox();
            //await messageBox.ShowDialog(this);
            var customMessageBox = new CustomMessageBox(this);  // Передаем текущий MainWindow
            customMessageBox.ShowDialog(this);  // Показываем окно

        }

        // Метод для добавления игрока и его цвета в список
        //public void AddPlayerInfo(string playerName, string color)
        //{
        //    // Создаем новый TextBlock с информацией о игроке
        //    var playerInfoText = new TextBlock
        //    {
        //        Text = $"{playerName} - {color}",
        //        Foreground = Brushes.Black,
        //        FontWeight = FontWeight.Bold,
        //        FontSize = 16
        //    };

        //    // Добавляем этот элемент в PlayersListPanel
        //    PlayersListPanel.Children.Add(playerInfoText);
        //}
        public void AddPlayerInfo(string playerName, string color)
        {
            // Создаем новый StackPanel для каждого игрока
            var playerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            // Создаем TextBlock с именем игрока
            var playerInfoText = new TextBlock
            {
                Text = playerName,
                Foreground = Brushes.Black,
                FontSize = 18,
                FontWeight = FontWeight.Bold
            };

            // Создаем Path для фишки игрока с нужным цветом
            var playerColorPath = new Path
            {
                Width = 30,
                Height = 30,
                Data = Geometry.Parse("M 12,2 A 10,10 0 1,0 12,22 A 10,10 0 1,0 12,2 Z"), // Пример SVG
                Fill = new SolidColorBrush(Avalonia.Media.Color.Parse(color)),// Используем Avalonia.Media.Color.Parse
                Stroke = Brushes.Black, // Черная обводка
                StrokeThickness = 1 // Толщина обводки 1 пиксель
            };

            // Добавляем текст и фишку в StackPanel игрока
            playerPanel.Children.Add(playerColorPath);
            playerPanel.Children.Add(playerInfoText);

            // Добавляем созданный элемент в PlayersListPanel
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