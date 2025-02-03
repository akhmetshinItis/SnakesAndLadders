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
using Client.Enums;
using Client.Network;
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
        private bool _isRunning { get; set; }= false;
        private Image? _imageView;
        private Button? _rollDiceButton;
        private readonly Random _random = new();
        private Path? _playerToken;
        private DispatcherTimer? _movementTimer;
        private int _currentPosition { get; set; } = 0; // Позиция на поле (0 - старт)
        private const int _gridSize = 10; // Размер поля 10x10
        private const int _cellSize = 45; // Размер клетки 
        public static CustomMessageBox CustomMessageBox { get; set; }
        private Button? _toggleButton;


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
        }
        

        private async void ShowCustomMessageBox()
        {
            //var messageBox = new CustomMessageBox();
            //await messageBox.ShowDialog(this);
            CustomMessageBox= new CustomMessageBox(this);  // �������� ������� MainWindow
            await CustomMessageBox.ShowDialog(this);  // ���������� ����
        }

        private async Task ShowWinMessageBox(Path token)
        {
            // Получаем имя игрока по его фишке
            string playerName = Storage.PlayerTokens.FirstOrDefault(x => x.Value == token).Key;

            // Создаем и показываем окно с сообщением о победе
            WinMessageBox winMessageBox = new WinMessageBox(playerName);
            await winMessageBox.ShowDialog(this); // Открываем окно как модальное
        }

        ////пока закомментил хочу переделать 
        //// private void RollDice_Click(object? sender, RoutedEventArgs e)
        //// {
        ////     int diceRoll = _random.Next(1, 7); // Бросок кубика (1-6)
        ////     int targetPosition = Math.Min(_currentPosition + diceRoll, _gridSize * _gridSize - 1);
        ////     MoveToken(_playerToken,_currentPosition, targetPosition);
        ////     _currentPosition = targetPosition;
        //// }

        //private async Task RollDice(int score)
        //{
        //    int targetPosition = Math.Min(_currentPosition + score, _gridSize * _gridSize - 1);
        //    await MoveToken(_playerToken,_currentPosition, targetPosition);
        //    _currentPosition = targetPosition;
        //}
        private async Task RollDice(int score)
        {
            int targetPosition = Math.Min(_currentPosition + score, _gridSize * _gridSize - 1);
            await MoveToken(_playerToken, _currentPosition, targetPosition);
           
        }


        //public async Task MoveToken(Path token, int start, int end)
        //{
        //    if (token == null || !Storage.TokenPositions.ContainsKey(token)) return;

        //    int steps = Math.Abs(end - start);

        //    // Обычное пошаговое движение до конечной позиции
        //    for (int i = 0; i < steps; i++)
        //    {
        //        Storage.TokenPositions[token]++;  // Обновляем позицию в словаре
        //        await SmoothMoveTo(token, Storage.TokenPositions[token]); // Двигаем фишку обычным способом
        //    }

        //    // Теперь обновляем текущую позицию фишки
        //    _currentPosition = end;

        //    // Проверяем, есть ли на этой клетке лестница или змея
        //    if (Storage.SnakesAndLadders.TryGetValue(_currentPosition, out int newPosition))
        //    {
        //        // Перемещаем фишку по диагонали к новой позиции
        //        await SmoothDiagonalMoveTo(token, _currentPosition, newPosition);

        //        // Обновляем текущую позицию фишки
        //        _currentPosition = newPosition;
        //        Storage.TokenPositions[token] = newPosition;
        //    }
        //    else
        //    {
        //        // Если лестницы или змеи нет, обновляем позицию в словаре
        //        Storage.TokenPositions[token] = _currentPosition;
        //    }
        //}
        public async Task MoveToken(Path token, int start, int end)
        {
            if (token == null || !Storage.TokenPositions.ContainsKey(token)) return;

            int steps = Math.Abs(end - start);

            // Обычное пошаговое движение до конечной позиции
            for (int i = 0; i < steps; i++)
            {
                Storage.TokenPositions[token]++;  // Обновляем позицию в словаре
                await SmoothMoveTo(token, Storage.TokenPositions[token]); // Двигаем фишку обычным способом
            }

            // Теперь обновляем текущую позицию фишки
            _currentPosition = end;

            // Проверяем, есть ли на этой клетке лестница или змея
            if (Storage.SnakesAndLadders.TryGetValue(_currentPosition, out int newPosition))
            {
                // Перемещаем фишку по диагонали к новой позиции
                await SmoothDiagonalMoveTo(token, _currentPosition, newPosition);

                // Обновляем текущую позицию фишки
                _currentPosition = newPosition;
                Storage.TokenPositions[token] = newPosition;
            }
            else
            {
                // Если лестницы или змеи нет, обновляем позицию в словаре
                Storage.TokenPositions[token] = _currentPosition;
            }

            // Проверка, достиг ли игрок последней клетки
            if (_currentPosition == (_gridSize * _gridSize - 1))
            {
                await ShowWinMessageBox(token); // Показываем окно о победе
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

        private async Task SmoothDiagonalMoveTo(Path token, int startPosition, int targetPosition)
        {
            //targetPosition--;
            if (token == null) return;

            // Вычисляем координаты стартовой и целевой позиции
            int startRow = _gridSize - 1 - (startPosition / _gridSize);
            int startCol = (startPosition / _gridSize) % 2 == 0
                ? startPosition % _gridSize
                : _gridSize - 1 - (startPosition % _gridSize);

            int targetRow = _gridSize - 1 - (targetPosition / _gridSize);
            int targetCol = (targetPosition / _gridSize) % 2 == 0
                ? targetPosition % _gridSize
                : _gridSize - 1 - (targetPosition % _gridSize);

            // Вычисляем координаты в пикселях
            double startX = startCol * _cellSize + (_cellSize - token.Width) / 2;
            double startY = startRow * _cellSize + _cellSize - token.Height;

            double targetX = targetCol * _cellSize + (_cellSize - token.Width) / 2;
            double targetY = targetRow * _cellSize + _cellSize - token.Height;

            int frames = 20; // Количество кадров для анимации
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
            if (Storage.TokenPositions.ContainsKey(token))
            {
                Storage.TokenPositions[token] = position;
            }
            else
            {
                Storage.TokenPositions.Add(token, position);
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
            if (Storage.PlayerTokens.Count == 0 || isClient)
            {
                _playerToken = newToken;
            }

            Storage.PlayerTokens[playerName] = newToken; // Добавляем фишку в словарь
            Storage.TokenPositions[newToken] = -1; // Сохраняем начальную позицию фишки
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
        
        
        private async void ToggleImageChanging(object? sender, EventArgs e)
        {
            if (_imageView is null || _imagePaths.Count == 0) return;

            _rollDiceButton.IsEnabled = false;

            if (_isRunning)
            {
                _timer_img?.Stop();
                _isRunning = false;
                return;
            }

            _timer_img = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.1) // Промежуток между изменениями изображений
            };
    
            _timer_img.Tick += (s, args) => ChangeImage();
            _timer_img.Start();

            _isRunning = true;

            await Task.Delay(2000);

            _timer_img.Stop();
            _isRunning = false;

            // Определяем результат по последнему изображению
            string lastImage = _imagePaths[_random_img.Next(_imagePaths.Count)];
            _imageView.Source = new Bitmap(lastImage);
            var score = Int32.Parse(GetDiceResultFromImage(lastImage));
            Console.WriteLine($"Анимация завершена! Результат: {score}");
            await PacketSender.SendRollDice(Storage.Name, score);
            await RollDice((score));
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