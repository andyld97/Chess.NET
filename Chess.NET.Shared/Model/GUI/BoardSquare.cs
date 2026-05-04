namespace Chess.NET.Shared.Model.GUI
{
    public class BoardSquare<T>
    {
        public int File { get; }

        public int Rank { get; }

        public T Border { get; }

        public BoardSquare(int file, int rank, T border)
        {
            File = file;
            Rank = rank;
            Border = border;
        }
    }
}
