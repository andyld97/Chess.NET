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

        Task EndMatchAsync(string matchId, MatchResult matchResult, Color? color);
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

        public async Task StartMatchAsync(Match match)
        {
            // TODO: mit lock absichern

            var game = new Game();
            game.StartNewGame(null);
            game.OnMovedPiece += async delegate (MoveNotation move) 
            {
                // Notify clients
                await _hub.Clients.Users([match.ClientWhite.ClientID, match.ClientBlack.ClientID]).SendAsync("MoveMade", new MoveMade() { Move = move.FormatMove(false, false), Color = move.Piece.Color });
            };
            game.OnCheckmate += async delegate 
            {
                await EndMatchAsync(match.MatchId, MatchResult.Checkmate, null);
            };
            game.OnStalemate += async delegate
            {
                await EndMatchAsync(match.MatchId, MatchResult.Checkmate, null);
            };
            game.OnResign += async delegate (Color color)
            {
                await EndMatchAsync(match.MatchId, MatchResult.Resign, color); 
            };

            match.Game = game;
            matches.Add(match);
        }

        public Match? GetMatchByClientId(string clientId)
        {
            return matches.FirstOrDefault(p => p.ClientBlack.ClientID == clientId || p.ClientWhite.ClientID == clientId);
        }

        public async Task EndMatchAsync(string matchId, MatchResult matchResult, Color? color)
        {
            var match = GetMatch(matchId);
            ArgumentNullException.ThrowIfNull(match);

            matches.Remove(match);

            // Notify clients that the match has ended
            await _hub.Clients.Users([match.ClientWhite.ClientID, match.ClientBlack.ClientID]).SendAsync("GameOver", "TODO DATENSTRUKTUR");
        }
    }
}
