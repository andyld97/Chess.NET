using Chess.NET.Shared.Model.Online;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chess.NET.Shared.Netcode
{
    public class SignalRClient
    {
        public static readonly string CLIENT_ID = Guid.NewGuid().ToString();
        private HubConnection? connection = null;

        #region Events

        public delegate void onMatchFound(MatchInfo match);
        public event onMatchFound? OnMatchFound;

        public delegate void onMoveMade(MoveMade moveMade);
        public event onMoveMade? OnMoveMade;

        public delegate void onMatchEnds(MatchEnd matchEnd);
        public event onMatchEnds? OnMatchEnds;
        #endregion

        public async Task<Client?> ConnectAsync(string playerName, string playerElo)
        {
            connection = new HubConnectionBuilder()
                .WithUrl($"{Consts.SERVER_URL_HUB}{CLIENT_ID}")
                .WithAutomaticReconnect()
                .Build();

            connection.On<MatchInfo>("MatchFound", async match =>
            {
                OnMatchFound?.Invoke(match);
            });

            connection.On<MoveMade>("MoveMade", async payload =>
            {
                OnMoveMade?.Invoke(payload);
            });

            connection.On<MatchEnd>("GameOver", async payload =>
            {
                OnMatchEnds?.Invoke(payload);
            });

            await connection.StartAsync();

            var client = new Client
            {
                ClientID = CLIENT_ID,
                PlayerName = playerName,
                PlayerElo = playerElo
            };

            await APIClient.JoinQueueAsync(client);

            return client;
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (connection != null)
                {
                    await connection.StopAsync();
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}
