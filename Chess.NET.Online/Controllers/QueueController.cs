using Chess.NET.Online.Services;
using Chess.NET.Shared.Model.Online;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Chess.NET.Online.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly IMatchMakingService _matchmaking;
        private readonly IHubContext<GameHub> _hub;
        private readonly ILogger<QueueController> _logger;
        private readonly IGameService _gameService;
        private readonly WebhookAPI.Webhook _webhook;

        public QueueController(ILogger<QueueController> logger, IMatchMakingService matchmaking, IHubContext<GameHub> hub, IGameService gameService, WebhookAPI.Webhook webhook)
        {
            _logger = logger;
            _matchmaking = matchmaking;
            _hub = hub;
            _gameService = gameService;
            _webhook = webhook;
        }

        /// <summary>
        /// Handles a client request to join the matchmaking queue and attempts to pair the client with an available
        /// opponent.
        /// </summary>
        /// <remarks>If a match is found, both clients are notified via real-time messaging and the match
        /// is started. If no match is available, the client remains in the matchmaking queue until paired.</remarks>
        /// <param name="client">The client information used to join the matchmaking queue. Must not be null.</param>
        /// <returns>An HTTP 200 OK response indicating that the join request was processed.</returns>
        [HttpPost("Join")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Join([FromBody] Client client)
        {
            if (string.IsNullOrEmpty(client.PlayerName))
                return BadRequest("PlayerName is required.");

            var match = _matchmaking.Join(client);

            await _webhook.PostWebHookAsync(WebhookAPI.Webhook.LogLevel.Info, $"Client {client.PlayerName} joined the queue.", "Chess");

            if (match != null)
            {
                _logger.LogInformation($"Found match: {match.ClientWhite.PlayerName} [WHITE] vs {match.ClientBlack.PlayerName} [BLACK]");

                await _webhook.PostWebHookAsync(WebhookAPI.Webhook.LogLevel.Info, $"Found match: {match.ClientWhite.PlayerName} [WHITE] vs {match.ClientBlack.PlayerName} [BLACK]", "Chess");

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

                await _hub.Clients.Users(match.ClientWhite.ClientID).SendAsync("MatchFound", matchInfoWhite);
                await _hub.Clients.Users(match.ClientBlack.ClientID).SendAsync("MatchFound", matchInfoBlack);

                await _gameService.StartMatchAsync(match);
            }

            return Ok();
        }

        /// <summary>
        /// Removes the specified client from the matchmaking queue.
        /// </summary>
        /// <param name="client">The client to remove from the queue. The <see cref="Client.ClientID"/> property must identify a client
        /// currently in the queue.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns HTTP 200 (OK) if the client
        /// was successfully removed.</returns>
        [HttpPost("Leave")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Leave([FromBody] Client client)
        {
            await _webhook.PostWebHookAsync(WebhookAPI.Webhook.LogLevel.Info, $"Client {client.PlayerName} left the queue.", "Chess");
            _matchmaking.Leave(client.ClientID, "Client left Queue");
            return Ok();
        }
    }
}