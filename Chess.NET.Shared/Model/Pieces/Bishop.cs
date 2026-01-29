namespace Chess.NET.Shared.Model.Pieces
{
    public class Bishop : Piece
    {
        public override PieceType Type => PieceType.Bishop;

        public override int MaterialValue => 3;

        public Bishop(Position position, Color color) : base(position, color)
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

        public override object Clone()
        {
            return new Bishop((Position)Position.Clone(), Color);
        }
    }
}