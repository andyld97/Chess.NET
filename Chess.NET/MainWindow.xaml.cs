using System.Windows;
using System.Windows.Input;

namespace Chess.NET
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ICommand MoveLeftCommand { get; }

        public ICommand MoveRightCommand { get; }

        public MainWindow()
        {
            InitializeComponent();
            Chessboard.Game.MovedPiece += Game_MovedPiece;

            MoveLeftCommand = new RelayCommand(new Action(() => Chessboard.ShowPreviousMove()));
            MoveRightCommand = new RelayCommand(new Action(() => Chessboard.ShowNextMove()));
            DataContext = this;
        }

        // TODO: Anzeige welcher Spieler gerade mehr Figuren hat oder welche Figuren der Spieler schon geschlagen hat

        private void Game_MovedPiece(Model.Move move)
        {
            if (move.Piece.Color == Model.PieceColor.White)
                ListMoves.Items.Add($"{move.Count}. {move}");
            else
                ListMoves.Items[^1] = ListMoves.Items[^1] + $" | {move}";

            ListMoves.SelectedIndex = ListMoves.Items.Count - 1;
        }

        private void ButtonRestart_Click(object sender, RoutedEventArgs e)
        {
            ListMoves.Items.Clear();
            Chessboard.Restart();
        }

        private void ButtonMirror_Click(object sender, RoutedEventArgs e)
        {
            Chessboard.Mirror();
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
            => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter)
            => _execute();

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}