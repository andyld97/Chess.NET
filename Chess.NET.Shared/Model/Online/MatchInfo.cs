namespace Chess.NET.Shared.Model.Online
{
    public class MatchInfo
    {
        public string MatchId { get; set; } = string.Empty;

        public string OpponentName { get; set; } = string.Empty;

        public string OpponentElo { get; set; } = string.Empty;

        public Color OpponentColor { get; set; }

        public Color ClientColor {  get; set; }
    }
}