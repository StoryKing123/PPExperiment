
using Microsoft.Xrm.Sdk;

namespace CustomPlugin
{
    public class SupplierPreAndPostValid : BasePluginClass
    {
        public override async Task ExecuteDataversePluginAsync(IServiceProvider serviceProvider)
        {

            // Get the execution context
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));



            try
            {

                tracingService.Trace("log trigger event");
                if (context.InputParameters.Contains("Target") && //Is a message with Target
                         context.InputParameters["Target"] is Entity && //Target is an entity
                         ((Entity)context.InputParameters["Target"]).LogicalName.Equals("zzz_supplier") && //Target is an account
                         ((Entity)context.InputParameters["Target"])["zzz_suppliername"] != null && //account name is passed
                         context.MessageName.Equals("Update") && //Message is Update
                         context.PreEntityImages["a"] != null && //PreEntityImage with alias 'a' included with step
                         context.PreEntityImages["a"]["zzz_suppliername"] != null) //account name included with PreEntityImage with step
                {

                    var entity = (Entity)context.InputParameters["Target"];
                    var newName = (string)entity["zzz_suppliername"];
                    var oldName = (string)context.PreEntityImages["a"]["zzz_suppliername"];
                    if (newName.Contains("new") && oldName.Contains("old"))
                    {
                        throw new InvalidPluginExecutionException("newname include new and oldname inclue old");
                    }

                }

            }
            catch (Exception ex)
            {
                tracingService.Trace("Error: " + ex.Message);
                throw new InvalidPluginExecutionException("An error occurred in the plugin: " + ex.Message);
            }


        }

    }


}