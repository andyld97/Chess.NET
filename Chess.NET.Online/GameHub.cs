using Chess.NET.Online.Services;
using Chess.NET.Shared;
using Chess.NET.Shared.Model;
using Microsoft.AspNetCore.SignalR;

namespace Chess.NET.Online
{
    public class GameHub : Hub
    {
        private readonly ILogger<GameHub> _logger;   
        private readonly IMatchMakingService _matchMakingService;
        private readonly IGameService _gameService;

        public GameHub(ILogger<GameHub> logger, IMatchMakingService matchMakingService, IGameService gameService)
        {
            _logger = logger;
            _matchMakingService = matchMakingService;
            _gameService = gameService;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var clientId = Context.UserIdentifier;

            if (clientId != null)
            {
                _matchMakingService.Leave(clientId, "Disconnected");

                // Also check if clientId is currently in a match
                var match = _gameService.GetMatchByClientId(clientId);
                if (match != null)
                {
                    _logger.LogInformation($"[{match.MatchId}]: Client {clientId} disconnected! Ending game!");
                    Color? col = match.GetColorByClientId(clientId);
                    ArgumentNullException.ThrowIfNull(col);

                    match.Game.Resign(col.Value, true);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}