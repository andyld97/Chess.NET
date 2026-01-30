using Chess.NET.Online.Services;
using Chess.NET.Shared.Model.Online;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Chess.NET.Online.Controllers
{
    [ApiController]
    [Route("queue")]
    public class QueueController : ControllerBase
    {
        private readonly IMatchMakingService _matchmaking;
        private readonly IHubContext<GameHub> _hub;
        private readonly ILogger<QueueController> _logger;
        private readonly IGameService _gameService;

        public QueueController(ILogger<QueueController> logger, IMatchMakingService matchmaking, IHubContext<GameHub> hub, IGameService gameService)
        {
            _logger = logger;
            _matchmaking = matchmaking;
            _hub = hub;
            _gameService = gameService;
        }

        [HttpPost("Join")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Join([FromBody] Client client)
        {
            var match = _matchmaking.Join(client);

            if (match != null)
            {
                _logger.LogInformation($"Found match: {match.ClientWhite.PlayerName} vs {match.ClientBlack.PlayerName}");

                MatchInfo matchInfoWhite = new MatchInfo
                {
                    MatchId = match.MatchId,
                    OpponentName = match.ClientBlack.PlayerName,
                    OpponentElo = match.ClientBlack.PlayerElo,
                    OpponentColor = Shared.Model.Color.Black,
                    ClientColor = Shared.Model.Color.White,
                };

                MatchInfo matchInfoBlack = new MatchInfo
                {
                    MatchId = match.MatchId,
                    OpponentName = match.ClientWhite.PlayerName,
                    OpponentElo = match.ClientWhite.PlayerElo,
                    OpponentColor = Shared.Model.Color.White,
                    ClientColor = Shared.Model.Color.Black
                };

                // await _hub.Clients.Users([match.ClientWhite.ClientID, match.ClientBlack.ClientID]).SendAsync("MatchFound", match);

                await _hub.Clients.Users(match.ClientWhite.ClientID).SendAsync("MatchFound", matchInfoWhite);
                await _hub.Clients.Users(match.ClientBlack.ClientID).SendAsync("MatchFound", matchInfoBlack);

                await _gameService.StartMatchAsync(match);
            }

            return Ok();
        }

        [HttpPost("Leave")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Leave([FromBody] Client client)
        {
            _logger.LogInformation($"Client {client.ClientID} leaving queue ...");
            _matchmaking.Leave(client.ClientID);
            return Ok();
        }
    }
}
