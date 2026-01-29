using Microsoft.AspNetCore.SignalR;

namespace Chess.NET.Online
{
    public class ClientIdUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.GetHttpContext()?.Request.Query["clientId"];
        }
    }
}