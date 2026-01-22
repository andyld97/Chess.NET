namespace Chess.NET.Model.Pieces
{
    public class Knight : Piece
    {
        public override PieceType Type => PieceType.Knight;

        public override int MaterialValue => 3;

        public Knight(Position position, PieceColor color) : base(position, color)
        {
        }

        public override List<Position> GetPossibleMoves(IBoard board)
        {
            List<Position> positions = [];
            int file = Position.File;
            int rank = Position.Rank;
            
            // All 8 possible knight moves
            List<(int, int)> knightMoves = [
                (2, 1), (1, 2), (-1, 2), (-2, 1),
                (-2, -1), (-1, -2), (1, -2), (2, -1)
            ];
            foreach (var move in knightMoves)
            {
                int newFile = file + move.Item1;
                int newRank = rank + move.Item2;

                if (newFile >= 1 && newFile <= 8 && newRank >= 1 && newRank <= 8)
                {
                    var pos = new Position(newFile, newRank);

                    var currentPieceThere = board.GetPiece(pos);

                    if (currentPieceThere == null || currentPieceThere.Color != this.Color)
                        positions.Add(pos);
                }
            }
            return positions;
        }

        public override object Clone()
        {
            return new Knight((Position)Position.Clone(), Color);
        }
    }
}