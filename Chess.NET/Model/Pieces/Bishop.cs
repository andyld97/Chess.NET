namespace Chess.NET.Model.Pieces
{
    public class Bishop : Piece
    {
        public override PieceType Type => PieceType.Bishop;

        public override int MaterialValue => 3;

        public Bishop(Position position, PieceColor color) : base(position, color)
        {

        }
        
        public override List<Position> GetPossibleMoves(IBoard board)
        {
            var moves = new List<Position>();   
            AddRay(moves, board, 1, 1); // diagonal
            AddRay(moves, board, 1, -1);
            AddRay(moves, board, -1, 1);
            AddRay(moves, board, -1, -1);

            return moves;
        }
    }
}