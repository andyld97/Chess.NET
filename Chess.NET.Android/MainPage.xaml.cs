using Chess.NET.Android.Controls.Dialogs;
using Chess.NET.Shared.Model;
using Chess.NET.Shared.Model.Bot;
using Chess.NET.Shared.Model.Online;
using Chess.NET.Shared.Netcode;

#if ANDROID
using A = Android;
#endif

namespace Chess.NET.Android
{
    public partial class MainPage : ContentPage
    {
        private IChessBot? opponent = null;

        public MainPage()
        {
            InitializeComponent();
            RefreshPlayerDisplay();

            Chessboard.Game.OnMovedPiece += Game_OnMovedPiece;
        }

        private void Game_OnMovedPiece(MoveNotation move)
        {
            RefreshPlayerDisplay();
        }

        #region Online Match
        private Client? client = null;
        private SignalRClient _networkClient = null!; 
        private Shared.Model.Color? ownPieceColor = null;
        private MatchInfo? currentMatchInfo = null;
        private bool isOnlineMatch = false;

        private WaitingQueueDialog waitingQueueDialog;
     
        private async Task StartOnlineMatchAsync()
        {
            if (string.IsNullOrEmpty(Settings.Instance.Player1Name))
            {
                // TODO
                // MessageBox.Show(Properties.Resources.strPleaseSetAName, Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ownPieceColor = null;
            currentMatchInfo = null;  

            _networkClient = new SignalRClient();
            _networkClient.OnMatchFound += _networkClient_OnMatchFound;
            _networkClient.OnMoveMade += _networkClient_OnMoveMade;
            _networkClient.OnMatchEnds += _networkClient_OnMatchEnds;

            try
            {
                waitingQueueDialog = new WaitingQueueDialog();
                waitingQueueDialog.OnWaitingQueueExited += WaitingQueueDialog_OnWaitingQueueExited;
                await Navigation.PushModalAsync(waitingQueueDialog);
                client = await _networkClient.ConnectAsync($"{Settings.Instance.Player1Name} (Android)", Settings.Instance.Player1Elo);
 
                if (client == null)
                {
                    // Error
                    await Navigation.PopModalAsync();
                }
            }
            catch (Exception ex)
            {
                // TODO
                // MessageBox.Show(string.Format(Properties.Resources.strFailedToConnectToServer, ex.Message), Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void WaitingQueueDialog_OnWaitingQueueExited()
        {
            try
            {
                if (client == null)
                    return;

                await APIClient.LeaveQueueAsync(client);
            }
            catch
            { }
        }

        private async void _networkClient_OnMatchEnds(MatchEnd matchEnd)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // TODO Chessboard.DisablePieces();
                ButtonResign.IsVisible = false;

                string playerWon = string.Empty;

                if (matchEnd.ColorWins.HasValue && matchEnd.ColorWins == ownPieceColor)
                    playerWon = Settings.Instance.Player1Name;
                else if (matchEnd.ColorWins.HasValue)
                    playerWon = currentMatchInfo?.OpponentName ?? string.Empty;

                currentMatchInfo = null;
                ButtonRestart.IsEnabled = true;
                isOnlineMatch = false;
                await _networkClient.DisconnectAsync();

               await Task.Delay(50).ContinueWith(t =>
               {
                   MainThread.BeginInvokeOnMainThread(async () =>
                   {
                       // Only consider sounds that are normally not played using the Game-Class
                       if (matchEnd.Result == GameResult.Disconnected || matchEnd.Result == GameResult.Resign || matchEnd.Result == GameResult.Timeout)
                          await Sound.Play(SoundType.Checkmate);

#if ANDROID
                       A.Widget.Toast.MakeText(A.App.Application.Context, $"Game Over: {matchEnd.Result}. Won: {(matchEnd.ColorWins == null ? "-" : matchEnd.ColorWins.ToString())}", A.Widget.ToastLength.Long).Show();
#endif

                       // TODO
                       /*GameOverDialog gameOverDialog = new GameOverDialog(matchEnd.Result, matchEnd.ColorWins, playerWon) { Owner = this };
                       gameOverDialog.ShowDialog();*/
                   });
               });

            });
        }

        private void _networkClient_OnMoveMade(MoveMade moveMade)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var pendingMove = PendingMove.Parse(moveMade.Move, (Board)Chessboard.Game.Board, Chessboard.Game, moveMade.Color);
                if (moveMade.Color != ownPieceColor)
                {
                    Chessboard.Game.Move(pendingMove, true);
                    Chessboard.RenderChessBoard(Chessboard.Game.Board, true);
                }
            });
        }

        private void _networkClient_OnMatchFound(MatchInfo match)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {                
                ButtonRestart.IsEnabled = false;
                ButtonResign.IsVisible = true;
                isOnlineMatch = true;
                await Navigation.PopModalAsync();

                if (match.ClientColor == Chess.NET.Shared.Model.Color.Black)
                {
                    if (!Chessboard.IsMirrored)
                        Chessboard.Mirror();

                    ownPieceColor = Shared.Model.Color.Black;
                }
                else
                {
                    if (Chessboard.IsMirrored)
                        Chessboard.Mirror();

                    ownPieceColor = Shared.Model.Color.White;
                }

                currentMatchInfo = match;
                Chessboard.Game.StartNewGame(null);
                Chessboard.RenderChessBoard(Chessboard.Game.Board, false);
                Chessboard.SetOnline(ownPieceColor.Value);
                RefreshPlayerDisplay();
            });
        }

        #endregion

        private async void ButtonRestart_Clicked(object sender, EventArgs e)
        {
            var dialog = new NewGameDialog();
            await Navigation.PushModalAsync(dialog);

            var result = await dialog.WaitForResultAsync();

            if (result == 0)
            {
                opponent = null;
                await StartOnlineMatchAsync();
            }
            else
            {
                if (result == 1)
                {
                    opponent = new StupidoBot();
                    Chessboard.Restart(opponent);
                }
                else
                {
                    opponent = null;
                    Chessboard.Restart(null);
                }
            }

            RefreshPlayerDisplay();
        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            Chessboard.Mirror();
            RefreshPlayerDisplay();
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

                //MessageBox.Show(string.Format(Properties.Resources.strFailedToMove, ex.Message), Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshPlayerDisplay()
        {
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
                    if (ownPieceColor == Shared.Model.Color.White)
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
                    if (ownPieceColor == Shared.Model.Color.White)
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
                    playerTopElo = formatPlayerElo(Settings.Instance.Player1Elo);

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
                    playerTopElo = formatPlayerElo(Settings.Instance.Player2Elo);

                    playerBottomName = Helper.GetPlayerName(1);
                    playerBottomElo = formatPlayerElo(Settings.Instance.Player1Elo);
                }
                else
                {
                    playerTopName = Helper.GetPlayerName(1);
                    playerTopElo = formatPlayerElo(Settings.Instance.Player1Elo);

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
                TextPlayerInfoTop.Text = playerInfo.GetWhite();
                TextPlayerInfoBottom.Text = playerInfo.GetBlack(); 
            }
            else
            {
                TextPlayerInfoTop.Text = playerInfo.GetBlack();
                TextPlayerInfoBottom.Text = playerInfo.GetWhite();
            }
        }

        private async void ButtonResign_Clicked(object sender, EventArgs e)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(currentMatchInfo);
                await APIClient.ResignAsync(currentMatchInfo.MatchId);
            }
            catch (Exception ex)
            {
                // TODO MessageBox.Show(string.Format(Properties.Resources.strFailedToResign, ex.Message), Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MenuItemSettings_Clicked(object sender, EventArgs e)
        {
            var dialog = new SettingsDialog();
            await Navigation.PushModalAsync(dialog);
        }
    }
}
