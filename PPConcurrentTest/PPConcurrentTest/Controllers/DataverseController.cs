using Microsoft.AspNetCore.Mvc;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
// using PPConcurrentTest.Services;

namespace PPConcurrentTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataverseController : ControllerBase
    {
        private readonly ILogger<DataverseController> _logger;
        // private readonly DataverseClientService _dataverseService;
             private readonly ServiceClient _serviceClient;


        public DataverseController(
            ILogger<DataverseController> logger,
            DataverseClientService dataverseService)
        {
            _logger = logger;
            _serviceClient = dataverseService.Client;

        }

        [HttpGet("records/{entityName}")]
        public async Task<IActionResult> GetRecords(string entityName, [FromQuery] int top = 10)
        {
            try
            {
                var query = new QueryExpression(entityName)
                {
                    ColumnSet = new ColumnSet(true),
                    TopCount = top
                };

                var results = await _serviceClient.RetrieveMultipleAsync(query);
                return Ok(results.Entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving records");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("records/{entityName}")]
        public  IActionResult CreateRecord(
            string entityName,
            [FromBody] Dictionary<string, object> attributes)
        {
            try
            {
                return Ok("123123");
                // var result = await _serviceClient.CreateRecordAsync(entityName, attributes);
                // return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating record");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}