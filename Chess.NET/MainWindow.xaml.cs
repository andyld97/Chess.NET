using Chess.NET.Bot;
using Chess.NET.Controls.Dialogs;
using Chess.NET.Model;
using System.Windows;
using System.Windows.Controls;
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

        #region Commands

        public ICommand MoveLeftCommand { get; }

        public ICommand MoveRightCommand { get; }

        public ICommand NewGameCommand { get; }

        public ICommand MirrorBoardCommand { get; }

        #endregion

        #region Ctor
        public MainWindow()
        {
            InitializeComponent();

            // Assign events
            Chessboard.Game.OnMovedPiece += Game_MovedPiece;
            Chessboard.Game.OnCheckmate += Game_OnCheckmate;
            Chessboard.Game.OnStalemate += Game_OnStalemate;
            Chessboard.Game.OnPlaySound += Game_OnPlaySound;

            // Assign commands
            MoveLeftCommand = new RelayCommand(async () => await Chessboard.ShowPreviousMoveAsync());
            MoveRightCommand = new RelayCommand(async () => await Chessboard.ShowNextMoveAsync());
            NewGameCommand = new RelayCommand(new Action(StartNewGame));
            MirrorBoardCommand = new RelayCommand(new Action(Chessboard.Mirror));
            DataContext = this;

            RefreshPlayerDisplay();
            InitializePuzzleMenu();
        }

        #endregion

        #region Puzzle Menu

        private void InitializePuzzleMenu()
        {
            foreach (var puzzle in Puzzle.Puzzles)
            {
                MenuItem puzzleItem = new MenuItem
                {
                    Header = puzzle.Name,
                    Tag = puzzle
                };
                puzzleItem.Click += MenuPuzzle_Click;

                MenuPuzzle.Items.Add(puzzleItem);
            }
        }

        private void MenuPuzzle_Click(object sender, RoutedEventArgs e)
        {
            new PuzzleDialog((sender as MenuItem).Tag as Puzzle).ShowDialog();
        }

        #endregion

        #region Game Events

        private async void Game_OnCheckmate(PieceColor pieceColor)
        {
            string playerText = string.Empty;

            if (pieceColor == PieceColor.White)
            {
                if (opponent == null)
                    playerText = $"{Helper.GetPlayerName(1)} ({Properties.Resources.strBlack})";
                else
                    playerText = $"{opponent?.Name} [Bot] ({Properties.Resources.strBlack})";
            }
            else if (pieceColor == PieceColor.Black)
                playerText = $"{Helper.GetPlayerName(1)} ({Properties.Resources.strWhite})";

            await Task.Delay(250).ContinueWith(t =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(string.Format(Properties.Resources.strGameOver_WinMessage, playerText), Properties.Resources.strGameOver_Win, MessageBoxButton.OK, MessageBoxImage.Information);
                });
            });
        }

        private void Game_OnPlaySound(Sound.SoundType type)
        {
            // Always play
            Sound.Play(type);
        }

        private async void Game_OnStalemate()
        {
            await Task.Delay(250).ContinueWith(t =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(Properties.Resources.strGameOver_StalemateText, Properties.Resources.strGameOver_Stalemate, MessageBoxButton.OK, MessageBoxImage.Information);
                });
            });
        }

        private void Game_MovedPiece(Model.MoveNotation move)
        {
            if (move.Piece.Color == Model.PieceColor.White)
                ListMoves.Items.Add($"{move.Count}. {move}");
            else
                ListMoves.Items[^1] = $"{ListMoves.Items[^1]} | {move}";

            ListMoves.SelectedIndex = ListMoves.Items.Count - 1;

            RefreshPlayerDisplay();
        }


        #endregion

        private void StartNewGame()
        {
            ListMoves.Items.Clear();

            if (CmbOpponent.SelectedIndex == 0)
            {
                if (!System.IO.File.Exists(Settings.Instance.StockfishPath))
                {
                    MessageBox.Show(Properties.Resources.strStockfishNotFound, Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int skill = 8;
                int depth = 10;

                if (Settings.Instance.Difficulty == 0)
                {
                    skill = 0;
                    depth = 2;
                }
                else if (Settings.Instance.Difficulty == 1)
                {
                    skill = 3;
                    depth = 4;
                }
                else if (Settings.Instance.Difficulty == 2)
                {
                    skill = 8;
                    depth = 10;
                }

                opponent = new StockfischBot(skill, depth, Settings.Instance.StockfishPath);
            }
            else if (CmbOpponent.SelectedIndex == 1)
                opponent = new StupidoBot();
            else
                opponent = null;

            Chessboard.Restart(opponent);
            RefreshPlayerDisplay();
        }

        private void ButtonRestart_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }

        private void ButtonMirror_Click(object sender, RoutedEventArgs e)
        {
            Chessboard.Mirror();
            isMirrored = !isMirrored;
            RefreshPlayerDisplay();
        }

        private async void ButtonJumpToStart_Click(object sender, RoutedEventArgs e)
        {
            await Chessboard.JumpToStartAsync();
        }

        private async void ButtonJumpToPreviousMove_Click(object sender, RoutedEventArgs e)
        {
            await Chessboard.ShowPreviousMoveAsync();
        }

        private async void ButtonJumpToNextMove_Click(object sender, RoutedEventArgs e)
        {
           await Chessboard.ShowNextMoveAsync();
        }

        private async void ButtonJumpToLastMove_Click(object sender, RoutedEventArgs e)
        {
         await   Chessboard.ShowLastMoveAsync();
        }

        private async void ListMoves_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int n = ListMoves.SelectedIndex;

            int firstMove = 2 * n - 1;
            int secondMove = 2 * n;

            // We are currently not able to differentae which one, so we always choose the first one
            await Chessboard.ShowMoveAsync(firstMove);
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

            static string formatPlayerElo(string? value)
            {
                if (string.IsNullOrEmpty(value))
                    return "Elo: ????";

                return $"Elo: {value}";
            }

            if (opponent is null)
            {
                if (isMirrored)
                {
                    // Top:    Player 1 (White)
                    // Bottom: Player 2 (Black)
                    TextPlayerTopName.Text = Helper.GetPlayerName(1);
                    TextPlayerTopElo.Text = formatPlayerElo(Settings.Instance.Player1Elo);

                    TextPlayerBottomName.Text = Helper.GetPlayerName(2);
                    TextPlayerBottomElo.Text = formatPlayerElo(Settings.Instance.Player2Elo);

                    TextPlayerInfoTop.Text = playerInfo.GetWhite();
                    TextPlayerInfoBottom.Text = playerInfo.GetBlack();
                }
                else
                {
                    // Top:     Player 2 (Black)
                    // Bottom:  Player 1 (White)
                    TextPlayerTopName.Text = Helper.GetPlayerName(2);
                    TextPlayerTopElo.Text = formatPlayerElo(Settings.Instance.Player2Elo);

                    TextPlayerBottomName.Text = Helper.GetPlayerName(1);
                    TextPlayerBottomElo.Text = formatPlayerElo(Settings.Instance.Player1Elo);

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
                    TextPlayerTopName.Text = Helper.GetPlayerName(1);
                    TextPlayerTopElo.Text = formatPlayerElo(Settings.Instance.Player1Elo);

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

                    TextPlayerBottomName.Text = Helper.GetPlayerName(1);
                    TextPlayerBottomElo.Text = formatPlayerElo(Settings.Instance.Player1Elo);

                    TextPlayerInfoTop.Text = playerInfo.GetBlack();
                    TextPlayerInfoBottom.Text = playerInfo.GetWhite();
                }
            }
        }

        #region Menu

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItemSettings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SettingsDialog() { Owner = this };
            dialog.ShowDialog();
        }

        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AboutDialog() { Owner = this };
            dialog.ShowDialog();
        }

        private void MenuNewGame_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }

        private void MenuMirrorBoard_Click(object sender, RoutedEventArgs e)
        {
            Chessboard.Mirror();
        }

        private async void MenuItemJumpToStart_Click(object sender, RoutedEventArgs e)
        {
            await Chessboard.JumpToStartAsync();
        }

        private async void MenuItemJumpToEnd_Click(object sender, RoutedEventArgs e)
        {
            await Chessboard.ShowLastMoveAsync();
        }

        #endregion
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