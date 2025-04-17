

using Microsoft.PowerPlatform.Dataverse.Client;

public class ClientConfiguration
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Url { get; set; }
}


public class DataverseClientPool
{
    private readonly ILogger<DataverseClientPool> _logger;
    private readonly List<ClientConfiguration> _clientConfigurations;

    private readonly List<ServiceClient> _clientList;
    public DataverseClientPool(ILogger<DataverseClientPool> logger, int maxConnections = 4)
    {
        _logger = logger;
        _clientList = new List<ServiceClient>();
        _clientConfigurations = new List<ClientConfiguration>
        {
            new ClientConfiguration
            {
                ClientId = "1bdca875-081c-4ab9-b53a-504c0498a799",
                ClientSecret = "Omo8Q~MPCiVhI4vfz4-zuhTxByxSZjLyeou.WaGR",
                Url = "https://org401d6a6f.crm5.dynamics.com"
            },
            new ClientConfiguration
            {
                ClientId = "2aebfe17-b3ee-4121-89b0-7eb11502b1a8",
                ClientSecret = "x8Z8Q~dgEmAMocjX2K.vAD3zlSM4ylGFEdS5~aNQ",
                Url = "https://org401d6a6f.crm5.dynamics.com"
            }
        };

        InitializePool(maxConnections).GetAwaiter().GetResult();
    }

    private async Task InitializePool(int maxConnections)
    {
        try
        {
            _logger.LogInformation("Initializing Dataverse client pool with {0} connections", maxConnections);

            var tasks = new List<Task>();
            for (int i = 0; i < maxConnections; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var config = _clientConfigurations[i % _clientConfigurations.Count];
                    var connectionString = $"AuthType=ClientSecret;Url={config.Url};ClientId={config.ClientId};ClientSecret={config.ClientSecret}";

                    ServiceClient.MaxConnectionTimeout = TimeSpan.FromMinutes(5);
                    var client = new ServiceClient(connectionString);
                    client.EnableAffinityCookie = false;

                    await ValidateConnection(client);
                    _clientList.Add(client);
                    // _clientPool.Add(client);
                }));
            }

            await Task.WhenAll(tasks);
            _logger.LogInformation("Pool initialized with {0} clients", maxConnections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Dataverse client pool");
            throw;
        }
    }

    private async Task ValidateConnection(ServiceClient client)
    {
        if (client.IsReady)
        {
            try
            {
                var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("zzz_test")
                {
                    TopCount = 1
                };
                await client.RetrieveMultipleAsync(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating connection");
                throw;
            }
        }
    }


    public async Task<T> ExecuteWithClientAsync<T>(Func<ServiceClient, Task<T>> action, TimeSpan? timeout = null)
    {


        var client = _clientList[Random.Shared.Next(_clientList.Count)];

        return await action(client);

    }
}