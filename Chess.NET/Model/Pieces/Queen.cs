namespace Chess.NET.Model.Pieces
{
    public class Queen : Piece
    {
        public override PieceType Type => PieceType.Queen;

        public override int MaterialValue => 9;

        public Queen(Position position, PieceColor color) : base(position, color)
        {
        }

        public override List<Position> GetPossibleMoves(IBoard board)
        {
            var moves = new List<Position>();

            AddRay(moves, board, 1, 0);     // rechts
            AddRay(moves, board, -1, 0);    // links
            AddRay(moves, board, 0, 1);     // oben
            AddRay(moves, board, 0, -1);    // unten

            AddRay(moves, board, 1, 1);     // diagonal
            AddRay(moves, board, 1, -1);
            AddRay(moves, board, -1, 1);
            AddRay(moves, board, -1, -1);

            return moves;
        }    
    }
}