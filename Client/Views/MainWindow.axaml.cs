using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Client.Views;
using TCPClient;
using XProtocol;
using XProtocol.Serializator;
using static System.Net.WebRequestMethods;
using Path = Avalonia.Controls.Shapes.Path;

namespace Client.Views
{
    public partial class MainWindow : Window
    {
        private List<string> _imagePaths = new(); 
        private readonly Random _random = new();
        private DispatcherTimer? _timer;
        private bool _isRunning = false;
        private Image? _imageView;
        private Button? _toggleButton;
        
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

        //public void AddPlayerInfo(string playerName, string color)
        //{
        //    var playerPanel = new StackPanel
        //    {
        //        Orientation = Orientation.Horizontal,
        //        Spacing = 10
        //    };


        //    var playerInfoText = new TextBlock
        //    {
        //        Text = playerName,
        //        Foreground = Brushes.Black,
        //        FontSize = 18,
        //        FontWeight = FontWeight.Bold
        //    };


        //    var playerColorPath = new Path
        //    {
        //        Width = 30,
        //        Height = 30,
        //        Data = Geometry.Parse("M 12,2 A 10,10 0 1,0 12,22 A 10,10 0 1,0 12,2 Z"),
        //        Fill = new SolidColorBrush(Avalonia.Media.Color.Parse(color)),
        //        Stroke = Brushes.Black, 
        //        StrokeThickness = 1 
        //    };

        //    playerPanel.Children.Add(playerColorPath);
        //    playerPanel.Children.Add(playerInfoText);

        //    PlayersListPanel.Children.Add(playerPanel);
        //}

        public void AddPlayerInfo(string playerName, string color)
        {
            var playerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var playerInfoText = new TextBlock
            {
                Text = playerName,
                Foreground = Brushes.Black,
                FontSize = 18,
                FontWeight = FontWeight.Bold
            };

            var playerColorPath = new Path
            {
                Width = 30,
                Height = 30,
                Data = Geometry.Parse("M 12,2 A 10,10 0 1,0 12,22 A 10,10 0 1,0 12,2 Z"),
                Fill = new SolidColorBrush(Avalonia.Media.Color.Parse(color)),
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            playerPanel.Children.Add(playerColorPath);
            playerPanel.Children.Add(playerInfoText);

            PlayersListPanel.Children.Add(playerPanel);

            // Добавляем фишку игрока
            AddPlayerToken(color);
        }


        //    private void AddPlayerToken(string color)
        //{
        //    var token = new Path
        //    {
        //        Width = 30,
        //        Height = 80,
        //        Data = Geometry.Parse(" M 15,5 C 22,5 25,12 25,20 C 25,25 22,30 18,34 C 28,45 30,65 27,80 L 3,80 C 0,65 2,45 12,34 C 8,30 5,25 5,20 C 5,12 8,5 15,5 Z"),

        //        Fill = new SolidColorBrush(Avalonia.Media.Color.Parse(color)),
        //        Stroke = Brushes.Black,
        //        StrokeThickness = 1.5
        //    };

        //    var tokenContainer = new Canvas
        //    {
        //        Width = 50,
        //        Height = 100
        //    };

        //    tokenContainer.Children.Add(token);

        //    // Устанавливаем позицию фишки внизу слева
        //    Canvas.SetLeft(tokenContainer, 100); // Отступ слева
        //    Canvas.SetTop(tokenContainer, MainGrid.Bounds.Height - 100); // Отступ снизу

        //    MainGrid.Children.Add(tokenContainer);
        //}
        private void AddPlayerToken(string color)
        {
            var token = new Path
            {
                Width = 30,
                Height = 80,
                Data = Geometry.Parse("M 15,5 C 22,5 25,12 25,20 C 25,25 22,30 18,34 C 28,45 30,65 27,80 L 3,80 C 0,65 2,45 12,34 C 8,30 5,25 5,20 C 5,12 8,5 15,5 Z"),
                Fill = new SolidColorBrush(Avalonia.Media.Color.Parse(color)),
                Stroke = Brushes.Black,
                StrokeThickness = 1.5
            };

            var tokenContainer = new Canvas
            {
                Width = 50,
                Height = 100
            };

            tokenContainer.Children.Add(token);
            MainGrid.Children.Add(tokenContainer);

            // Гарантируем, что MainGrid уже имеет размеры
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                double xPos = 100; // Отступ слева
                double yPos = MainGrid.Bounds.Height - 110; // Отступ от нижней границы

                Canvas.SetLeft(tokenContainer, xPos);
                Canvas.SetTop(tokenContainer, yPos);
            }, DispatcherPriority.Background);
        }


        // логика кубикка
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
         
            _imageView = this.FindControl<Image>("ImageView");
            _toggleButton = this.FindControl<Button>("ToggleButton");

            if (_imageView is null || _toggleButton is null)
            {
                Console.WriteLine("Ошибка: Не удалось найти элементы управления!");
                return;
            }

            string imageFolder = "Assets/Images/cube";

            // Теперь можно присвоить значение без ошибки
            _imagePaths = Directory.Exists(imageFolder)
                ? Directory.GetFiles(imageFolder, "*.png").ToList()
                : new List<string>();

            if (_imagePaths.Count == 0)
            {
                Console.WriteLine("Ошибка: Нет изображений в папке Images.");
                _toggleButton.IsEnabled = false;
                return;
            }

            _toggleButton.Click += ToggleImageChanging;
        }

        private void ToggleImageChanging(object? sender, EventArgs e)
        {
            if (_imageView is null) return;

            if (_isRunning)
            {
                _timer?.Stop();
                _isRunning = false;
            }
            else
            {
                _timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(0.08)
                };
                _timer.Tick += (s, args) => ChangeImage();
                _timer.Start();
                _isRunning = true;
            }
        }

        private void ChangeImage()
        {
            if (_imageView is null || _imagePaths.Count == 0) return;

            string randomImage = _imagePaths[_random.Next(_imagePaths.Count)];
            _imageView.Source = new Bitmap(randomImage);
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
        
    }
}