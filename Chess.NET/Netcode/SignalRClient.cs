using Chess.NET.Controls;
using Chess.NET.Model;
using Chess.NET.Shared.Model;
using Chess.NET.Shared.Model.Online;
using Microsoft.AspNetCore.SignalR.Client;
using System.CodeDom;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;

namespace Chess.NET.Netcode
{
    public class SignalRClient
    {
        private static readonly string SERVER_URL = "http://localhost:5010";
        private static readonly string SERVER_URL_HUB = $"{SERVER_URL}/hubs/game?clientId=";


        private static readonly string CLIENT_ID = Guid.NewGuid().ToString();

        private static Color ownPieceColor = Color.White;
        private static Match? currentMatch = null;

        public static async Task ConnectAsync(ChessBoard chessBoard)
        {
            try
            {
                var connection = new HubConnectionBuilder()
                    .WithUrl($"{SERVER_URL_HUB}{CLIENT_ID}")
                    .WithAutomaticReconnect()
                    .Build();

                // Events
                connection.On<Match>("MatchFound", async match =>
                {
                    // White or Black? If black, rotate board (ensure that it was not rotated before)
                    await App.UiDispatcher.Invoke(async () =>
                    {
                        if (match.ClientBlack.ClientID == CLIENT_ID)
                        {
                            chessBoard.Mirror();
                            ownPieceColor = Color.Black;
                        }
                        else
                            ownPieceColor = Color.White;

                        currentMatch = match;
                        chessBoard.Game.StartNewGame(null);
                        chessBoard.SetOnline(ownPieceColor);

                    });
                });

                connection.On<MoveMade>("MoveMade", async payload =>
                {
                    await App.UiDispatcher.Invoke(async () =>
                    {
                        var pendingMove = PendingMove.Parse(payload.Move, (Board)chessBoard.Game.Board,  payload.Color);
                        if (payload.Color != ownPieceColor) 
                        {
                            await chessBoard.Game.MoveAsync(pendingMove, true);
                            chessBoard.RenderChessBoard(chessBoard.Game.Board, true);
                        }
                    });
                });

                // 2) Verbinden
                await connection.StartAsync();

                Console.WriteLine("Connected to SignalR hub.");


                // 3) Jetzt Queue-Join per HTTP
                using var http = new HttpClient();

                var client = new Client
                {
                    ClientID = CLIENT_ID,
                    PlayerName = Settings.Instance.Player1Name,
                    PlayerElo = Settings.Instance.Player1Elo
                };

                var response = await http.PostAsync($"{SERVER_URL}/queue/join", new StringContent(JsonSerializer.Serialize(client), Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Failed to connect to chess server: " + ex.Message);
            }

        }

        public static async Task MakeMoveAsync(string move)
        {
            if (currentMatch == null)
                return;

            // TODO: Unbedingt verhindern, dass man Züge mit den gegnerischen Figuren machen kann

            try
            {
                using var http = new HttpClient();
                var content = new StringContent(JsonSerializer.Serialize(move), Encoding.UTF8, "application/json");
                var response = await http.PostAsync($"{SERVER_URL}/game/{currentMatch.MatchId}/{CLIENT_ID}/MakeMove", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                MessageBox.Show("failed to make move: " + ex.Message);
            }
        }

    }
}
