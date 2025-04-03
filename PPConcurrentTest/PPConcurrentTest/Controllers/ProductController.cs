
using Microsoft.AspNetCore.Mvc;
using PPConcurrentTest.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Bogus;
using Microsoft.PowerPlatform.Dataverse.Client;
using Newtonsoft.Json;

namespace PPConcurrentTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IProductService _productService;

        private readonly ServiceClient _serviceClient;


        public ProductController(ILogger<ProductController> logger, IProductService productService, DataverseClientService dataverseService)
        {
            _serviceClient = dataverseService.Client;
            _logger = logger;
            _productService = productService;
        }

        [HttpGet("insert")]
        public async Task<IActionResult> InsertProduct()
        {
            try
            {
                var result = await _productService.CreateProductAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InsertProduct endpoint");
                return StatusCode(500, "An error occurred while creating the product");
            }
        }

        [HttpGet("batchinsert")]
        public async Task<IActionResult> BatchInsertProduct()
        {
            try
            {

                var result = await _productService.BatchCreateProductsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BatchInsertProduct endpoint");
                return StatusCode(500, "An error occurred while batch creating products");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProduct endpoint");
                return StatusCode(500, "An error occurred while retrieving the product");
            }
        }


        [HttpGet("update")]
        public async Task<object> UpdateCateogry()
        {

            var request = new OrganizationRequest("zzz_UpdateProductCategory");
            // 设置输入参数（如果有）
            request["ID"] = "0c7371c1-ab0f-f011-998a-002248ee44e0";
            // 调用操作
            var response = _serviceClient.Execute(request);
            // 获取返回值（如果有）
            var outputValue = response["Res"];
            Console.WriteLine($"Output: {outputValue}");
            return Ok($"successful update to :{outputValue}");
        }

        [HttpGet("get")]
        public async Task<object> QueryProduct()
        {

            return  JsonConvert.SerializeObject(await  _productService.GetProudct());

        }
    }
}
