using Chess.NET.Online.Services;
using Chess.NET.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace Chess.NET.Online.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private readonly IGameService _gameService;
        private readonly IConfiguration _configuration;
        private readonly WebhookAPI.Webhook _webhook;

        private readonly bool webHook_SendImages = false;
        private readonly string webHook_ImageURL = string.Empty;    
        private readonly string webHook_APIKey = string.Empty;

        public GameController(ILogger<GameController> logger, IGameService gameService, WebhookAPI.Webhook webhook, IConfiguration configuration)
        {
            _logger = logger;
            _gameService = gameService;
            _webhook = webhook;
            _configuration = configuration;

            webHook_SendImages = configuration.GetValue<bool>("Webhook:Images:enabled");
            webHook_ImageURL = configuration.GetValue<string>("Webhook:Images:url") ?? string.Empty;
            webHook_APIKey = configuration.GetValue<string>("Webhook:Images:api-key") ?? string.Empty;
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

                    if (!webHook_SendImages)
                        await _webhook.PostWebHookAsync(WebhookAPI.Webhook.LogLevel.Info, $"[{match}]: Move {move} was made by {col}!", "Chess");
                    else
                    {
                        var lastMove = match.Game.Moves.LastOrDefault();
                        var renderer = new Chess.NET.Shared.Renderer(64, lastMove?.From ?? null, lastMove?.To ?? null);
                        var image = renderer.Render(match.Game.Board as Board, "default", match.ClientWhite.PlayerName, match.ClientWhite.PlayerElo, match.ClientBlack.PlayerName, match.ClientBlack.PlayerElo);

                        _ = Task.Run(() => SendGameStatusAsync(image, $"{Math.Ceiling(match.Game.Moves.Count / 2.0)}. {move}"));
                    }

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

        private async Task<HttpResponseMessage> SendGameStatusAsync(byte[] imageBytes, string text, string fileName = "image.png")
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("X-API-KEY", webHook_APIKey);

                using var content = new MultipartFormDataContent();

                var imageContent = new ByteArrayContent(imageBytes);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                content.Add(imageContent, "image", fileName);
                content.Add(new StringContent(text), "text");

                return await httpClient.PostAsync(webHook_ImageURL, content);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send game status with image: {ex}");
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}