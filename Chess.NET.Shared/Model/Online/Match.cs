using System.Text.Json.Serialization;

namespace Chess.NET.Shared.Model.Online
{
    public class Match
    {
        public string MatchId { get; set; } = string.Empty;

        [JsonIgnore] // a client only should know it's own client id due to security restrictions
        public Client ClientWhite { get; set; } = null!;

        [JsonIgnore] // a client only should know it's own client id due to security restrictions
        public Client ClientBlack { get; set; } = null!;

        [JsonIgnore]
        public Game Game { get; set; } = null!;

        [JsonIgnore]
        public SemaphoreSlim MatchSemaphore { get; } = new SemaphoreSlim(1, 1);

        public Color? GetColorByClientId(string clientId)
        {
            if (ClientWhite.ClientID == clientId)
                return Color.White;
            else if (ClientBlack.ClientID == clientId)
                return Color.Black;

            return null;
        }
    }

    public enum MatchResult
    {
        Checkmate,
        Stalemate,
        Resign,
        TimeOver,
        Disconnected
    }
}
