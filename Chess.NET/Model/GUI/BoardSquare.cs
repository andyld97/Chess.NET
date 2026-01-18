using System.Windows.Controls;

namespace Chess.NET.Model.GUI
{
    public class BoardSquare
    {
        public int File { get; }

        public int Rank { get; }

        public Border Border { get; }

        public BoardSquare(int file, int rank, Border border)
        {
            File = file;
            Rank = rank;
            Border = border;
        }
    }
}
