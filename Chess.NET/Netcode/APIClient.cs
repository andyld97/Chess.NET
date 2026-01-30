using Chess.NET.Shared.Model.Online;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Chess.NET.Netcode
{
    public static class APIClient
    {
        private static HttpClient httpClient = new HttpClient();  

        public static async Task JoinQueueAsync(Client client)
        {
            var content = new StringContent(JsonSerializer.Serialize(client), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Consts.SERVER_URL}/queue/join", content);
            response.EnsureSuccessStatusCode();
        }

        public static async Task MakeMoveAsync(Match match, string move)
        {
            var content = new StringContent(JsonSerializer.Serialize(move), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Consts.SERVER_URL}/game/{match.MatchId}/{SignalRClient.CLIENT_ID}/MakeMove", content);
            response.EnsureSuccessStatusCode();
        }
    }
}
