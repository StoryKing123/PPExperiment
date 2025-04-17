using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PPConcurrentTest.Models;
using Bogus;
using Microsoft.Xrm.Sdk.Messages;
using System.Net.Http.Headers;

namespace PPConcurrentTest.Services
{
    public class ProductService : IProductService
    {
        private readonly ServiceClient _serviceClient;
        private readonly DataverseClientPool _clientPool;
        private readonly ILogger<ProductService> _logger;

        public ProductService(DataverseClientService dataverseService, ILogger<ProductService> logger, DataverseClientPool clientPool)
        {
            _serviceClient = dataverseService.Client;
            _logger = logger;
            _clientPool = clientPool;
        }

        public async Task<Guid> CreateProductAsync()
        {
            string logicalName = Product.EntityLogicalName;
            var newRecord = new Entity { LogicalName = logicalName };
            var faker = new Faker();

            try
            {
                // Add all zzz_ fields with faker data
                newRecord[Product.ZZZ_ProductName] = faker.Commerce.ProductName();
                newRecord[Product.ZZZ_Description] = faker.Commerce.ProductDescription();
                newRecord[Product.ZZZ_Category] = new OptionSetValue(993700000);
                newRecord[Product.ZZZ_Color] = faker.Commerce.Color();
                newRecord[Product.ZZZ_Price] = new Money(decimal.Parse(faker.Commerce.Price()));
                newRecord[Product.ZZZ_IsAvailable] = faker.Random.Bool();
                newRecord[Product.ZZZ_Rating] = faker.Random.Int(1, 5);
                newRecord[Product.ZZZ_StockQuantity] = faker.Random.Int(0, 1000);
                newRecord[Product.ZZZ_Weight] = faker.Random.Decimal(1, 100);
                newRecord[Product.ZZZ_WarrantyPeriod] = faker.Random.Int(0, 60);
                newRecord[Product.ZZZ_ReleasedAte] = faker.Date.Past();

                // Query a random supplier
                var supplierQuery = new QueryExpression(Supplier.EntityLogicalName)
                {
                    ColumnSet = new ColumnSet(Supplier.Id)
                };

                var suppliers = await _serviceClient.RetrieveMultipleAsync(supplierQuery);
                if (suppliers.Entities.Any())
                {
                    newRecord[Product.ZZZ_SupplierId] = new EntityReference(
                        Supplier.EntityLogicalName,
                        suppliers.Entities[faker.Random.Int(0, suppliers.Entities.Count - 1)].Id
                    );
                }

                return await _serviceClient.CreateAsync(newRecord);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product record");
                throw;
            }
        }

        public async Task<Entity> GetProductByIdAsync(Guid id)
        {
            try
            {
                return await _serviceClient.RetrieveAsync(Product.EntityLogicalName, id, new ColumnSet(true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product");
                throw;
            }
        }

        public async Task<ExecuteMultipleResponse> BatchCreateProductsAsync(ExecuteMultipleRequest request)
        {
            try
            {
                return (ExecuteMultipleResponse)await _serviceClient.ExecuteAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch creating products");
                throw;
            }
        }

        public async Task<ExecuteMultipleResponse> BatchCreateProductsAsync()
        {
            var multipleRequest = new ExecuteMultipleRequest()
            {
                Settings = new ExecuteMultipleSettings()
                {
                    ContinueOnError = true,
                    ReturnResponses = true
                },
                Requests = new OrganizationRequestCollection()
            };
            var supplierQuery = new QueryExpression(Supplier.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(Supplier.Id)
            };

            var suppliers = await _serviceClient.RetrieveMultipleAsync(supplierQuery);

            for (int i = 0; i < 1000; i++)
            {
                var newRecord = new Entity { LogicalName = Product.EntityLogicalName };
                var faker = new Faker();

                newRecord[Product.ZZZ_ProductName] = faker.Commerce.ProductName();
                newRecord[Product.ZZZ_Description] = faker.Commerce.ProductDescription();
                newRecord[Product.ZZZ_Category] = new OptionSetValue(993700000);
                newRecord[Product.ZZZ_Color] = faker.Commerce.Color();
                newRecord[Product.ZZZ_Price] = new Money(decimal.Parse(faker.Commerce.Price()));
                newRecord[Product.ZZZ_IsAvailable] = faker.Random.Bool();
                newRecord[Product.ZZZ_Rating] = faker.Random.Int(1, 5);
                newRecord[Product.ZZZ_StockQuantity] = faker.Random.Int(0, 1000);
                newRecord[Product.ZZZ_Weight] = faker.Random.Decimal(1, 100);
                newRecord[Product.ZZZ_WarrantyPeriod] = faker.Random.Int(0, 60);
                newRecord[Product.ZZZ_ReleasedAte] = faker.Date.Past();

                if (suppliers.Entities.Any())
                {
                    newRecord[Product.ZZZ_SupplierId] = new EntityReference(
                        Supplier.EntityLogicalName,
                        suppliers.Entities[faker.Random.Int(0, suppliers.Entities.Count - 1)].Id
                    );
                }

                var createRequest = new CreateRequest { Target = newRecord };
                multipleRequest.Requests.Add(createRequest);
            }
            var res = (ExecuteMultipleResponse)await _serviceClient.ExecuteAsync(multipleRequest);
            return res;

        }

        public async Task<object> GetProudct()
        {

            var productQuery = new QueryExpression(Product.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(true)
            };

            // Add link to Supplier entity
            productQuery.LinkEntities.Add(new LinkEntity
            {
                LinkFromEntityName = Product.EntityLogicalName,
                LinkToEntityName = Supplier.EntityLogicalName,
                LinkFromAttributeName = Product.ZZZ_SupplierId,
                LinkToAttributeName = Supplier.Id,
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet(Supplier.ZZZ_SupplierName),
                EntityAlias = "supplier"
            });
            var products = await _clientPool.ExecuteWithClientAsync(async client =>
            {

                Console.WriteLine("got client and start query");

                string logicalName = Product.EntityLogicalName;
                var newRecord = new Entity { LogicalName = logicalName };
                var faker = new Faker();


                // Add all zzz_ fields with faker data
                newRecord[Product.ZZZ_ProductName] = faker.Commerce.ProductName();
                newRecord[Product.ZZZ_Description] = faker.Commerce.ProductDescription();
                newRecord[Product.ZZZ_Category] = new OptionSetValue(993700000);
                newRecord[Product.ZZZ_Color] = faker.Commerce.Color();
                newRecord[Product.ZZZ_Price] = new Money(decimal.Parse(faker.Commerce.Price()));
                newRecord[Product.ZZZ_IsAvailable] = faker.Random.Bool();
                newRecord[Product.ZZZ_Rating] = faker.Random.Int(1, 5);
                newRecord[Product.ZZZ_StockQuantity] = faker.Random.Int(0, 1000);
                newRecord[Product.ZZZ_Weight] = faker.Random.Decimal(1, 100);
                newRecord[Product.ZZZ_WarrantyPeriod] = faker.Random.Int(0, 60);
                newRecord[Product.ZZZ_ReleasedAte] = faker.Date.Past();

                // Query a random supplier
                var supplierQuery = new QueryExpression(Supplier.EntityLogicalName)
                {
                    ColumnSet = new ColumnSet(Supplier.Id)
                };

                var suppliers = await client.RetrieveMultipleAsync(supplierQuery);
                if (suppliers.Entities.Any())
                {
                    newRecord[Product.ZZZ_SupplierId] = new EntityReference(
                        Supplier.EntityLogicalName,
                        suppliers.Entities[faker.Random.Int(0, suppliers.Entities.Count - 1)].Id
                    );
                }

                await client.CreateAsync(newRecord);
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("https://org401d6a6f.crm5.dynamics.com/api/data/v9.2/");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", client.CurrentAccessToken);
                // 发送请求
                HttpResponseMessage response = await httpClient.GetAsync("WhoAmI");
                // 查看响应 header
                foreach (var header in response.Headers)
                {
                    Console.WriteLine($"{header.Key}: {string.Join(",", header.Value)}");
                }




                var res = await client.RetrieveMultipleAsync(productQuery);

                return res;

            });




            // string logicalName = Product.EntityLogicalName;
            // var newRecord = new Entity { LogicalName = logicalName };
            // var faker = new Faker();


            // // Add all zzz_ fields with faker data
            // newRecord[Product.ZZZ_ProductName] = faker.Commerce.ProductName();
            // newRecord[Product.ZZZ_Description] = faker.Commerce.ProductDescription();
            // newRecord[Product.ZZZ_Category] = new OptionSetValue(993700000);
            // newRecord[Product.ZZZ_Color] = faker.Commerce.Color();
            // newRecord[Product.ZZZ_Price] = new Money(decimal.Parse(faker.Commerce.Price()));
            // newRecord[Product.ZZZ_IsAvailable] = faker.Random.Bool();
            // newRecord[Product.ZZZ_Rating] = faker.Random.Int(1, 5);
            // newRecord[Product.ZZZ_StockQuantity] = faker.Random.Int(0, 1000);
            // newRecord[Product.ZZZ_Weight] = faker.Random.Decimal(1, 100);
            // newRecord[Product.ZZZ_WarrantyPeriod] = faker.Random.Int(0, 60);
            // newRecord[Product.ZZZ_ReleasedAte] = faker.Date.Past();

            // // Query a random supplier
            // var supplierQuery = new QueryExpression(Supplier.EntityLogicalName)
            // {
            //     ColumnSet = new ColumnSet(Supplier.Id)
            // };

            // var suppliers = await _serviceClient.RetrieveMultipleAsync(supplierQuery);
            // if (suppliers.Entities.Any())
            // {
            //     newRecord[Product.ZZZ_SupplierId] = new EntityReference(
            //         Supplier.EntityLogicalName,
            //         suppliers.Entities[faker.Random.Int(0, suppliers.Entities.Count - 1)].Id
            //     );
            // }

            // await _serviceClient.CreateAsync(newRecord);
            // var products = await _serviceClient.RetrieveMultipleAsync(productQuery);

            var productList = products.Entities.Select(entity => new
            {
                Id = entity.Id,
                ProductName = entity.GetAttributeValue<string>(Product.ZZZ_ProductName),
                Description = entity.GetAttributeValue<string>(Product.ZZZ_Description),
                Category = entity.GetAttributeValue<OptionSetValue>(Product.ZZZ_Category)?.Value,
                Color = entity.GetAttributeValue<string>(Product.ZZZ_Color),
                Price = entity.GetAttributeValue<Money>(Product.ZZZ_Price)?.Value,
                IsAvailable = entity.GetAttributeValue<bool>(Product.ZZZ_IsAvailable),
                Rating = entity.GetAttributeValue<int>(Product.ZZZ_Rating),
                StockQuantity = entity.GetAttributeValue<int>(Product.ZZZ_StockQuantity),
                Weight = entity.GetAttributeValue<decimal>(Product.ZZZ_Weight),
                WarrantyPeriod = entity.GetAttributeValue<int>(Product.ZZZ_WarrantyPeriod),
                ReleasedDate = entity.GetAttributeValue<DateTime>(Product.ZZZ_ReleasedAte),
                SupplierId = entity.GetAttributeValue<EntityReference>(Product.ZZZ_SupplierId)?.Id,
                SupplierName = entity.GetAttributeValue<AliasedValue>("supplier." + Supplier.ZZZ_SupplierName)?.Value as string
            }).ToList();

            return productList;
            // throw new NotImplementedException();
        }
    }
}



