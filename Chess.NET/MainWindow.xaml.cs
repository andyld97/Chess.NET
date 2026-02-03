using Chess.NET.Controls;
using Chess.NET.Controls.Dialogs;
using Chess.NET.Model;
using Chess.NET.Netcode;
using Chess.NET.Shared.Model;
using Chess.NET.Shared.Model.Bot;
using Chess.NET.Shared.Model.Online;
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
            Chessboard.Game.OnGameOver += Game_OnGameOver;
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
            var puzzle = (sender as MenuItem)!.Tag as Puzzle;
            var puzzleDialog = new PuzzleDialog(puzzle);
            puzzleDialog.ShowDialog();
        }

        #endregion

        #region Game Events
        private async void Game_OnGameOver(GameResult result, Color? colorWon)
        {
            if (isOnlineMatch)
                return;

            await Task.Delay(250).ContinueWith(t =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    string playerName = string.Empty;
                    if (colorWon != null)
                    {
                        if (colorWon == Color.White)
                            playerName = Settings.Instance.Player1Name;
                        else
                        {
                            if (opponent is null)
                                playerName = Settings.Instance.Player2Name;
                            else
                                playerName = $"{opponent.Name} (Bot)";
                        }
                    }

                    GameOverDialog gameOverDialog = new GameOverDialog(result, colorWon, playerName) { Owner = this };
                    gameOverDialog.ShowDialog();
                });
            });
        }

        private void Game_OnPlaySound(SoundType type)
        {
            // Always play
            Sound.Play(type);
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

            // ... select +
            ListMoves.SelectedIndex = ListMoves.Items.Count - 1;

            // ... scroll into view
            if (ListMoves.SelectedItem != null)
                ListMoves.ScrollIntoView(ListMoves.SelectedItem);

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
                await StartOnlineMatchAsync();
            else
                opponent = null;

            Chessboard.Restart(opponent);
            RefreshPlayerDisplay();
        }

        #region Online Match / Net GUI Code

        private SignalRClient _networkClient = null!;
        private WaitingQueueDialog? waitingQueueDialog = null;
        private Color? ownPieceColor = null;
        private MatchInfo? currentMatchInfo = null;
        private Client? client = null;
        private bool isOnlineMatch = false;

        private async Task StartOnlineMatchAsync()
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
                // Error
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

            string playerWon = string.Empty;

            if (matchEnd.ColorWins.HasValue && matchEnd.ColorWins == ownPieceColor)
                playerWon = Settings.Instance.Player1Name;
            else if (matchEnd.ColorWins.HasValue)
                playerWon = currentMatchInfo?.OpponentName ?? string.Empty;

            currentMatchInfo = null;
            ButtonRestart.IsEnabled = true;
            isOnlineMatch = false;
            await _networkClient.DisconnectAsync();

            await Task.Delay(250).ContinueWith(t =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GameOverDialog gameOverDialog = new GameOverDialog(matchEnd.Result, matchEnd.ColorWins, playerWon) { Owner = this };
                    gameOverDialog.ShowDialog();
                });
            });
        }

        private async void Chessboard_OnMoveMadeOnline(MoveNotation moveNotation)
        {
            if (_networkClient == null || currentMatchInfo == null)
                return;

            var result = await _networkClient.MakeMoveAsync(currentMatchInfo.MatchId, moveNotation.FormatMove(false, false));

            if (!result)
            {
                // TODO: Wenn Move vom Server nicht akzeptiert wurde, ihn wieder lokal rückgängig machen!

            }
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
                MessageBox.Show(string.Format(Properties.Resources.strFailedToResign, ex.Message), Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
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

            string playerTopName = string.Empty;
            string playerTopElo = string.Empty;
            string playerBottomName = string.Empty;
            string playerBottomElo = string.Empty;

            if (isOnlineMatch)
            {
                if (!Chessboard.IsMirrored)
                {
                    if (ownPieceColor == Color.White)
                    {
                        playerTopName = currentMatchInfo?.OpponentName ?? string.Empty;
                        playerTopElo = formatPlayerElo(currentMatchInfo?.OpponentElo ?? string.Empty);

                        playerBottomName = Helper.GetPlayerName(1);
                        playerBottomElo = formatPlayerElo(Settings.Instance.Player1Elo);
                    }
                    else
                    {
                        playerBottomName = currentMatchInfo?.OpponentName ?? string.Empty;
                        playerBottomElo = formatPlayerElo(currentMatchInfo?.OpponentElo ?? string.Empty);

                        playerTopName = Helper.GetPlayerName(1);
                        playerTopElo = formatPlayerElo(Settings.Instance.Player1Elo);
                    }
                }
                else
                {
                    if (ownPieceColor == Color.White)
                    {
                        playerTopName = Helper.GetPlayerName(1);
                        playerTopElo = formatPlayerElo(Settings.Instance.Player1Elo);

                        playerBottomName = currentMatchInfo?.OpponentName ?? string.Empty;
                        playerBottomElo = formatPlayerElo(currentMatchInfo?.OpponentElo ?? string.Empty);
                    }
                    else
                    {
                        playerBottomName = Helper.GetPlayerName(1);
                        playerBottomElo = formatPlayerElo(Settings.Instance.Player1Elo);

                        playerTopName = currentMatchInfo?.OpponentName ?? string.Empty;
                        playerTopElo = formatPlayerElo(currentMatchInfo?.OpponentElo ?? string.Empty);
                    }
                }
            }
            else if (opponent is not null)
            {
                // Bot
                if (!Chessboard.IsMirrored)
                {
                    playerTopName = $"{opponent.Name} (Bot)";               
                    playerTopElo = formatElo(opponent.Elo);

                    playerBottomName = Helper.GetPlayerName(1);
                    playerBottomElo = formatPlayerElo(Settings.Instance.Player1Elo);
                }
                else
                {
                    playerTopName = Helper.GetPlayerName(1);
                    playerTopElo = Settings.Instance.Player1Elo;

                    playerBottomName = $"{opponent.Name} (Bot)";
                    playerBottomElo = formatElo(opponent.Elo);
                }
            }
            else
            {
                // Player 2
                if (!Chessboard.IsMirrored)
                {
                    playerTopName = Helper.GetPlayerName(2);
                    playerTopElo = Settings.Instance.Player2Elo;

                    playerBottomName = Helper.GetPlayerName(1);
                    playerBottomElo = formatPlayerElo(Settings.Instance.Player1Elo);
                }
                else
                {
                    playerTopName = Helper.GetPlayerName(1);
                    playerTopElo = Settings.Instance.Player1Elo;

                    playerBottomName = Helper.GetPlayerName(2);
                    playerBottomElo = formatPlayerElo(Settings.Instance.Player2Elo);
                }
            }

            TextPlayerTopName.Text = playerTopName;
            TextPlayerTopElo.Text = playerTopElo;
            TextPlayerBottomName.Text = playerBottomName;
            TextPlayerBottomElo.Text = playerBottomElo;

            if (Chessboard.IsMirrored)
            {
                TextPlayerInfoTop.Text = playerInfo.GetBlack();
                TextPlayerInfoBottom.Text = playerInfo.GetWhite();
            }
            else
            {
                TextPlayerInfoTop.Text = playerInfo.GetBlack();
                TextPlayerInfoBottom.Text = playerInfo.GetWhite();          
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