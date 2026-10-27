namespace Chess.NET.Shared
{
    public sealed record PlayerDisplay(string TopName, string TopElo, string TopInfo, TimeSpan TopTime, string BottomName, string BottomElo, string BottomInfo, TimeSpan TimeBottom);

    public static class PlayerDisplayHelper
    {
        public static string FormatElo(int? value) => value == null ? "Elo: ????" : $"Elo: {value:D4}";

        public static string FormatElo(string? value) => string.IsNullOrEmpty(value) ? "Elo: ????" : $"Elo: {value}";

        public static PlayerDisplay GetPlayerDisplay(string ownName, string ownElo, string otherName, string otherElo, bool isMirrored, bool ownIsBlack, string whiteInfo, string blackInfo, TimeSpan timeWhite, TimeSpan timeBlack)
        {
            // The opponent is on top by default; mirroring the board or playing black flips it
            bool otherOnTop = isMirrored == ownIsBlack;

            var (topName, topElo) = otherOnTop ? (otherName, otherElo) : (ownName, ownElo);
            var (bottomName, bottomElo) = otherOnTop ? (ownName, ownElo) : (otherName, otherElo);

            return new PlayerDisplay(topName, topElo, isMirrored ? whiteInfo : blackInfo, isMirrored ? timeWhite : timeBlack, bottomName, bottomElo, isMirrored ? blackInfo : whiteInfo, isMirrored ? timeBlack : timeWhite);
        }
    }
}