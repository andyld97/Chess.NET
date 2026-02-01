using Chess.NET.Shared.Model;
using Chess.NET.Shared.Model.Online;
using Microsoft.AspNetCore.SignalR;

namespace Chess.NET.Online.Services
{
    public interface IGameService
    {
        Task StartMatchAsync(Match match);

        Match? GetMatch(string matchId);

        Match? GetMatchByClientId(string clientId);

        Task EndMatchAsync(string matchId, GameResult matchResult, Color? color);
    }

    public class GameService : IGameService
    {
        private readonly ILogger<GameService> _logger;
        private readonly IHubContext<GameHub> _hub;
        private readonly List<Match> matches = [];

        public GameService(ILogger<GameService> logger, IHubContext<GameHub> hub)
        {
            _logger = logger;
            _hub = hub;
        }

        public Match? GetMatch(string matchId)
        {
            var match = matches.FirstOrDefault(m => m.MatchId == matchId);
            return match;
        }

        public Match? GetMatchByClientId(string clientId)
        {
            return matches.FirstOrDefault(p => p.ClientBlack.ClientID == clientId || p.ClientWhite.ClientID == clientId);
        }

        public async Task StartMatchAsync(Match match)
        {
            await match.MatchSemaphore.WaitAsync();

            try
            {
                var game = new Game();
                game.StartNewGame(null);
                game.OnMovedPiece += async delegate (MoveNotation move)
                {
                    // Notify clients
                    await _hub.Clients.Users([match.ClientWhite.ClientID, match.ClientBlack.ClientID]).SendAsync("MoveMade", new MoveMade() { Move = move.FormatMove(false, false), Color = move.Piece.Color });
                };
                game.OnGameOver += async delegate (GameResult result, Color? colorWon)
                {
                    await EndMatchAsync(match.MatchId, result, colorWon);
                };

                match.Game = game;
                matches.Add(match);
            }
            finally
            {
                match.MatchSemaphore.Release(); 
            }
        }

        public async Task EndMatchAsync(string matchId, GameResult matchResult, Color? color)
        {
            var match = GetMatch(matchId);
            ArgumentNullException.ThrowIfNull(match);

            await match.MatchSemaphore.WaitAsync();

            try
            {
                matches.Remove(match);

                string matchLog = $"[{matchId}] ended: ";
                if (matchResult == GameResult.Stalemate)
                    matchLog += "Stalemate (Remis)";
                else
                {
                    string playerName = string.Empty;
                    if (color == Color.Black)
                        playerName = match.ClientBlack.PlayerName;
                    else if (color == Color.White)
                        playerName = match.ClientWhite.PlayerName;

                    matchLog += $"{playerName} [{color}] wins due to {matchResult}!";
                }

                _logger.LogInformation(matchLog);

                MatchEnd matchEnd = new MatchEnd
                {
                    ColorWins = color,
                    Result = matchResult
                };

                // Notify clients that the match has ended
                await _hub.Clients.Users([match.ClientWhite.ClientID, match.ClientBlack.ClientID]).SendAsync("GameOver", matchEnd);
            }
            finally
            {
                match.MatchSemaphore.Release(); 
            }
        }
    }
}