using System.Text;

namespace Chess.NET.Shared.Model
{
    public class ChessPosition
    {
        public string PiecesHash { get; set; } = string.Empty;

        public Color SideToMove { get; set; }

        public bool WhiteCanCastleKingSide { get; set; }

        public bool WhiteCanCastleQueenSide { get; set; }

        public bool BlackCanCastleKingSide { get; set; }

        public bool BlackCanCastleQueenSide { get; set; }

        public Position? EnPassantSquare { get; set; }

        public string GetHash()
        {
            StringBuilder hashBuilder = new StringBuilder();
            hashBuilder.AppendLine(PiecesHash);
            hashBuilder.AppendLine(SideToMove.ToString());
            hashBuilder.AppendLine(WhiteCanCastleKingSide.ToString());
            hashBuilder.AppendLine(WhiteCanCastleQueenSide.ToString());
            hashBuilder.AppendLine(BlackCanCastleKingSide.ToString());
            hashBuilder.AppendLine(BlackCanCastleQueenSide.ToString());
            if (EnPassantSquare != null)
                hashBuilder.AppendLine(EnPassantSquare.ToString());

            var bytes = System.Text.Encoding.UTF8.GetBytes(hashBuilder.ToString());
            var hash = System.Security.Cryptography.SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}