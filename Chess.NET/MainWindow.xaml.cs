using Chess.NET.Controls;
using Chess.NET.Controls.Dialogs;
using Chess.NET.Model;
using Chess.NET.Shared;
using Chess.NET.Shared.Model;
using Chess.NET.Shared.Model.Bot;
using Chess.NET.Shared.Model.Online;
using Chess.NET.Shared.Netcode;
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
        private readonly bool isInitialized = false; 
        public static MainWindow W_INSTANCE { get; private set; } = null!;    

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
            W_INSTANCE = this;
            imgBackground.Source = Helper.GetBackground(Settings.Instance.Background);

#if STORE
            Title = "Open Chess";
#endif

            // Assign events
            Chessboard.Game.OnMovedPiece += Game_MovedPiece;
            Chessboard.Game.OnGameOver += Game_OnGameOver;
            Chessboard.Game.OnPlaySound += Game_OnPlaySound;
            Chessboard.Game.OnUpdatePlayerTime += Game_OnUpdatePlayerTime;

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

            if (Settings.Instance.LastSelectedGameMode >= 0 && Settings.Instance.LastSelectedGameMode < CmbOpponent.Items.Count)
                CmbOpponent.SelectedIndex = Settings.Instance.LastSelectedGameMode;

            isInitialized = true;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Instance.IsOOBE)
            {
                var dialog = new SettingsDialog() { Owner = this };
                dialog.ShowDialog();
            }
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

        private void Game_OnUpdatePlayerTime(TimeSpan playerWhiteTime, TimeSpan playerBlackTime)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                if (Chessboard.IsMirrored)
                {
                    TextPlayerTopTimeControl.Text = playerWhiteTime.ToString(@"mm\:ss");
                    TextPlayerBottomTimeControl.Text = playerBlackTime.ToString(@"mm\:ss");
                }
                else
                {
                    TextPlayerTopTimeControl.Text = playerBlackTime.ToString(@"mm\:ss");
                    TextPlayerBottomTimeControl.Text = playerWhiteTime.ToString(@"mm\:ss");
                }

                UpdatePlayerTurnColor();
            });
        }

        private void UpdatePlayerTurnColor()
        {
            if (Chessboard.Game.PlayersTurn == Color.White)
            {
                if (Chessboard.IsMirrored)
                {
                    // White is on top
                    BorderTimeControlTop.Background = (System.Windows.Media.SolidColorBrush)FindResource("PlayerTimeColor");
                    BorderTimeControlBottom.Background = (System.Windows.Media.SolidColorBrush)FindResource("PlayerTimeDefaultColor");
                }
                else
                {
                    BorderTimeControlTop.Background = (System.Windows.Media.SolidColorBrush)FindResource("PlayerTimeDefaultColor");
                    BorderTimeControlBottom.Background = (System.Windows.Media.SolidColorBrush)FindResource("PlayerTimeColor");
                }
            }
            else
            {
                if (Chessboard.IsMirrored)
                {
                    // White is on top
                    BorderTimeControlBottom.Background = (System.Windows.Media.SolidColorBrush)FindResource("PlayerTimeColor");
                    BorderTimeControlTop.Background = (System.Windows.Media.SolidColorBrush)FindResource("PlayerTimeDefaultColor");
                }
                else
                {
                    BorderTimeControlBottom.Background = (System.Windows.Media.SolidColorBrush)FindResource("PlayerTimeDefaultColor");
                    BorderTimeControlTop.Background = (System.Windows.Media.SolidColorBrush)FindResource("PlayerTimeColor");
                }
            }
        }

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

            if (rotatedBoard)
            {
                rotatedBoard = false;
                Chessboard.Mirror();
            }

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
        private bool rotatedBoard = false;

        private async Task StartOnlineMatchAsync()
        {
            if (string.IsNullOrEmpty(Settings.Instance.Player1Name))
            {
                MessageBox.Show(Properties.Resources.strPleaseSetAName, Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error); 
                return;
            }

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
            try
            {
                client = await _networkClient.ConnectAsync(Settings.Instance.Player1Name, Settings.Instance.Player1Elo);
                waitingQueueDialog?.Client = client;

                if (client == null)
                {
                    // Error
                    waitingQueueDialog?.FoundMatch = false;
                    waitingQueueDialog?.Loaded -= WaitingQueueDialog_Loaded;
                    waitingQueueDialog?.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.strFailedToConnectToServer, ex.Message), Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void NetworkClient_OnMatchFound(MatchInfo match)
        {
            await App.UiDispatcher.Invoke(async () =>
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
                    {
                        Chessboard.Mirror();
                        rotatedBoard = true;
                    }

                    ownPieceColor = Color.Black;
                }
                else
                {
                    if (Chessboard.IsMirrored)
                    {
                        Chessboard.Mirror();
                        rotatedBoard = true;
                    }

                    ownPieceColor = Color.White;
                }

                currentMatchInfo = match;
                Chessboard.Game.StartNewGame(null, true);
                Chessboard.SetOnline(ownPieceColor.Value);
                RefreshPlayerDisplay();
            });
        }

        private async void NetworkClient_OnMoveMade(MoveMade moveMade)
        {
            await App.UiDispatcher.Invoke(async () =>
            {
                var pendingMove = PendingMove.Parse(moveMade.Move, (Board)Chessboard.Game.Board, Chessboard.Game, moveMade.Color);
                if (moveMade.Color != ownPieceColor)
                {
                    Chessboard.Game.Move(pendingMove, true);
                    Chessboard.RenderChessBoard(Chessboard.Game.Board, true);
                }
            });
        }

        private async void NetworkClient_OnMatchEnds(MatchEnd matchEnd)
        {
            await App.UiDispatcher.Invoke(async () =>
            {
                Chessboard.DisablePieces();
                Chessboard.ResetOnline();
                ButtonResign.Visibility = Visibility.Collapsed;

                string playerWon = string.Empty;

                if (matchEnd.ColorWins.HasValue && matchEnd.ColorWins == ownPieceColor)
                    playerWon = Settings.Instance.Player1Name;
                else if (matchEnd.ColorWins.HasValue)
                    playerWon = currentMatchInfo?.OpponentName ?? string.Empty;

                currentMatchInfo = null;
                ButtonRestart.IsEnabled = true;               
                await _networkClient.DisconnectAsync();

                await Task.Delay(50).ContinueWith(t =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Only consider sounds that are normally not played using the Game-Class
                        if (matchEnd.Result == GameResult.Disconnected || matchEnd.Result == GameResult.Resign || matchEnd.Result == GameResult.Timeout)
                            Sound.Play(SoundType.Checkmate);

                        GameOverDialog gameOverDialog = new GameOverDialog(matchEnd.Result, matchEnd.ColorWins, playerWon) { Owner = this };
                        gameOverDialog.ShowDialog();
                        isOnlineMatch = false;
                    });
                });
            });
        }

        private async void Chessboard_OnMoveMadeOnline(MoveNotation moveNotation)
        {
            if (_networkClient == null || currentMatchInfo == null)
                return;

            try
            {
                await APIClient.MakeMoveAsync(currentMatchInfo.MatchId, moveNotation.FormatMove(false, false));               
            }
            catch (Exception ex)
            {            
                // TODO: Wenn Move vom Server nicht akzeptiert wurde, ihn wieder lokal rückgängig machen!
             
                MessageBox.Show(string.Format(Properties.Resources.strFailedToMove, ex.Message), Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
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
            var playerInfo = Chessboard.Game.GetPlayerInformation();
            string ownName = Helper.GetPlayerName(1);
            string ownElo = PlayerDisplayHelper.FormatElo(Settings.Instance.Player1Elo);

            string otherName, otherElo;
            if (isOnlineMatch)
            {
                otherName = currentMatchInfo?.OpponentName ?? string.Empty;
                otherElo = PlayerDisplayHelper.FormatElo(currentMatchInfo?.OpponentElo);
            }
            else if (opponent is not null)
            {
                otherName = $"{opponent.Name} (Bot)";
                otherElo = PlayerDisplayHelper.FormatElo(opponent.Elo);
            }
            else
            {
                otherName = Helper.GetPlayerName(2);
                otherElo = PlayerDisplayHelper.FormatElo(Settings.Instance.Player2Elo);
            }

            var d = PlayerDisplayHelper.GetPlayerDisplay(ownName, ownElo, otherName, otherElo, Chessboard.IsMirrored, isOnlineMatch && ownPieceColor == Shared.Model.Color.Black, playerInfo.GetWhite(), playerInfo.GetBlack(), Chessboard.Game.PlayerWhiteTime, Chessboard.Game.PlayerBlackTime);

            TextPlayerTopName.Text = d.TopName;
            TextPlayerTopElo.Text = d.TopElo;
            TextPlayerInfoTop.Text = d.TopInfo;
            TextPlayerBottomName.Text = d.BottomName;
            TextPlayerBottomElo.Text = d.BottomElo;
            TextPlayerInfoBottom.Text = d.BottomInfo;
            TextPlayerTopTimeControl.Text = d.TopTime.ToString(@"mm\:ss");
            TextPlayerBottomTimeControl.Text = d.TimeBottom.ToString(@"mm\:ss");

            UpdatePlayerTurnColor();
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

        public void Refresh()
        {
            imgBackground.Source = Helper.GetBackground(Settings.Instance.Background);
            Chessboard.RenderChessBoard(Chessboard.Game.Board, true);
        }

        private void CmbOpponent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInitialized)
                return;

            Settings.Instance.LastSelectedGameMode = CmbOpponent.SelectedIndex;
            Settings.Instance.Save();
        }

        #endregion
    }
}