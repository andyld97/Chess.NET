using Chess.NET.Android.Controls.Dialogs;
using Chess.NET.Shared;
using Chess.NET.Shared.Model;
using Chess.NET.Shared.Model.Bot;
using Chess.NET.Shared.Model.Online;
using Chess.NET.Shared.Netcode;
using Microsoft.AspNetCore.SignalR.Client;

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
            Chessboard.Game.OnUpdatePlayerTime += Game_OnUpdatePlayerTime;
#if ANDROID
            MainActivity.Paused += MainActivity_Paused;
#endif
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            DeviceDisplay.Current.KeepScreenOn = true;
        }

        private async void MainActivity_Paused()
        {
            if (isOnlineMatch && _networkClient != null)
            {
                try
                {
                    Chessboard.Reset();
                    await _networkClient.DisconnectAsync();
                }
                catch
                {

                }
                finally
                {
                    ButtonRestart.IsEnabled = true;
                    ButtonResign.IsVisible = false;
                    currentMatchInfo = null;
                    client = null;
                    ownPieceColor = null;
                    _networkClient = null;
                    isOnlineMatch = false;
                }
            }
        }

        private void Game_OnUpdatePlayerTime(TimeSpan playerWhiteTime, TimeSpan playerBlackTime)
        {
            MainThread.BeginInvokeOnMainThread(() =>            
            {
                BorderTimeControlTop.IsVisible = true;
                BorderTimeControlBottom.IsVisible = true;

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
            var whiteToMove = Chessboard.Game.PlayersTurn == Shared.Model.Color.White;
            var topActive = whiteToMove == Chessboard.IsMirrored;

            BorderTimeControlTop.SetDynamicResource(VisualElement.BackgroundProperty, topActive ? "PlayerTimeColor" : "PlayerTimeDefaultColor");
            BorderTimeControlBottom.SetDynamicResource(VisualElement.BackgroundProperty, topActive ? "PlayerTimeDefaultColor" : "PlayerTimeColor");
        }

        private void Game_OnMovedPiece(MoveNotation move)
        {
            RefreshPlayerDisplay();
        }

        #region Online Match
        private Client? client = null;
        private SignalRClient? _networkClient = null!; 
        private Shared.Model.Color? ownPieceColor = null;
        private MatchInfo? currentMatchInfo = null;
        private bool isOnlineMatch = false;
        private bool rotatedBoard = false;
        private WaitingQueueDialog? waitingQueueDialog = null;
     
        private async Task StartOnlineMatchAsync(TimeControl timeControl)
        {
            if (string.IsNullOrEmpty(Settings.Instance.Player1Name) || Settings.Instance.Player1Name == Properties.Resources.strPlayer1)
            {
                await DialogHelper.ShowErrorAsync(Shared.Resources.strPleaseSetAName, Shared.Resources.strError);
                return;
            }

            ownPieceColor = null;
            currentMatchInfo = null;

            _networkClient = new SignalRClient();
            _networkClient.OnMatchFound += _networkClient_OnMatchFound;
            _networkClient.OnMoveMade += _networkClient_OnMoveMade;
            _networkClient.OnMatchEnds += _networkClient_OnMatchEnds;
            _networkClient.OnMatchTimeUpdate += _networkClient_OnMatchTimeUpdate;

            try
            {
                waitingQueueDialog = new WaitingQueueDialog();
                waitingQueueDialog.OnWaitingQueueExited += WaitingQueueDialog_OnWaitingQueueExited;
                await Navigation.PushModalAsync(waitingQueueDialog);
                client = await _networkClient.ConnectAsync($"{Settings.Instance.Player1Name} (Android)", Settings.Instance.Player1Elo, timeControl);
 
                if (client == null)
                {
                    // Error
                    await Navigation.PopModalAsync();
                }
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowErrorAsync(string.Format(Shared.Resources.strFailedToConnectToServer, ex.Message), Shared.Resources.strError);
            }
        }

        private void _networkClient_OnMatchTimeUpdate(MatchTimeUpdate matchTimeUpdate)
        {
            Chessboard.Game.PlayerWhiteTime = matchTimeUpdate.PlayerWhiteTime;
            Chessboard.Game.PlayerBlackTime = matchTimeUpdate.PlayerBlackTime;

            Game_OnUpdatePlayerTime(matchTimeUpdate.PlayerWhiteTime, matchTimeUpdate.PlayerBlackTime);
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
                Chessboard.DisablePieces();
                ButtonResign.IsVisible = false;

                string playerWon = string.Empty;

                if (matchEnd.ColorWins.HasValue && matchEnd.ColorWins == ownPieceColor)
                    playerWon = Settings.Instance.Player1Name;
                else if (matchEnd.ColorWins.HasValue)
                    playerWon = currentMatchInfo?.OpponentName ?? string.Empty;

                currentMatchInfo = null;
                ButtonRestart.IsEnabled = true;
                Chessboard.ResetOnline();
                if (_networkClient != null)
                    await _networkClient.DisconnectAsync();

                await Task.Delay(50).ContinueWith(t =>
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        // Only consider sounds that are normally not played using the Game-Class
                        if (matchEnd.Result == GameResult.Disconnected || matchEnd.Result == GameResult.Resign || matchEnd.Result == GameResult.Timeout)
                            await Sound.Play(SoundType.Checkmate);

                        string playerWon = string.Empty;

                        if (matchEnd.ColorWins.HasValue && matchEnd.ColorWins == ownPieceColor)
                            playerWon = Settings.Instance.Player1Name;
                        else if (matchEnd.ColorWins.HasValue)
                            playerWon = currentMatchInfo?.OpponentName ?? string.Empty;

                        var dialog = new GameOverDialog(matchEnd.Result, matchEnd.ColorWins, playerWon);
                        isOnlineMatch = false;
                        await Navigation.PushModalAsync(dialog);
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
                    {
                        Chessboard.Mirror();
                        rotatedBoard = true;
                    }

                    ownPieceColor = Shared.Model.Color.Black;
                }
                else
                {
                    if (Chessboard.IsMirrored)
                    {
                        Chessboard.Mirror();
                        rotatedBoard = true;
                    }

                    ownPieceColor = Shared.Model.Color.White;
                }

                currentMatchInfo = match;
                Chessboard.Game.StartNewGame(null, true, match.TimeControl);
                Chessboard.RenderChessBoard(Chessboard.Game.Board, false);
                Chessboard.SetOnline(ownPieceColor.Value);
                Chessboard.EnablePieces();
                RefreshPlayerDisplay();
            });
        }

        #endregion

        private async void ButtonRestart_Clicked(object sender, EventArgs e)
        {
            if (isOnlineMatch)
                return;

            var dialog = new NewGameDialog();
            await Navigation.PushModalAsync(dialog);

            var result = await dialog.WaitForResultAsync();
            var tc = (Shared.Model.TimeControl)result.Item2;

            Chessboard.Reset();

            if (rotatedBoard)
            {
                rotatedBoard = false;
                Chessboard.Mirror();
            }

            if (result.Item1 == 0)
            {
                opponent = null;
                await StartOnlineMatchAsync(tc);
            }
            else
            {
                if (result.Item1 == 1)
                {
                    opponent = new StupidoBot();
                    Chessboard.Restart(opponent, tc);
                }
                else
                {
                    opponent = null;
                    Chessboard.Restart(null, tc);
                }
            }

            Chessboard.EnablePieces();
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
                await DialogHelper.ShowErrorAsync(string.Format(Shared.Resources.strFailedToMove, ex.Message), Shared.Resources.strError);
            }
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

        private async void ButtonResign_Clicked(object sender, EventArgs e)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(currentMatchInfo);
                await APIClient.ResignAsync(currentMatchInfo.MatchId);
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowErrorAsync(string.Format(Shared.Resources.strFailedToResign, ex.Message), Shared.Resources.strError);
            }
        }

        private async void MenuItemAbout_Clicked(object sender, EventArgs e)
        {
            var dialog = new AboutDialog();
            await Navigation.PushModalAsync(dialog);
        }

        private async void MenuItemSettings_Clicked(object sender, EventArgs e)
        {
            var dialog = new SettingsDialog();
            dialog.OnRefrsh += Dialog_OnRefrsh;
            await Navigation.PushModalAsync(dialog);           
        }

        private void Dialog_OnRefrsh()
        {
            Chessboard.RenderChessBoard(Chessboard.Game.Board);
            Chessboard.RefreshBackground();
        }
    }
}