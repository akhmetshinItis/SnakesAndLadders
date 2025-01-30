using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Client.Network;

namespace Client.Views
{
    public partial class CustomMessageBox : Window
    {
        private string _selectedColor = "None"; // Хранит выбранный цвет
        private string _playerName = ""; // Хранит имя игрока
        private readonly MainWindow _mainWindow;



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

        // TEST
        private async void TestCaller(object sender, RoutedEventArgs e)
        {
            await Test();
        }
        
        private async Task Test()
        {
            await PacketProcessor.ConnectAndSendHandshakeAsync();
        }

        private async void TestCaller2(object sender, RoutedEventArgs e)
        {
            await Test2();
        }

        private async Task Test2()
        {
            await PacketProcessor.ConnectAndSendHandshakeAsync();
        }
        // TEST
        
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

        //private void OnOkButtonClick(object sender, RoutedEventArgs e)
        //{
        //    Console.WriteLine($"Выбранный цвет: {_selectedColor} перед закрытием");
        //    Close();
        //}
        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            _playerName = UserInputTextBox.Text; // Получаем имя игрока из текстового поля

            if (!string.IsNullOrWhiteSpace(_playerName) && _selectedColor != "None")
            {
                // Добавляем информацию о игроке в список
                _mainWindow.AddPlayerInfo(_playerName, _selectedColor);

                // Закрыть окно после нажатия "OK"
                Close();
            }

        }
    }
}
