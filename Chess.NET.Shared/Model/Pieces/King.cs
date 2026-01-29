namespace Chess.NET.Shared.Model.Pieces
{
    public class King : Piece
    {
        public override PieceType Type => PieceType.King;

        public override int MaterialValue => int.MaxValue;

        public King(Position position, Color color) : base(position, color)
        {
        }

        private List<Position> GetKingPositionsIntern(Position pos)
        {
            List<Position> positions = [];

            int file = pos.File;
            int rank = pos.Rank;

            // Top
            if (rank + 1 <= 8)
                positions.Add(new Position(file, rank + 1));
            // Top, Right
            if (rank + 1 <= 8 && file + 1 <= 8)
                positions.Add(new Position(file + 1, rank + 1));
            // Right    
            if (file + 1 <= 8)
                positions.Add(new Position(file + 1, rank));
            // Bottom, Right
            if (rank - 1 >= 1 && file + 1 <= 8)
                positions.Add(new Position(file + 1, rank - 1));
            // Bottom
            if (rank - 1 >= 1)
                positions.Add(new Position(file, rank - 1));
            // Bottom, Left
            if (rank - 1 >= 1 && file - 1 >= 1)
                positions.Add(new Position(file - 1, rank - 1));
            // Left
            if (file - 1 >= 1)
                positions.Add(new Position(file - 1, rank));
            // Left, Top
            if (rank + 1 <= 8 && file - 1 >= 1)
                positions.Add(new Position(file - 1, rank + 1));

            return positions;   
        }

        public override List<Position> GetPossibleMoves(IBoard board)
        {
            List<Position> positions = GetKingPositionsIntern(Position);

            foreach (var position in positions.ToArray())
            {
                var piece = board.GetPiece(position);

                if (piece != null && piece.Color == this.Color)
                    positions.Remove(position);
                else
                {
                    // Der König darf nicht auf ein Feld ziehen, das direkt anliegend zu dem gegnerischen König ist!
                    foreach (var nearKingPositions in GetKingPositionsIntern(position).Where(p => board.GetPiece(p) is King k && k.Color != Color))
                        positions.Remove(position);
                }             
            }

            return positions;
        }

        public override object Clone()
        {
            return new King((Position)Position.Clone(), Color);
        }
    }
}