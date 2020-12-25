using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MultiplayerTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var isServer = Environment.GetCommandLineArgs().Contains("server");
            Title = isServer ? "Server" : "Client";
            DataContext = new MainViewModel(isServer);

            this.Loaded += async (s, e) => await ((MainViewModel)DataContext).InitAsync();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext is MainViewModel viewModel &&
                sender is IInputElement element)
            {
                var position = e.GetPosition(element);
                viewModel.SendMoveOrder(position.X, position.Y);
            }
        }
    }
}
