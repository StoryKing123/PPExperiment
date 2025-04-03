using Microsoft.PowerPlatform.Dataverse.Client;

public class DataverseClientService
{
    private readonly ServiceClient _serviceClient;
    private readonly ILogger<DataverseClientService> _logger;

    public DataverseClientService(ILogger<DataverseClientService> logger)
    {


        // Bump up the min threads reserved for this app to ramp connections faster - minWorkerThreads defaults to 4, minIOCP defaults to 4 
        // ThreadPool.SetMinThreads(100, 100);
        // Change max connections from .NET to a remote service default: 2
        // System.Net.ServicePointManager.DefaultConnectionLimit = 65000;
        // Turn off the Expect 100 to continue message - 'true' will cause the caller to wait until it round-trip confirms a connection to the server 
        // System.Net.ServicePointManager.Expect100Continue = false;
        // Can decrease overall transmission overhead but can cause delay in data packet arrival
        // System.Net.ServicePointManager.UseNagleAlgorithm = false;

        _logger = logger;
        string connectionString = "AuthType=ClientSecret;Url=https://org401d6a6f.crm5.dynamics.com;ClientId=1bdca875-081c-4ab9-b53a-504c0498a799;ClientSecret=Omo8Q~MPCiVhI4vfz4-zuhTxByxSZjLyeou.WaGR";



        try
        {
            _serviceClient = new ServiceClient(connectionString);
            _serviceClient.EnableAffinityCookie = false;
            // 立即触发一个简单的查询来强制认证
            InitializeConnection().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ServiceClient");
            throw;
        }
    }

    private async Task InitializeConnection()
    {
        if (_serviceClient.IsReady)
        {
            try
            {
                // 执行一个简单的查询来触发认证
                var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("zzz_test")
                {
                    TopCount = 1
                };
                await _serviceClient.RetrieveMultipleAsync(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing connection");
                throw;
            }
        }
    }

    public ServiceClient Client => _serviceClient;
}

