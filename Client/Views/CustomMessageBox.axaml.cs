using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Client.Network;
using Colors = Client.Enums.Colors;

namespace Client.Views
{
    public partial class CustomMessageBox : Window
    {
        private string _selectedColor = "None"; // Хранит выбранный цвет
        private string _playerName = ""; // Хранит имя игрока
        private readonly MainWindow _mainWindow;
        private bool _isProcessing = false;



        //public CustomMessageBox()
        //{
        //    InitializeComponent(); // Загружаем XAML перед изменениями
        //}
        public CustomMessageBox(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;  // Сохраняем ссылку на главное окно
            InitializeComponent();
        }
        
        private void OnColorSelected(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                string selectedColor = clickedButton.Tag?.ToString() ?? "None";
                Console.WriteLine($"Выбран цвет: {selectedColor}");

                if (_selectedColor != selectedColor)
                {
                    _selectedColor = selectedColor;
                    ResetButtonBorders(); // Сбрасываем обводку перед установкой новой

                    // Добавляем задержку, чтобы убедиться, что изменения применяются
                    Dispatcher.UIThread.Post(() =>
                    {
                        clickedButton.BorderBrush = Brushes.Black;
                        clickedButton.BorderThickness = new Thickness(3);
                    }, Avalonia.Threading.DispatcherPriority.Background);
                }
            }
        }
        
        private void ResetButtonBorders()
        {
            var buttons = new[] { RedButton, GreenButton, YellowButton, BlueButton };

            foreach (var button in buttons)
            {
                if (button != null)
                {
                    button.BorderBrush = Brushes.Transparent;
                    button.BorderThickness = new Thickness(0);
                }
            }
        }
        
        private async void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            if(_isProcessing) return;
            _isProcessing = true;
            Ok.IsEnabled = false;
            _playerName = UserInputTextBox.Text; // Получаем имя игрока из текстового поля
            MainWindow.PlayerName = _playerName;
            if (!string.IsNullOrWhiteSpace(_playerName) && _selectedColor != "None")
            {
                // _mainWindow.AddPlayerInfo(_playerName, _selectedColor);

                try
                {
                    await PacketProcessor.ConnectAndSendHandshakeAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Подключение не удалось: " + ex.Message);
                }
                finally
                {
                    Ok.IsEnabled = true;
                }
                
                PacketProcessor.MainWindow = _mainWindow;
                PacketProcessor.CustomMessageBox = this;
                await Task.Delay(100);
                await PacketSender.SendNewPlayerPacket(_playerName, Colors.GetColorId(_selectedColor));
                await Task.Delay(100);
                if (PacketProcessor.CorrectInf)
                {
                    // при такой реализации не будет работать проверка на цвет для второго юзера хз что делать с этим пока что
                    await PacketSender.SendPlayersInfoRequest();
                    await Task.Delay(100);
                    Close();
                }
                PacketProcessor.CorrectInf = true;
                Notification.IsVisible = true;
                Ok.IsEnabled = true;
                _isProcessing = false;
            }
        }
        }
    }

