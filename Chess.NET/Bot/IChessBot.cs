using Chess.NET.Model;

namespace Chess.NET.Bot
{
    public interface IChessBot
    {
        NextMove? Move(Game game);

        int Elo { get; }

        string Name { get; }
    }

    public class NextMove
    {
        public Piece Piece { get; set; }

        public Position To { get; set; }

        public PieceType? PromotionType { get; set; } = null;

        public NextMove(Piece piece, Position to, PieceType? promotionType = null)
        {
            Piece = piece;
            To = to;
            PromotionType = promotionType;
        }
    }
}