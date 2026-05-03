using Android.App;
using Chess.NET.Android.Controls.Dialogs;
using Chess.NET.Shared.Model;
using Chess.NET.Shared.Model.Bot;
using Chess.NET.Shared.Model.Online;
using Chess.NET.Shared.Netcode;

namespace Chess.NET.Android
{
    public partial class MainPage : ContentPage
    {
        #region Online Match
        private Client? client = null;
        private SignalRClient _networkClient = null!; 
        private Shared.Model.Color? ownPieceColor = null;
        private MatchInfo? currentMatchInfo = null;
        private bool isOnlineMatch = false;

        private WaitingQueueDialog waitingQueueDialog;

        private async Task StartOnlineMatchAsync()
        {
            // TODO
            //if (string.IsNullOrEmpty(Settings.Instance.Player1Name))
            //{
            //    MessageBox.Show(Properties.Resources.strPleaseSetAName, Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

            ownPieceColor = null;
            currentMatchInfo = null;  

            _networkClient = new SignalRClient();
            _networkClient.OnMatchFound += _networkClient_OnMatchFound;
            _networkClient.OnMoveMade += _networkClient_OnMoveMade;
            _networkClient.OnMatchEnds += _networkClient_OnMatchEnds;

            try
            {
                waitingQueueDialog = new WaitingQueueDialog();
                await Navigation.PushModalAsync(waitingQueueDialog);
                client = await _networkClient.ConnectAsync("Player 1 (Android)" /*Settings.Instance.Player1Name*/, "500" /*Settings.Instance.Player1Elo*/);
 
                if (client == null)
                {
                    // Error
                    await Navigation.PopModalAsync();
                }
            }
            catch (Exception ex)
            {
                // TODO   MessageBox.Show(string.Format(Properties.Resources.strFailedToConnectToServer, ex.Message), Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void _networkClient_OnMatchEnds(MatchEnd matchEnd)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // TODO Chessboard.DisablePieces();
                // TODO ButtonResign.Visibility = Visibility.Collapsed;

                string playerWon = string.Empty;

                if (matchEnd.ColorWins.HasValue && matchEnd.ColorWins == ownPieceColor)
                    playerWon = "Player 1"; // TODO:  Settings.Instance.Player1Name;
                else if (matchEnd.ColorWins.HasValue)
                    playerWon = currentMatchInfo?.OpponentName ?? string.Empty;

                currentMatchInfo = null;
                ButtonRestart.IsEnabled = true;
                isOnlineMatch = false;
                await _networkClient.DisconnectAsync();

                // TODO
                //await Task.Delay(50).ContinueWith(t =>
                //{
                //    Application.Current.Dispatcher.Invoke(() =>
                //    {
                //        // Only consider sounds that are normally not played using the Game-Class
                //        if (matchEnd.Result == GameResult.Disconnected || matchEnd.Result == GameResult.Resign || matchEnd.Result == GameResult.Timeout)
                //            Sound.Play(SoundType.Checkmate);

                //        GameOverDialog gameOverDialog = new GameOverDialog(matchEnd.Result, matchEnd.ColorWins, playerWon) { Owner = this };
                //        gameOverDialog.ShowDialog();
                //    });
                //});

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
                // TODO: ButtonResign.Visibility = Visibility.Visible;
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
                // TODO RefreshPlayerDisplay();
            });
        }

        #endregion

        public MainPage()
        {
            InitializeComponent();
        }

        private async void ButtonRestart_Clicked(object sender, EventArgs e)
        {
            var dialog = new NewGameDialog();
            await Navigation.PushModalAsync(dialog);

            var result = await dialog.WaitForResultAsync();

            if (result == 0)
                await StartOnlineMatchAsync();
            else
            {
                if (result == 1)
                    Chessboard.Restart(new StupidoBot());
                else
                    Chessboard.Restart(null);
            }
        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            Chessboard.Mirror();
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
    }
}
