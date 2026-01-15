using Chess.NET.Model;

namespace Chess.NET
{
    public static class Helper
    {
        public static PieceColor InvertPieceColor(PieceColor pieceColor)
        {
            if (pieceColor == PieceColor.White)
                return PieceColor.Black;

            return PieceColor.White;
        }
    }
}
