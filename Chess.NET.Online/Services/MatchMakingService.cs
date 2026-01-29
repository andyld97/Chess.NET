using Chess.NET.Shared.Model.Online;

namespace Chess.NET.Online.Services
{
    public interface IMatchMakingService
    {
        Match? Join(Client client);

        void Leave(string clientId);
    }

    public class MatchMakingService : IMatchMakingService
    {
        private readonly ILogger<IMatchMakingService> _logger;

        private List<Client> clients = [];
        private object sync = new object();

        public MatchMakingService(ILogger<IMatchMakingService> logger)
        {
            _logger = logger;
        }

        public Match? Join(Client client)
        {
            lock (sync)
            {
                clients.Add(client);

                _logger.LogInformation($"New client enqueued. ID: {client.ClientID} | Name: {client.PlayerName}");

                if (clients.Count >= 2)
                {
                    var client1 = clients[0];
                    var client2 = clients[1];

                    clients.Remove(client1);
                    clients.Remove(client2);

                    // Match found
                    return new Match()
                    {
                        MatchId = Guid.NewGuid().ToString(),
                        ClientWhite = client1,
                        ClientBlack = client2 // TODO: assign random
                    };
                }

                return null; // no match
            }
        }

        public void Leave(string clientId)
        {
            lock (sync)
            {
                var item = clients.FirstOrDefault(c => c.ClientID == clientId);
                if (item != null)
                    clients.Remove(item);
            }
        }
    }
}
