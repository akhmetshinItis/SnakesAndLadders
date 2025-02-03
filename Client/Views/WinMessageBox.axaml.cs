using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Client.Views
{
    public partial class WinMessageBox : Window
    {
        public WinMessageBox(string playerName)
        {
            InitializeComponent();
            WinnerText.Text = $"Игру выиграл игрок: {playerName}";
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            // Закрываем окно при нажатии на кнопку "OK"
            this.Close();
        }
    }
}