using Chess.NET.Controls;
using Chess.NET.Controls.Dialogs;
using Chess.NET.Model;
using Chess.NET.Netcode;
using Chess.NET.Shared.Model;
using Chess.NET.Shared.Model.Bot;
using Chess.NET.Shared.Model.Online;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private MoveNotationDisplay currentMoveNotationDisplay = null!;
        private IChessBot? opponent = null;

        #region Commands

        public ICommand MoveLeftCommand { get; }

        public ICommand MoveRightCommand { get; }

        public ICommand NewGameCommand { get; }

        public ICommand MirrorBoardCommand { get; }

        #endregion

        // TODO: Highlight welcher Spieler gerade dran ist!

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
            NewGameCommand = new RelayCommand(async() => await StartNewGameAsync());
            MirrorBoardCommand = new RelayCommand(new Action(Chessboard.Mirror));
            DataContext = this;

            // Add start move (always in list)
            MoveNotationDisplay start = new MoveNotationDisplay(true);
            start.OnJumpToMove += OnJumpToMove;
            ListMoves.Items.Add(start);

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
            new PuzzleDialog((sender as MenuItem)!.Tag as Puzzle).ShowDialog();
        }

        #endregion

        #region Game Events

        private async void Game_OnCheckmate(Color pieceColor)
        {
            if (isOnlineMatch)
                return;

            string playerText = string.Empty;

            if (pieceColor == Color.White)
            {
                if (opponent == null)
                    playerText = $"{Helper.GetPlayerName(1)} ({Properties.Resources.strBlack})";
                else
                    playerText = $"{opponent?.Name} [Bot] ({Properties.Resources.strBlack})";
            }
            else if (pieceColor == Color.Black)
                playerText = $"{Helper.GetPlayerName(1)} ({Properties.Resources.strWhite})";

            await Task.Delay(250).ContinueWith(t =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(string.Format(Properties.Resources.strGameOver_WinMessage, playerText), Properties.Resources.strGameOver_Win, MessageBoxButton.OK, MessageBoxImage.Information);
                });
            });
        }

        private void Game_OnPlaySound(SoundType type)
        {
            // Always play
            Sound.Play(type);
        }

        private async void Game_OnStalemate()
        {
            if (isOnlineMatch)
                return;

            await Task.Delay(250).ContinueWith(t =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(Properties.Resources.strGameOver_StalemateText, Properties.Resources.strGameOver_Stalemate, MessageBoxButton.OK, MessageBoxImage.Information);
                });
            });
        }

        private void Game_MovedPiece(MoveNotation move)
        {
            if (currentMoveNotationDisplay == null)
            {
                currentMoveNotationDisplay = new MoveNotationDisplay();
                currentMoveNotationDisplay.OnJumpToMove += OnJumpToMove;
            }

            if (move.Piece.Color == Color.White)
            {
                currentMoveNotationDisplay.Move1 = move;
                ListMoves.Items.Add(currentMoveNotationDisplay);
            }
            else
            {
                currentMoveNotationDisplay.Move2 = move;
                currentMoveNotationDisplay = new MoveNotationDisplay();
                currentMoveNotationDisplay.OnJumpToMove += OnJumpToMove;
            }

            ListMoves.SelectedIndex = ListMoves.Items.Count - 1;
            RefreshPlayerDisplay();
        }

        private async void OnJumpToMove(int index)
        {
            if (index == -1)
            {
                await Chessboard.JumpToStartAsync();
                return;
            }

            await Chessboard.ShowMoveAsync(index);
        }

        #endregion

        private async Task StartNewGameAsync()
        {
            if (isOnlineMatch)
                return;

            // First item should always stay in list
            for (int i = ListMoves.Items.Count - 1; i >= 1; i--)
                ListMoves.Items.RemoveAt(i);

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
            else if (CmbOpponent.SelectedIndex == 2)
                await StartNewOnlineMatch();
            else
                opponent = null;

            Chessboard.Restart(opponent);
            RefreshPlayerDisplay();
        }

        #region Online Match / Net GUI Code

        private SignalRClient _networkClient;
        private WaitingQueueDialog? waitingQueueDialog = null;
        private Color? ownPieceColor = null;
        private MatchInfo? currentMatchInfo = null;
        private Client? client = null;
        private bool isOnlineMatch = false;

        private async Task StartNewOnlineMatch()
        {
            ownPieceColor = null;
            currentMatchInfo = null;
            waitingQueueDialog = null;

            _networkClient = new SignalRClient();
            _networkClient.OnMatchFound += NetworkClient_OnMatchFound;
            _networkClient.OnMoveMade += NetworkClient_OnMoveMade;
            _networkClient.OnMatchEnds += NetworkClient_OnMatchEnds;

            waitingQueueDialog = new WaitingQueueDialog() { Owner = this };
            waitingQueueDialog.Loaded += WaitingQueueDialog_Loaded;
            waitingQueueDialog.ShowDialog();  
        }

        private async void WaitingQueueDialog_Loaded(object sender, RoutedEventArgs e)
        {
            client = await _networkClient.ConnectAsync(Chessboard);
            waitingQueueDialog?.Client = client;

            if (client == null)
            {
                // = Error
                waitingQueueDialog?.FoundMatch = false;
                waitingQueueDialog?.Loaded -= WaitingQueueDialog_Loaded;
                waitingQueueDialog?.Close();
            }
        }

        private void NetworkClient_OnMatchFound(MatchInfo match)
        {
            ButtonRestart.IsEnabled = false;
            ButtonResign.Visibility = Visibility.Visible;
            isOnlineMatch = true;
            waitingQueueDialog?.FoundMatch = true; 
            waitingQueueDialog?.Loaded -= WaitingQueueDialog_Loaded;
            waitingQueueDialog?.Close();
            waitingQueueDialog = null;

            if (match.ClientColor == Color.Black)
            {
                if (!Chessboard.IsMirrored)
                    Chessboard.Mirror();

                ownPieceColor = Color.Black;
            }
            else
            {
                if (Chessboard.IsMirrored)
                    Chessboard.Mirror();

                ownPieceColor = Color.White;
            }

            currentMatchInfo = match;
            Chessboard.Game.StartNewGame(null);
            Chessboard.SetOnline(ownPieceColor.Value);
            RefreshPlayerDisplay();
        }

        private async void NetworkClient_OnMoveMade(MoveMade moveMade)
        {
            var pendingMove = PendingMove.Parse(moveMade.Move, (Board)Chessboard.Game.Board, moveMade.Color);
            if (moveMade.Color != ownPieceColor)
            {
                await Chessboard.Game.MoveAsync(pendingMove, true);
                Chessboard.RenderChessBoard(Chessboard.Game.Board, true);
            }
        }

        private async void NetworkClient_OnMatchEnds(MatchEnd matchEnd)
        {
            Chessboard.DisablePieces();
            ButtonResign.Visibility = Visibility.Collapsed;

            string? playeOpponentName = currentMatchInfo?.OpponentName;

            currentMatchInfo = null;
            ButtonRestart.IsEnabled = true;
            isOnlineMatch = false;
            await _networkClient.DisconnectAsync();

            string message = $"Match ended: ";
            if (matchEnd.Result == MatchResult.Stalemate)
                message += "Stalemate (Remis)";
            else
            {
                string playerText = string.Empty;
                if (matchEnd.ColorWins == ownPieceColor)
                    playerText = "You won";
                else
                    playerText = $"You lost! Your opponennt {playeOpponentName} won";

                message += $"{playerText} due to {matchEnd.Result}!";
            }

            MessageBox.Show(message, "Game is over!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void Chessboard_OnMoveMadeOnline(MoveNotation moveNotation)
        {
            if (_networkClient == null || currentMatchInfo == null)
                return;

            await _networkClient.MakeMoveAsync(currentMatchInfo.MatchId, moveNotation.FormatMove(false, false));
        }

        private async void ButtonResign_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(currentMatchInfo);
                await APIClient.ResignAsync(currentMatchInfo.MatchId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to resign: {ex.Message}", Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        private async void ButtonRestart_Click(object sender, RoutedEventArgs e)
        {
            await StartNewGameAsync();
        }

        private void ButtonMirror_Click(object sender, RoutedEventArgs e)
        {
            Chessboard.Mirror();
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
            await Chessboard.ShowLastMoveAsync();
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
                if (Chessboard.IsMirrored)
                {
                    // Top:    Player 1 (White)
                    // Bottom: Player 2 (Black)
                    if (isOnlineMatch && currentMatchInfo != null)
                    {
                        TextPlayerTopName.Text = currentMatchInfo.OpponentName;
                        TextPlayerTopElo.Text = formatPlayerElo(currentMatchInfo.OpponentElo);
                    }
                    else
                    {
                        TextPlayerTopName.Text = Helper.GetPlayerName(1);
                        TextPlayerTopElo.Text = formatPlayerElo(Settings.Instance.Player1Elo);
                    }

                    TextPlayerBottomName.Text = Helper.GetPlayerName(2);
                    TextPlayerBottomElo.Text = formatPlayerElo(Settings.Instance.Player2Elo);

                    TextPlayerInfoTop.Text = playerInfo.GetWhite();
                    TextPlayerInfoBottom.Text = playerInfo.GetBlack();
                }
                else
                {
                    // Top:     Player 2 (Black)
                    // Bottom:  Player 1 (White)
                    if (isOnlineMatch && currentMatchInfo != null)
                    {
                        TextPlayerTopName.Text = currentMatchInfo.OpponentName;
                        TextPlayerTopElo.Text = formatPlayerElo(currentMatchInfo.OpponentElo);
                    }
                    else
                    {
                        TextPlayerTopName.Text = Helper.GetPlayerName(2);
                        TextPlayerTopElo.Text = formatPlayerElo(Settings.Instance.Player2Elo);
                    }

                    TextPlayerBottomName.Text = Helper.GetPlayerName(1);
                    TextPlayerBottomElo.Text = formatPlayerElo(Settings.Instance.Player1Elo);

                    TextPlayerInfoTop.Text = playerInfo.GetBlack();
                    TextPlayerInfoBottom.Text = playerInfo.GetWhite();
                }
            }
            else
            {
                if (Chessboard.IsMirrored)
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

        private async void MenuNewGame_Click(object sender, RoutedEventArgs e)
        {
            await StartNewGameAsync();
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
}