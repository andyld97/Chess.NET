using Chess.NET.Shared.Model;
using Chess.NET.Shared.Model.Online;
using Microsoft.AspNetCore.SignalR;

namespace Chess.NET.Online.Services
{
    public interface IGameService
    {
        Task StartMatchAsync(Match match);

        Match? GetMatch(string matchId);
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
            game.OnCheckmate += Game_OnCheckmate;
            game.OnStalemate += Game_OnStalemate;

            match.Game = game;
            matches.Add(match);
        }

        private void Game_OnStalemate()
        {
            // TODO: Game is over
        }

        private void Game_OnCheckmate(Color pieceColor)
        {
            // TODO: Game is over
        }
    }
}
