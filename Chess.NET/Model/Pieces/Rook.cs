namespace Chess.NET.Model.Pieces
{
    public class Rook : Piece
    {
        public override PieceType Type => PieceType.Rook; // The roooooooooooooook

        public override int MaterialValue => 5;

        public Rook(Position position, PieceColor color) : base(position, color)
        {
        }

        public override List<Position> GetPossibleMoves(IBoard board)
        {
            var moves = new List<Position>();

            AddRay(moves, board, 1, 0);     // rechts
            AddRay(moves, board, -1, 0);    // links
            AddRay(moves, board, 0, 1);     // oben
            AddRay(moves, board, 0, -1);    // unten

            return moves;
        }

        public override object Clone()
        {
            return new Rook((Position)Position.Clone(), Color);
        }
    }
}