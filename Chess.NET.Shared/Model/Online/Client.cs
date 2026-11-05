namespace Chess.NET.Shared.Model.Online
{
    public class Client
    {
        public string ClientID { get; set; } = string.Empty;

        public string PlayerName { get; set; } = string.Empty;

        public string PlayerElo { get; set; } = string.Empty;   

        public TimeControl TimeControl { get; set; } = TimeControl.Rapid10;
    }
}
