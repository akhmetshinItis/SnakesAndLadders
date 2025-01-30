using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
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
            await customMessageBox.ShowDialog(this);  // ���������� ����

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
    }
}