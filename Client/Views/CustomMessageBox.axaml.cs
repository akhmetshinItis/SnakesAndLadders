using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Client.Enums;
using Client.Network;
using Colors = Client.Enums.Colors;

namespace Client.Views
{
    public partial class CustomMessageBox : Window
    {
        private string _selectedColor = "None"; // Хранит выбранный цвет
        private string _playerName = ""; // Хранит имя игрока
        private readonly MainWindow _mainWindow;
        private bool _isProcessing { get; set; } = false;
        public CustomMessageBox(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
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
                    }, DispatcherPriority.Background);
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
            _playerName = UserInputTextBox.Text;
            Storage.Name = _playerName;
            if (!string.IsNullOrWhiteSpace(_playerName) && _selectedColor != "None")
            {
                try
                {
                    await PacketProcessor.ConnectAndSendHandshakeAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Подключение не удалось: " + ex.Message);
                    ServerNotAvailible.IsVisible = true;
                    _isProcessing = false;
                    return;
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
                if (Storage.CorrectInf)
                {
                    await PacketSender.SendPlayersInfoRequest();
                    await Task.Delay(100);
                    Close();
                }
                Storage.CorrectInf = true;
                Notification.IsVisible = true;
                Ok.IsEnabled = true;
                _isProcessing = false;
            }
            else
            {
                ColorOrNameNotSelected.IsVisible = true;
                Ok.IsEnabled = true;
                _isProcessing = false;
                return;
            }
        }
        }
    }

