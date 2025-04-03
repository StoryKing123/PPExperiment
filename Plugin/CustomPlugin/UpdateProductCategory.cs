using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace CustomPlugin
{

    public class UpdateProductCategory : BasePluginClass
    {
        public override async Task ExecuteDataversePluginAsync(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(null);


            tracingService.Trace($"count:{context.OutputParameters.Count}");
            foreach (var param in context.OutputParameters)
            {
                tracingService.Trace($"Output param - Key: {param.Key}, Value: {param.Value}");
            }

            context.OutputParameters["Res"] = "product Res";
            context.OutputParameters["AAA"] = "result AAA";
            var ID = (string)context.InputParameters["ID"];
            tracingService.Trace("UpdateProductCategory plugin executed" + context.MessageName);
            tracingService.Trace("UpdateProductCategory plugin executed" + ID);
            if (!string.IsNullOrEmpty(ID))
            {
                var product = service.Retrieve("zzz_product", Guid.Parse(ID), new ColumnSet(true));
                var CategoryList = new List<int>
                {
                    993700000,
                    993700001,
                    993700002,
                    993700003
                };
                var newCategory = CategoryList[new Random().Next(0, 4)];
                if (product != null)
                {
                    service.Update(new Entity("zzz_product", Guid.Parse(ID)) { ["zzz_category"] = new OptionSetValue(newCategory) });
                    context.OutputParameters["Res"] = "product updated to :" + newCategory;
                    context.OutputParameters["AAA"] = "result AAA";
                    tracingService.Trace($"count:{context.OutputParameters.Count}");
                    foreach (var param in context.OutputParameters)
                    {
                        tracingService.Trace($"Output param - Key: {param.Key}, Value: {param.Value}");
                    }
                    tracingService.Trace("UpdateProductCategory plugin executed" + newCategory);
                }
                else
                {
                    tracingService.Trace("UpdateProductCategory plugin executed" + "no product found");
                    context.OutputParameters["Res"] = "no product found";
                }

            }
            else
            {
                context.OutputParameters["Res"] = "no product id";

            }
            // throw new NotImplementedException();
        }
    }
}
