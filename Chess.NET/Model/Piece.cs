namespace Chess.NET.Model
{
    public abstract class Piece
    {
        public Position Position { get; set; }  

        public abstract PieceType Type { get; }

        public abstract int MaterialValue { get; }

        public PieceColor Color { get; set; }

        public Piece(Position position, PieceColor color)
        {
            Position = position;
            Color = color;
        }

        public override string ToString()
        {
            return $"{Type}: {Position}";
        }

        public abstract List<Position> GetPossibleMoves(IBoard board);

        #region Helper Methods

        protected void AddRay(List<Position> moves, IBoard board, int dx, int dy)
        {
            var current = Position;

            while (true)
            {
                if (current.File + dx < 1 || current.File + dx > 8 ||
                    current.Rank + dy < 1 || current.Rank + dy > 8)
                    return; // out of bounds

                current = new Position(current.File + dx, current.Rank + dy);

                var piece = board.GetPiece(current);

                if (piece == null)
                {
                    moves.Add(current);
                    continue;
                }

                if (piece.Color != Color)
                    moves.Add(current); // capture allowed

                return; // blocked
            }
        }

        #endregion
    }

    public enum PieceType
    {
        Pawn,
        Rook,
        Knight,
        Bishop,
        Queen,
        King,
    }

    public enum PieceColor
    {
        White,
        Black
    }   
}
