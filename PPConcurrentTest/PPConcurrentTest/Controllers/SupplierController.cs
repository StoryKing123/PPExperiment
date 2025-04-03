
using Microsoft.AspNetCore.Mvc;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.PowerPlatform.Dataverse.Client.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using PPConcurrentTest.Models;
using Bogus;

namespace PPConcurrentTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SupplierController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly ServiceClient _serviceClient;
        public SupplierController(ILogger<ProductController> logger, DataverseClientService dataverseService)
        {
            _logger = logger;
            _serviceClient = dataverseService.Client;
        }


        [HttpGet("insert")]
        public object InsertSupplier()
        {
            // var newSupplier = new Dictionary<string,object>{};
            string logicalName = Supplier.EntityLogicalName;
            var newRecord = new Entity { LogicalName = logicalName };
            newRecord[Supplier.ZZZ_SupplierName] = "supplier122";
            try
            {

                var result = _serviceClient.Create(newRecord);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error creating supplier record: {ex.Message}");
                _logger.LogError(ex, "Error creating supplier record");
                return Ok("system error");
            }
        }



        [HttpGet("batchinsert")]
        public object BatchInsertSupplier()
        {
            // var newSupplier = new Dictionary<string,object>{};
            string logicalName = Supplier.EntityLogicalName;
            // var batchRequest = new OrganizationRequestCollection();
            var multipleRequest = new ExecuteMultipleRequest()
            {
                Settings = new ExecuteMultipleSettings()
                {
                    ContinueOnError = true,
                    ReturnResponses = true
                },
                Requests = new OrganizationRequestCollection()
            };


            try
            {
                for (int i = 0; i < 100; i++)
                {

                    var newRecord = new Entity { LogicalName = logicalName };
                    var faker = new Bogus.Faker();
                    newRecord[Supplier.ZZZ_SupplierName] = faker.Company.CompanyName();
                    newRecord[Supplier.ZZZ_Phone] = faker.Phone.PhoneNumber();
                    newRecord[Supplier.ZZZ_ContactEmail] = faker.Internet.Email();
                    var newRequest = new CreateRequest { Target = newRecord };
                    multipleRequest.Requests.Add(newRequest);

                }

                var result = _serviceClient.Execute(multipleRequest);
                return Ok("success");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error creating supplier record: {ex.Message}");
                _logger.LogError(ex, "Error creating supplier record");
                return Ok("system error");
            }
        }
    }
}
