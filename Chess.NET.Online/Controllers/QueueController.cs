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
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<IActionResult> Join([FromBody] Client client)
        {
            var match = _matchmaking.Join(client);

            if (match != null)
            {
                _logger.LogInformation($"Found match: {match.ClientWhite.PlayerName} vs {match.ClientBlack.PlayerName}");
                await _hub.Clients.Users([match.ClientWhite.ClientID, match.ClientBlack.ClientID]).SendAsync("MatchFound", match);
                await _gameService.StartMatchAsync(match);
            }

            return Ok();
        }

        [HttpPost("Leave")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Leave([FromBody] Client client)
        {
            _matchmaking.Leave(client.ClientID);
            return Ok();
        }
    }
}
