using Chess.NET.Shared.Model.Pieces;
using System.Text;

namespace Chess.NET.Shared.Model
{
    public interface IBoard : ICloneable
    {
        Piece? GetPiece(Position pos);

        void CapturePiece(Piece piece);

        Piece? GetPiece(string pos);

        List<Piece> Pieces { get; }
    }

    public class Board : IBoard
    {
        public List<Piece> Pieces { get; } = [];

        public List<Piece> CapturedPieces { get; set; } = [];

        public List<Piece> PromotedPieces { get; set; } = [];   

        public Piece? GetPiece(string pos)
        {
            return GetPiece(Position.Parse(pos));
        }   

        public Piece? GetPiece(Position pos)
        {
            return Pieces.FirstOrDefault(p => p.Position == pos);
        }

        public void Reset()
        {
            CapturedPieces = [];
            Pieces.Clear();
            PromotedPieces.Clear();

            foreach (var color in new List<Color>() { Color.White, Color.Black })
            {
                int fileForPawns = 2;
                if (color == Color.Black)
                    fileForPawns = 7;

                int fileForOtherFigures = 1;
                if (color == Color.Black)
                    fileForOtherFigures = 8;

                // Add the pawns
                for (int p = 0; p < 8; p++)
                    Pieces.Add(new Pawn(new Position((p + 1), fileForPawns), color));

                // The roooook(s)
                Pieces.Add(new Rook(new Position(1, fileForOtherFigures), color));
                Pieces.Add(new Rook(new Position(8, fileForOtherFigures), color));

                // The kNight(s)
                Pieces.Add(new Knight(new Position(2, fileForOtherFigures), color));
                Pieces.Add(new Knight(new Position(7, fileForOtherFigures), color));

                // The bishop(s)
                Pieces.Add(new Bishop(new Position(3, fileForOtherFigures), color));
                Pieces.Add(new Bishop(new Position(6, fileForOtherFigures), color));

                // The queen
                Pieces.Add(new Queen(new Position(4, fileForOtherFigures), color));

                // The king
                Pieces.Add(new King(new Position(5, fileForOtherFigures), color));
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("  ┌──────────────────────────┐");

            for (int rank = 8; rank >= 1; rank--)
            {
                sb.Append(rank);
                sb.Append(" │ ");

                for (int file = 1; file <= 8; file++)
                {
                    var piece = GetPiece(new Position(file, rank));
                    if (file < 8)
                    {

                        if (piece == null)
                            sb.Append("·  ");
                        else
                            sb.Append($"{piece.ToEmoji()} ");
                    }
                    else
                    {
                        if (piece == null)
                            sb.Append("·  ");
                        else
                            sb.Append($"{piece.ToEmoji()} ");
                    }
                }
                sb.Append(" │");
                sb.AppendLine();
            }

            sb.AppendLine("  └──────────────────────────┘");
            sb.AppendLine("    a  b  c  d  e  f  g  h");

            return sb.ToString();
        }      

        public void CapturePiece(Piece piece)
        {
            CapturedPieces.Add(piece);
            Pieces.Remove(piece);   
        }

        public bool IsCheck(Color pieceColor)
        {
            var king = Pieces.First(p => p.Type == PieceType.King && p.Color == pieceColor);
            var opponentColor = Helper.InvertColor(pieceColor);

            foreach (var oponnentPiece in Pieces.Where(p => p.Type != PieceType.King && p.Color == opponentColor))
            {
                if (oponnentPiece.GetPossibleMoves(this).Any(p => p == king.Position))
                    return true;
            }

            return false;
        }

        public object Clone()
        {
            Board board = new Board();
            foreach (var piece in Pieces)
            {
                if (piece is Pawn)
                    board.Pieces.Add(new Pawn((Position)piece.Position.Clone(), piece.Color));
                else if (piece is Rook)
                    board.Pieces.Add(new Rook((Position)piece.Position.Clone(), piece.Color));
                else if (piece is Knight)
                    board.Pieces.Add(new Knight((Position)piece.Position.Clone(), piece.Color));
                else if (piece is Bishop)
                    board.Pieces.Add(new Bishop((Position)piece.Position.Clone(), piece.Color));
                else if (piece is Queen)
                    board.Pieces.Add(new Queen((Position)piece.Position.Clone(), piece.Color));
                else if (piece is King)
                    board.Pieces.Add(new King((Position)piece.Position.Clone(), piece.Color));
             
            }

            foreach (var piece in CapturedPieces)
            {
                if (piece is Pawn)
                    board.CapturedPieces.Add(new Pawn((Position)piece.Position.Clone(), piece.Color));
                else if (piece is Rook)
                    board.CapturedPieces.Add(new Rook((Position)piece.Position.Clone(), piece.Color));
                else if (piece is Knight)
                    board.CapturedPieces.Add(new Knight((Position)piece.Position.Clone(), piece.Color));
                else if (piece is Bishop)
                    board.CapturedPieces.Add(new Bishop((Position)piece.Position.Clone(), piece.Color));
                else if (piece is Queen)
                    board.CapturedPieces.Add(new Queen((Position)piece.Position.Clone(), piece.Color));
                else if (piece is King)
                    board.CapturedPieces.Add(new King((Position)piece.Position.Clone(), piece.Color));
            }

            return board;
        }
    }
}