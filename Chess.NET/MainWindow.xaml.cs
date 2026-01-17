using Chess.NET.Bot;
using System.Windows;
using System.Windows.Input;

namespace Chess.NET
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isMirrored = false;
        private IChessBot? opponent = null;

        public ICommand MoveLeftCommand { get; }

        public ICommand MoveRightCommand { get; }

        public MainWindow()
        {
            InitializeComponent();
            Chessboard.Game.MovedPiece += Game_MovedPiece;

            MoveLeftCommand = new RelayCommand(new Action(Chessboard.ShowPreviousMove));
            MoveRightCommand = new RelayCommand(new Action(Chessboard.ShowNextMove));
            DataContext = this;

            RefreshPlayerDisplay();
        }

        private void Game_MovedPiece(Model.Move move)
        {
            if (move.Piece.Color == Model.PieceColor.White)
                ListMoves.Items.Add($"{move.Count}. {move}");
            else
                ListMoves.Items[^1] = $"{ListMoves.Items[^1]} | {move}";

            ListMoves.SelectedIndex = ListMoves.Items.Count - 1;

            RefreshPlayerDisplay();
        }

        private void ButtonRestart_Click(object sender, RoutedEventArgs e)
        {
            ListMoves.Items.Clear();

            if (CmbOpponent.SelectedIndex == 0)
                opponent = new StockfischBot(0, 2, @"F:\Eigene Dateien\Downloads\stockfish-windows-x86-64\stockfish\stockfish-windows-x86-64.exe");
            else if (CmbOpponent.SelectedIndex == 1)
                opponent = new StupidoBot();
            else
                opponent = null;

            Chessboard.Restart(opponent);
            RefreshPlayerDisplay();
        }

        private void ButtonMirror_Click(object sender, RoutedEventArgs e)
        {
            Chessboard.Mirror();
            isMirrored = !isMirrored;
            RefreshPlayerDisplay();
        }

        private void ButtonJumpToStart_Click(object sender, RoutedEventArgs e)
        {
            Chessboard.JumpToStart();
        }

        private void ButtonJumpToPreviousMove_Click(object sender, RoutedEventArgs e)
        {
            Chessboard.ShowPreviousMove();
        }

        private void ButtonJumpToNextMove_Click(object sender, RoutedEventArgs e)
        {
            Chessboard.ShowNextMove();
        }

        private void ButtonJumpToLastMove_Click(object sender, RoutedEventArgs e)
        {
            Chessboard.ShowLastMove();
        }

        private void ListMoves_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Chessboard.ShowMove(ListMoves.SelectedIndex);
        }

        private void RefreshPlayerDisplay()
        {
            // TODO: Wenn in NavigationMode muss das auch nochmal aufgerufen werden, aber dann seine Infos von dem "anderen" Game beziehen.
            var playerInfo = Chessboard.Game.GetPlayerInformation();


            static string formatElo(int? value)
            {
                if (value == null)
                    return "Elo: ????";
                else
                    return $"Elo: {value:D4}";
            }

            if (opponent is null)
            {
                if (isMirrored)
                {
                    // Top:    Player 1 (White)
                    // Bottom: Player 2 (Black)
                    TextPlayerTopName.Text = "Player 1";
                    TextPlayerTopElo.Text = formatElo(null);

                    TextPlayerBottomName.Text = "Player 2";
                    TextPlayerBottomElo.Text = formatElo(null);

                    TextPlayerInfoTop.Text = playerInfo.GetWhite();
                    TextPlayerInfoBottom.Text = playerInfo.GetBlack();
                }
                else
                {
                    // Top:     Player 2 (Black)
                    // Bottom:  Player 1 (White)
                    TextPlayerTopName.Text = "Player 2";
                    TextPlayerTopElo.Text = formatElo(null);

                    TextPlayerBottomName.Text = "Player 1";
                    TextPlayerBottomElo.Text = formatElo(null);

                    TextPlayerInfoTop.Text = playerInfo.GetBlack();
                    TextPlayerInfoBottom.Text = playerInfo.GetWhite();
                }
            }
            else
            {
                if (isMirrored)
                {
                    // Top:    Player 1 (White)
                    // Bottom: Bot      (Black)
                    TextPlayerTopName.Text = "Player 1";
                    TextPlayerTopElo.Text = formatElo(null);

                    TextPlayerBottomName.Text = $"{opponent.Name} (Bot)";
                    TextPlayerBottomElo.Text = formatElo(opponent.Elo);

                    TextPlayerInfoTop.Text = playerInfo.GetWhite();
                    TextPlayerInfoBottom.Text = playerInfo.GetBlack();
                }
                else
                {
                    // Top:    Bot      (Black)
                    // Bottom: Player 1 (White)
                    TextPlayerTopName.Text = $"{opponent.Name} (Bot)";
                    TextPlayerTopElo.Text = formatElo(opponent.Elo);

                    TextPlayerBottomName.Text = "Player 1";
                    TextPlayerBottomElo.Text = formatElo(null);

                    TextPlayerInfoTop.Text = playerInfo.GetBlack();
                    TextPlayerInfoBottom.Text = playerInfo.GetWhite();
                }
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}