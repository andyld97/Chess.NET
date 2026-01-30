using Chess.NET.Controls;
using Chess.NET.Model;
using Chess.NET.Shared.Model.Online;
using Microsoft.AspNetCore.SignalR.Client;
using System.Windows;

namespace Chess.NET.Netcode
{
    public class SignalRClient
    {
        public static readonly string CLIENT_ID = Guid.NewGuid().ToString();

        #region Events

        public delegate void onMatchFound(MatchInfo match);
        public event onMatchFound? OnMatchFound;

        public delegate void onMoveMade(MoveMade moveMade);
        public event onMoveMade? OnMoveMade;

        #endregion

        public async Task<Client?> ConnectAsync(ChessBoard chessBoard)
        {
            try
            {
                var connection = new HubConnectionBuilder()
                    .WithUrl($"{Consts.SERVER_URL_HUB}{CLIENT_ID}")
                    .WithAutomaticReconnect()
                    .Build();

                // Events
                connection.On<MatchInfo>("MatchFound", async match =>
                {
                    // White or Black? If black, rotate board (ensure that it was not rotated before)
                    await App.UiDispatcher.Invoke(async () =>
                    {
                        OnMatchFound?.Invoke(match);
                    });
                });

                connection.On<MoveMade>("MoveMade", async payload =>
                {
                    await App.UiDispatcher.Invoke(async () =>
                    {
                        OnMoveMade?.Invoke(payload);
                    });
                });

                // 2) Verbinden
                await connection.StartAsync();

                Console.WriteLine("Connected to SignalR hub.");

                // 3) Jetzt Queue-Join per HTTP
                var client = new Client
                {
                    ClientID = CLIENT_ID,
                    PlayerName = Settings.Instance.Player1Name,
                    PlayerElo = Settings.Instance.Player1Elo
                };

                await APIClient.JoinQueueAsync(client);

                return client;
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Failed to connect to chess server: {ex.Message}", Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }

        public async Task MakeMoveAsync(string matchId, string move)
        {
            try
            {
                await APIClient.MakeMoveAsync(matchId, move);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to make move: {ex.Message}", Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
