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
    public partial class AlertMessageBox : Window
    {
        public AlertMessageBox()
        {
            InitializeComponent();
        }
    }
}

