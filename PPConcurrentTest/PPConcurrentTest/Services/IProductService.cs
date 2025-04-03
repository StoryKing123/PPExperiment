using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace PPConcurrentTest.Services
{
    public interface IProductService
    {
        Task<Guid> CreateProductAsync();
        Task<Entity> GetProductByIdAsync(Guid id);
        Task<ExecuteMultipleResponse> BatchCreateProductsAsync();

        Task<object> GetProudct();
    }
}
