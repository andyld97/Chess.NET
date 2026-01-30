using Chess.NET.Online.Services;
using Chess.NET.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Chess.NET.Online.Controllers
{
    [ApiController]
    [Route("game")]
    public class GameController : ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private readonly IGameService _gameService;

        public GameController(ILogger<GameController> logger, IGameService gameService)
        {
            _logger = logger;   
            _gameService = gameService;
        }

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

            // TODO: Resign match

            return Ok();

        }

        [HttpPost("{matchId}/{clientId}/MakeMove")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MakeMove(string matchId, string clientId, [FromBody] string move)
        {
            // TODO: mit lock absichern

            var match = _gameService.GetMatch(matchId);
            if (match == null)
                return NotFound();

            // Find color
            Color? col = null;
            if (match.ClientBlack.ClientID == clientId)
                col = Color.Black;
            else if (match.ClientWhite.ClientID == clientId)
                col = Color.White;

            if (col == null || match.Game.PlayersTurn != col)
            {
                _logger.LogInformation($"[{match.MatchId}]: Move {move} by {col} not accepted: Wrong Color!");
                return BadRequest(); // wrong player 
            }

            PendingMove? pendingMove = PendingMove.Parse(move, (Board)match.Game.Board, col.Value!);
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

            await match.Game.MoveAsync(pendingMove, false);
            _logger.LogInformation($"[{match.MatchId}]: Move {move} was made by {col}!");
            return Ok();
        }
    }
}