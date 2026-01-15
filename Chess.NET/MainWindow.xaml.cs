using System.Windows;

namespace Chess.NET
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonRestart_Click(object sender, RoutedEventArgs e)
        {
            Chessboard.Restart();
        }
    }
}