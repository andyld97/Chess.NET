using Chess.NET.Model;
using Chess.NET.Model.Pieces;

namespace Chess.NET.Bot
{
    public interface IChessBot
    {
        PendingMove? Move(Game game);

        int Elo { get; }

        string Name { get; }
    }
}