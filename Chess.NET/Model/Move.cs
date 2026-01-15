using Chess.NET.Model.Pieces;

namespace Chess.NET.Model
{
    public class Move
    {
        public PieceType PieceType { get; set; }

        public Position From { get; set; }

        public Position To { get; set; }

        public bool IsCapture { get; set; }

        public PieceColor PieceColor { get; set; }

        public bool IsCastleKingSide { get; set; }

        public bool IsCastleQueenSide { get; set; }

        public string ToUci()
        {
            if (PieceType == PieceType.Pawn && (To.Rank == 8 || To.Rank == 1)) // IST EH NOCH TODO, weil PROMOTION eh nicht geht.
                return $"{From}{To}q";

            return $"{From}{To}".ToLower();
        }

        public override string ToString()
        {
            string chessNotation = PieceType switch
            {
                PieceType.Pawn => "",
                PieceType.Knight => "N",
                PieceType.Bishop => "B",
                PieceType.Rook => "R",
                PieceType.Queen => "Q",
                PieceType.King => "K",
                _ => ""
            };

            string fromNotation = From.ToString();
            string toNotation = To.ToString();  

            string captureNotation = IsCapture ? "x" : "";

           return $"{chessNotation}{fromNotation}{captureNotation}{toNotation}";

        }

    }
}
