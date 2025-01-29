using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
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
            ShowCustomMessageBox("сообщение"); // Затем открываем окно для ввода имени
        }
        private async void ShowCustomMessageBox(string message)
        {
            var messageBox = new CustomMessageBox(message);
            await messageBox.ShowDialog(this);
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