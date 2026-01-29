using System.Text.Json.Serialization;

namespace Chess.NET.Shared.Model.Online
{
    public class MoveMade
    {
        [JsonPropertyName("move")]
        public string Move { get; set; } = string.Empty;

        [JsonPropertyName("color")]
        public Color Color { get; set; }
    }
}
