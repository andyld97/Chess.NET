using Chess.NET.Model;

namespace Chess.NET.Bot
{
    public interface IChessBot
    {
        (Piece, Position)? Move(Game game);
    }
}