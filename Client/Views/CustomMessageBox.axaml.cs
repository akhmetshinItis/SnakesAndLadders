using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes; // Для использования Path
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Client.Views
{
    public partial class CustomMessageBox : Window
    {
        private string _selectedColor = "None"; // Хранит выбранный цвет

        public CustomMessageBox(string message)
        {
            InitializeComponent();
            if (MessageText != null)
            {
                MessageText.Text = message; // Установить текст сообщения
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // Обработчик выбора цвета
        private void OnColorSelected(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                // Получаем цвет из Tag кнопки
                string selectedColor = clickedButton.Tag?.ToString() ?? "None";
                Console.WriteLine($"Выбран цвет: {selectedColor}");

                // Если выбран новый цвет, обновляем выбранный цвет
                if (_selectedColor != selectedColor)
                {
                    _selectedColor = selectedColor;

                    // Сбрасываем рамки у всех кнопок
                    ResetButtonBorders();

                    // Устанавливаем рамку на выбранной кнопке
                    clickedButton.BorderBrush = Brushes.Black;
                    clickedButton.BorderThickness = new Thickness(3);
                }
            }
        }

        // Сброс границ у всех кнопок
        private void ResetButtonBorders()
        {
            // Проверка и сброс всех кнопок
            if (RedButton != null)
            {
                RedButton.BorderBrush = Brushes.Transparent;
                RedButton.BorderThickness = new Thickness(0);
            }
            if (GreenButton != null)
            {
                GreenButton.BorderBrush = Brushes.Transparent;
                GreenButton.BorderThickness = new Thickness(0);
            }
            if (YellowButton != null)
            {
                YellowButton.BorderBrush = Brushes.Transparent;
                YellowButton.BorderThickness = new Thickness(0);
            }
            if (BlueButton != null)
            {
                BlueButton.BorderBrush = Brushes.Transparent;
                BlueButton.BorderThickness = new Thickness(0);
            }
        }

        // Обработчик нажатия OK
        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"Выбранный цвет: {_selectedColor} перед закрытием");
            this.Close(); // Закрыть окно только при нажатии "OK"
        }
    }
}