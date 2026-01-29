using Chess.NET.Shared.Model;

namespace Chess.NET.Shared.Model.Bot
{
    public interface IChessBot
    {
        PendingMove? Move(Game game);

        int Elo { get; }

        string Name { get; }
    }
}