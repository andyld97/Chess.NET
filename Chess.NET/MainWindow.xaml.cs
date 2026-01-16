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
            Chessboard.Game.MovedPiece += Game_MovedPiece;
        }

        // TODO: Anzeige welcher Spieler gerade mehr Figuren hat oder welche Figuren der Spieler schon geschlagen hat
        // TODO: Zug-Navigation mit Pfeiltasten implementieren!

        private void Game_MovedPiece(Model.Move move)
        {
            if (move.PieceColor == Model.PieceColor.White)
                TxtMoves.Text += $"{move.Count}. {move}";
            else
                TxtMoves.Text += $" | {move}\n";
        }

        private void ButtonRestart_Click(object sender, RoutedEventArgs e)
        {
            TxtMoves.Text = string.Empty;
            Chessboard.Restart();
        }

        private void ButtonMirror_Click(object sender, RoutedEventArgs e)
        {
            Chessboard.Mirror();
        }
    }
}