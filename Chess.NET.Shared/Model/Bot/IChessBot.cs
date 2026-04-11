namespace Chess.NET.Shared.Model.Bot
{
    public interface IChessBot
    {
        PendingMove? Move(Game game);

        int Elo { get; }

        string Name { get; }

        Color Color { get; }
    }
}