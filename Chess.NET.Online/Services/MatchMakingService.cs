using Chess.NET.Shared.Model.Online;

namespace Chess.NET.Online.Services
{
    public interface IMatchMakingService
    {
        Match? Join(Client client);

        void Leave(string clientId, string reason);
    }

    public class MatchMakingService : IMatchMakingService
    {
        private readonly ILogger<IMatchMakingService> _logger;

        private readonly List<Client> clients = [];
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

                    List<Client> tmpClients = [client1, client2];
                    bool firstIsWhite = Random.Shared.Next(2) == 0;

                    return new Match()
                    {
                        MatchId = Guid.NewGuid().ToString(),
                        ClientWhite = firstIsWhite ? tmpClients[0] : tmpClients[1],
                        ClientBlack = firstIsWhite ? tmpClients[1] : tmpClients[0]
                    };
                }

                return null; // no match (yet)
            }
        }

        public void Leave(string clientId, string reason)
        {
            lock (sync)
            {
                var item = clients.FirstOrDefault(c => c.ClientID == clientId);
                if (item != null)
                {
                    clients.Remove(item);
                    _logger.LogInformation($"Client [{clientId}] ({item.PlayerName}) leaved waiting queue due to reason: {reason}");
                }
            }
        }
    }
}
