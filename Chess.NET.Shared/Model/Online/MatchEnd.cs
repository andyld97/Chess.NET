namespace Chess.NET.Shared.Model.Online
{
    public class MatchEnd
    {
        public Color? ColorWins { get; set; } = null;

        public GameResult Result { get; set; }
    }
}
