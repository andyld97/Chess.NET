using Chess.NET.Online.Services;
using Chess.NET.Shared.Model;
using Microsoft.AspNetCore.Mvc;

namespace Chess.NET.Online.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private readonly IGameService _gameService;
        private readonly WebhookAPI.Webhook _webhook;

        public GameController(ILogger<GameController> logger, IGameService gameService, WebhookAPI.Webhook webhook)
        {
            _logger = logger;
            _gameService = gameService;
            _webhook = webhook;
        }

        /// <summary>
        /// Processes a resignation request for a player in the specified match.
        /// </summary>
        /// <param name="matchId">The unique identifier of the match in which the resignation is requested.</param>
        /// <param name="clientId">The unique identifier of the client attempting to resign from the match.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the resignation request. Returns 200 OK if the
        /// resignation is successful; 400 Bad Request if the request is invalid; 401 Unauthorized if the client is not
        /// a participant in the match; or 404 Not Found if the match does not exist.</returns>
        [HttpPost("{matchId}/{clientId}/Resign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Resign(string matchId, string clientId)
        {
            var match = _gameService.GetMatch(matchId);
            if (match == null)
                return NotFound();

            if (match.ClientWhite.ClientID != clientId && match.ClientBlack.ClientID != clientId)
                return Unauthorized();

            await match.MatchSemaphore.WaitAsync();

            try
            {
                var resignColor = match.GetColorByClientId(clientId);
                if (resignColor == null)
                    return BadRequest();

                match.Game.Resign(resignColor.Value);

                await _webhook.PostWebHookAsync(WebhookAPI.Webhook.LogLevel.Info, $"[{match}]: Player {resignColor} resigned.", "Chess");
            }
            finally
            {
                match.MatchSemaphore.Release();
            }

            return Ok();
        }

        /// <summary>
        /// Processes a move request for the specified match and client.
        /// </summary>
        /// <remarks>The move is only accepted if it is the requesting client's turn and the move is valid
        /// according to the current game state. The method does not process moves for clients not participating in the
        /// match.</remarks>
        /// <param name="matchId">The unique identifier of the match in which the move is to be made.</param>
        /// <param name="clientId">The unique identifier of the client attempting to make the move.</param>
        /// <param name="move">A string representing the move to be made, formatted according to the game's move notation.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the move request. Returns 200 OK if the move is
        /// accepted and processed; 400 Bad Request if the move is invalid, not the client's turn, or cannot be parsed;
        /// or 404 Not Found if the match does not exist.</returns>
        [HttpPost("{matchId}/{clientId}/MakeMove")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MakeMove(string matchId, string clientId, [FromBody] string move)
        {
            var match = _gameService.GetMatch(matchId);
            if (match == null)
                return NotFound();

            await match.MatchSemaphore.WaitAsync();

            try
            {
                // Find color
                Color? col = match.GetColorByClientId(clientId);

                if (col == null || match.Game.PlayersTurn != col)
                {
                    _logger.LogInformation($"[{match.MatchId}]: Move {move} by {col} not accepted: Wrong Color!");
                    return BadRequest(); // wrong player 
                }

                _logger.LogDebug($"Parsing pending move: {move}");
                try
                {
                    PendingMove? pendingMove = PendingMove.Parse(move, (Board)match.Game.Board, match.Game, col.Value!);
                    if (pendingMove == null)
                    {
                        _logger.LogInformation($"[{match.MatchId}]: Move {move} by {col} not accepted: Move couldn't be parsed!");
                        return BadRequest(); // move cannot be parsed!
                    }

                    if (!match.Game.IsMoveValid(pendingMove.Piece, pendingMove.To))
                    {
                        _logger.LogInformation($"[{match.MatchId}]: Move {move} by {col} not accepted: Illegal Move!");
                        return BadRequest(); // illegal move!
                    }

                    match.Game.Move(pendingMove, false);

                    _logger.LogInformation($"[{match.MatchId}]: Move {move} was made by {col}!");

                    await _webhook.PostWebHookAsync(WebhookAPI.Webhook.LogLevel.Info, $"[{match}]: Move {move} was made by {col}!", "Chess");

                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[{match.MatchId}]: Move {move} by {col} not accepted: Exception during parsing! {ex.Message}");
                    await _webhook.PostWebHookAsync(WebhookAPI.Webhook.LogLevel.Error, $"[{match}]: Move {move} by {col} not accepted: Exception during parsing! {ex.Message}", "Chess");

                    return BadRequest(); // move cannot be parsed!
                }
            }
            finally
            {
                match.MatchSemaphore.Release();
            }
        }
    }
}