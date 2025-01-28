using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Views
{
    public partial class CustomMessageBox : Window
    {
        //public CustomMessageBox(string message)
        //{
        //    InitializeComponent(); // Инициализация компонентов XAML
        //    MessageText.Text = message; // Устанавливаем текст сообщения после инициализации
        //}
        public CustomMessageBox(string message)
        {
            InitializeComponent(); // Инициализация компонентов XAML

            if (MessageText != null)
            {
                MessageText.Text = message; // Устанавливаем текст сообщения
            }
            else
            {
                Console.WriteLine("MessageText is null!");
            }
        }



        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this); // Загружаем XAML для этого окна
        }

        // Обработчик кнопки закрытия окна
        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close(); // Закрываем окно при нажатии на кнопку
        }
    }
}