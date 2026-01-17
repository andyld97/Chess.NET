namespace Chess.NET.Model
{
    public class Position : IEquatable<Position>, ICloneable
    {
        /// <summary>
        /// The file from A-H
        /// </summary>
        public int File { get; }

        /// <summary>
        /// The rank from 1-8
        /// </summary>
        public int Rank { get; }  

        public Position(int file, int rank)
        {
            if (file < 0 || file > 8 || rank < 0 || rank > 8)
                throw new ArgumentOutOfRangeException();

            File = file;
            Rank = rank;
        }

        public static Position Parse(string pos)
        {
            if (string.IsNullOrWhiteSpace(pos) || pos.Length != 2)
                throw new ArgumentException("Invalid position string", nameof(pos));

            char fileChar = char.ToLowerInvariant(pos[0]);
            char rankChar = pos[1];

            if (fileChar < 'a' || fileChar > 'h')
                throw new ArgumentException("Invalid file (a–h)", nameof(pos));

            if (rankChar < '1' || rankChar > '8')
                throw new ArgumentException("Invalid rank (1–8)", nameof(pos));

            int file = fileChar - 'a' + 1;   // a=1, h=8
            int rank = rankChar - '0';       // '1' -> 1, '8' -> 8

            return new Position(file, rank);
        }

        public Position Mirror()
        {
            return new Position(File, 9 - Rank);
        }

        public override string ToString() => $"{(char)('a' + File - 1)}{Rank}";

        public bool Equals(Position other) => File == other.File && Rank == other.Rank;

        public override bool Equals(object? obj) => obj is Position p && Equals(p);

        public override int GetHashCode() => HashCode.Combine(File, Rank);

        public object Clone()
        {
            return (object)new Position(File, Rank);
        }

        public static bool operator ==(Position left, Position right) => left.Equals(right);

        public static bool operator !=(Position left, Position right) => !left.Equals(right);
    }
}