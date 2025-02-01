using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Client.Views;
using XProtocol;
using XProtocol.Serializator;
using static System.Net.WebRequestMethods;
using Path = Avalonia.Controls.Shapes.Path;

namespace Client.Views
{
    public partial class MainWindow : Window
    {
        private List<string> _imagePaths = new(); 
        private readonly Random _random_img = new();
        private DispatcherTimer? _timer_img;
        private bool _isRunning = false;
        private Image? _imageView;
        private Button? _rollDiceButton;
        private readonly Random _random = new();
        private Path? _playerToken;
        private DispatcherTimer? _movementTimer;
        private int _currentPosition = 0; // Позиция на поле (0 - старт)
        private const int _gridSize = 10; // Размер поля 10x10
        private const int _cellSize = 45; // Размер клетки 
        private Dictionary<Path, int> _tokenPositions = new(); // Словарь: фишка → её текущая позиция
        private Dictionary<string, Path> _playerTokens = new();  // Словарь: имя игрока → его фишка
        public static string PlayerName { get; set; }
        public static CustomMessageBox CustomMessageBox { get; set; }
        private Button? _toggleButton;
        public AlertMessageBox AlertMessage { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            
            this.Show(); // ������� ���������� ������� ����
            ShowCustomMessageBox(); // ����� ��������� ���� ��� ����� �����
            
            _imageView = this.FindControl<Image>("ImageView");
            _rollDiceButton = this.FindControl<Button>("RollDiceButton");
            _playerToken = this.FindControl<Path>("PlayerToken");

            // if (_rollDiceButton != null)
            //     _rollDiceButton.Click += RollDice_Click;
            
            ShowAlertWindow();
        }

        // Надо понять почему не работает, пока что он будет просто не давать продолжить если цвет занят
        private async void ShowAlertWindow()
        {
            AlertMessage = new AlertMessageBox();
        }
        

        private async void ShowCustomMessageBox()
        {
            //var messageBox = new CustomMessageBox();
            //await messageBox.ShowDialog(this);
            CustomMessageBox= new CustomMessageBox(this);  // �������� ������� MainWindow
            await CustomMessageBox.ShowDialog(this);  // ���������� ����
        }
        
        //пока закомментил хочу переделать 
        // private void RollDice_Click(object? sender, RoutedEventArgs e)
        // {
        //     int diceRoll = _random.Next(1, 7); // Бросок кубика (1-6)
        //     int targetPosition = Math.Min(_currentPosition + diceRoll, _gridSize * _gridSize - 1);
        //     MoveToken(_playerToken,_currentPosition, targetPosition);
        //     _currentPosition = targetPosition;
        // }

        private async Task RollDice(int score)
        {
            int targetPosition = Math.Min(_currentPosition + score, _gridSize * _gridSize - 1);
            await MoveToken(_playerToken,_currentPosition, targetPosition);
            _currentPosition = targetPosition;
        }

        // Это нужно чтобы учитывать змеи/лестницы (сначала надо починить, чтобы фишка ходила ровно, а не как АЛКОГОЛИК
        
        
        // private readonly Dictionary<int, int> _snakesAndLadders = new()
        // {
        //     { 3, 22 }, // Лестница: с клетки 3 на 22
        //     { 5, 8 },  // Лестница: с 5 на 8
        //     { 11, 26 }, // Лестница: с 11 на 26
        //     { 20, 29 }, // Лестница: с 20 на 29
        //     { 17, 4 },  // Змея: с 17 на 4
        //     { 19, 7 },  // Змея: с 19 на 7
        //     { 27, 1 },  // Змея: с 27 на 1
        //     { 21, 9 }   // Змея: с 21 на 9
        // };
        
        // private void RollDice_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        // {
        //     int diceRoll = _random.Next(1, 7); // Бросок кубика (1-6)
        //     int targetPosition = Math.Min(_currentPosition + diceRoll, _gridSize * _gridSize - 1);
        //
        //     MoveToken(_currentPosition, targetPosition);
        //
        //     // Проверяем, попал ли игрок на змею или лестницу
        //     if (_snakesAndLadders.ContainsKey(targetPosition))
        //     {
        //         int newPosition = _snakesAndLadders[targetPosition];
        //         MoveToken(targetPosition, newPosition); // Перемещаем фишку
        //         targetPosition = newPosition; // Обновляем текущую позицию
        //     }
        //
        //     _currentPosition = targetPosition;
        // }
        
        private async Task MoveToken(Path token, int start, int end)
        {
            if (token == null || !_tokenPositions.ContainsKey(token)) return;

            int steps = end - start;

            for (int i = 0; i < steps; i++)
            {
                _tokenPositions[token]++;  // Обновляем позицию в словаре
                await SmoothMoveTo(token, _tokenPositions[token]); // Передаём фишку и новую позицию
            }
        }


        /// <summary>
        /// Плавно перемещает указанную фишку в новую клетку
        /// </summary>
        private async Task SmoothMoveTo(Path token, int position)
        {
            if (token == null) return;

            int row = _gridSize - 1 - (position / _gridSize);
            int col = (position / _gridSize) % 2 == 0
                ? position % _gridSize
                : _gridSize - 1 - (position % _gridSize);

            double targetX = col * _cellSize + (_cellSize - token.Width) / 2;
            double targetY = row * _cellSize + _cellSize - token.Height;

            double startX = Canvas.GetLeft(token);
            double startY = Canvas.GetTop(token);

            int frames = 20;
            double dx = (targetX - startX) / frames;
            double dy = (targetY - startY) / frames;

            for (int i = 0; i < frames; i++)
            {
                double newX = startX + dx * (i + 1);
                double newY = startY + dy * (i + 1);
                
                Canvas.SetLeft(token, Math.Round(newX, 2));
                Canvas.SetTop(token, Math.Round(newY, 2));

                await Task.Delay(16); // 16 мс для плавного 60 FPS
            }
        }

        private void UpdateTokenPosition(Path token, int position)
        {
            if (token == null) return;

            // Обновляем позицию в словаре
            if (_tokenPositions.ContainsKey(token))
            {
                _tokenPositions[token] = position;
            }
            else
            {
                _tokenPositions.Add(token, position);
            }

            int row = _gridSize - 1 - (position / _gridSize);
            int col = (position / _gridSize) % 2 == 0
                ? position % _gridSize
                : _gridSize - 1 - (position % _gridSize);

            double xPos = col * _cellSize + (_cellSize - token.Width) / 2 + _random.Next(20, 50);
            double yPos = row * _cellSize + _cellSize - token.Height + _random.Next(20, 50);

            Canvas.SetLeft(token, xPos);
            Canvas.SetTop(token, yPos);
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

        public void AddPlayerInfo(string playerName, string color, bool isClient = false)
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
            AddPlayerToken(playerName, color, isClient);
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
        public void AddPlayerToken(string playerName, string color, bool isClient = false)
        {
            var newToken = new Path
            {
                Width = 22.5,
                Height = 60,
                Data = Geometry.Parse("M 11.25,3.75 C 16.5,3.75 18.75,9 18.75,15 C 18.75,18.75 16.5,22.5 13.5,25.5 C 21,33.75 22.5,48.75 20.25,60 L 2.25,60 C 0,48.75 1.5,33.75 9,25.5 C 6,22.5 3.75,18.75 3.75,15 C 3.75,9 6,3.75 11.25,3.75 Z"),
                Fill = new SolidColorBrush(Avalonia.Media.Color.Parse(color)),
                Stroke = Brushes.Black,
                StrokeThickness = 1.5
            };

            // Если это первая фишка, сохраняем её в _playerToken
            if (_playerTokens.Count == 0 || isClient)
            {
                _playerToken = newToken;
            }

            _playerTokens[playerName] = newToken; // Добавляем фишку в словарь
            _tokenPositions[newToken] = -1; // Сохраняем начальную позицию фишки
            TokenLayer.Children.Add(newToken); // Добавляем на поле

            UpdateTokenPosition(newToken, -1); // Устанавливаем начальную позицию
        }


        // логика кубика
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
         
            _imageView = this.FindControl<Image>("ImageView");
            _rollDiceButton = this.FindControl<Button>("RollDiceButton");

            if (_imageView is null || _rollDiceButton is null)
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
                _rollDiceButton.IsEnabled = false;
                return;
            }

            _rollDiceButton.Click += ToggleImageChanging;
        }

        // private void ToggleImageChanging(object? sender, EventArgs e)
        // {
        //     if (_imageView is null) return;
        //
        //     if (_isRunning)
        //     {
        //         _timer_img?.Stop();
        //         _isRunning = false;
        //     }
        //     else
        //     {
        //         _timer_img = new DispatcherTimer
        //         {
        //             Interval = TimeSpan.FromSeconds(0.08)
        //         };
        //         _timer_img.Tick += (s, args) => ChangeImage();
        //         _timer_img.Start();
        //         _isRunning = true;
        //     }
        // }
        
        private async void ToggleImageChanging(object? sender, EventArgs e)
        {
            if (_imageView is null || _imagePaths.Count == 0) return;

            _rollDiceButton.IsEnabled = false;

            // Если анимация уже идет, остановим её
            if (_isRunning)
            {
                _timer_img?.Stop();
                _isRunning = false;
                return;
            }

            // Начинаем анимацию
            _timer_img = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.1) // Промежуток между изменениями изображений
            };
    
            _timer_img.Tick += (s, args) => ChangeImage(); // Меняем изображение каждый тик
            _timer_img.Start();

            _isRunning = true;

            await Task.Delay(2000);

            _timer_img.Stop();
            _isRunning = false;

            // Определяем результат по последнему изображению
            string lastImage = _imagePaths[_random_img.Next(_imagePaths.Count)];
            _imageView.Source = new Bitmap(lastImage);
            var score = GetDiceResultFromImage(lastImage);
            Console.WriteLine($"Анимация завершена! Результат: {score}");
            await RollDice(Int32.Parse(score));
            _rollDiceButton.IsEnabled = true;
        }

        private void ChangeImage()
        {
            if (_imageView is null || _imagePaths.Count == 0) return;

            string randomImage = _imagePaths[_random_img.Next(_imagePaths.Count)];
            _imageView.Source = new Bitmap(randomImage);
        }
        
        private string GetDiceResultFromImage(string imagePath)
        {
            var fileName = imagePath[^7];
            return fileName.ToString();
        }

        public void DeletePlayer(string playerName, string color)
        {
            StackPanel? playerToRemove = null;

            foreach (var child in PlayersListPanel.Children)
            {
                if (child is StackPanel playerPanel)
                {
                    var textBlock = playerPanel.Children.OfType<TextBlock>().FirstOrDefault();
                    if (textBlock != null && textBlock.Text == playerName)
                    {
                        playerToRemove = playerPanel;
                        break;
                    }
                }
            }

            if (playerToRemove != null)
            {
                PlayersListPanel.Children.Remove(playerToRemove);
            }

            RemovePlayerToken(color);
        }

        private void RemovePlayerToken(string color)
        {
            var tokenToRemove = TokenLayer.Children.OfType<Path>()
                .FirstOrDefault(token => ((SolidColorBrush)token.Fill).Color == Avalonia.Media.Color.Parse(color));

            if (tokenToRemove != null)
            {
                TokenLayer.Children.Remove(tokenToRemove);
            }
        }

    }
}