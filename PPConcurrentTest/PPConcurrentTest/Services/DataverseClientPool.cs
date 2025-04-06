using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;



/**
 * 1. 通过连接池管理多个Dataverse客户端连接，以提高并发性能。
 * 2. 使用SemaphoreSlim来限制并发连接数，避免同时创建过多连接导致性能问题。
 * 3. 在执行操作时，从连接池中获取可用的客户端连接，并在操作完成后释放回连接池。
 * 4. 如果客户端连接出现异常（如认证失败、连接超时等），则尝试重新创建连接。
 */
public class DataverseClientPool : IDisposable
{
    private readonly ILogger<DataverseClientPool> _logger;
    private readonly ConcurrentBag<ServiceClient> _clientPool;
    private readonly SemaphoreSlim _semaphore;
    private readonly List<ClientConfiguration> _clientConfigurations;
    private bool _disposed = false;

    public class ClientConfiguration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Url { get; set; }
    }

    public DataverseClientPool(ILogger<DataverseClientPool> logger, int maxConcurrentConnections = 4)
    {
        _logger = logger;
        _clientPool = new ConcurrentBag<ServiceClient>();
        _semaphore = new SemaphoreSlim(maxConcurrentConnections, maxConcurrentConnections);

        // Configure the client credentials
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
        // System.Net.ServicePointManager.DefaultConnectionLimit = 65000;
        //Bump up the min threads reserved for this app to ramp connections faster - minWorkerThreads defaults to 4, minIOCP defaults to 4
        // System.Threading.ThreadPool.SetMinThreads(100, 100);
        //Turn off the Expect 100 to continue message - 'true' will cause the caller to wait until it round-trip confirms a connection to the server
        // System.Net.ServicePointManager.Expect100Continue = false;
        //Can decrease overall transmission overhead but can cause delay in data packet arrival
        // System.Net.ServicePointManager.UseNagleAlgorithm = false;
        // Optimize network settings
        // System.Net.ServicePointManager.DefaultConnectionLimit = 65000;
        // System.Net.ServicePointManager.Expect100Continue = false;
        // System.Net.ServicePointManager.UseNagleAlgorithm = false;
        // ThreadPool.SetMinThreads(100, 100);

        InitializePool(maxConcurrentConnections).GetAwaiter().GetResult();
    }

    private async Task InitializePool(int maxConnections)
    {
        try
        {
            _logger.LogInformation("Initializing Dataverse client pool with {0} connections", maxConnections);

            // Create connections evenly distributed between available client configurations
            for (int i = 0; i < maxConnections; i++)
            {
                var config = _clientConfigurations[i % _clientConfigurations.Count];
                var connectionString = $"AuthType=ClientSecret;Url={config.Url};ClientId={config.ClientId};ClientSecret={config.ClientSecret}";

                ServiceClient.MaxConnectionTimeout = TimeSpan.FromMinutes(5);
                var client = new ServiceClient(connectionString);
                // client. = TimeSpan.FromSeconds(10);
                client.EnableAffinityCookie = false;

                // Initialize connection
                await ValidateConnection(client);

                _clientPool.Add(client);
                _logger.LogInformation("Added client to pool. Total: {0}", i + 1);
            }
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
                // Execute a simple query to trigger authentication
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
        timeout ??= TimeSpan.FromSeconds(600);

        if (await _semaphore.WaitAsync(timeout.Value))
        {
            ServiceClient client = null;
            try
            {
                if (_clientPool.TryTake(out client))
                {
                    // Execute the action with the acquired client
                    return await action(client);
                }
                else
                {
                    throw new InvalidOperationException("Failed to acquire client from the pool");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing action with Dataverse client");

                // If there's an authentication or connection issue, recreate the client
                if (ShouldRecreateClient(ex) && client != null)
                {
                    try
                    {
                        client.Dispose();
                        var configIndex = new Random().Next(_clientConfigurations.Count);
                        var config = _clientConfigurations[configIndex];
                        var connectionString = $"AuthType=ClientSecret;Url={config.Url};ClientId={config.ClientId};ClientSecret={config.ClientSecret}";
                        client = new ServiceClient(connectionString);
                        client.EnableAffinityCookie = false;
                        await ValidateConnection(client);
                    }
                    catch (Exception recreateEx)
                    {
                        _logger.LogError(recreateEx, "Failed to recreate client");
                        throw;
                    }
                }
                throw;
            }
            finally
            {
                // Return the client to the pool if it's still valid
                if (client != null && client.IsReady)
                {
                    _clientPool.Add(client);
                }

                _semaphore.Release();
            }
        }
        else
        {
            throw new TimeoutException("Timeout waiting for available Dataverse client");
        }
    }

    private bool ShouldRecreateClient(Exception ex)
    {
        // Add logic to determine if client should be recreated based on exception
        return ex is Microsoft.Xrm.Sdk.SdkExceptionBase ||
               ex.Message.Contains("authentication") ||
               ex.Message.Contains("token") ||
               ex.Message.Contains("timeout") ||
               ex.Message.Contains("connection");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose all clients in the pool
                foreach (var client in _clientPool)
                {
                    client?.Dispose();
                }

                _semaphore?.Dispose();
            }

            _disposed = true;
        }
    }
}
