
using Microsoft.Xrm.Sdk;

namespace CustomPlugin
{
    public class SupplierValid : BasePluginClass
    {
        public override async Task ExecuteDataversePluginAsync(IServiceProvider serviceProvider)
        {

            // Get the execution context
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));



            try
            {

                tracingService.Trace("SupplierValid plugin executedv2");
                if (context.MessageName.ToLower() == "create" || context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        Entity target = (Entity)context.InputParameters["Target"];

                        if (target.LogicalName == "zzz_supplier")
                        {
                            if (target.Contains("zzz_suppliername"))
                            {


                                string name = (string)target["zzz_suppliername"];
                                string oldName = (string)context.PreEntityImages["preSupplier"]["zzz_suppliername"];
                                if (context.MessageName.ToLower() == "update" && name.StartsWith(oldName))
                                {
                                    throw new InvalidPluginExecutionException("old name can not include with 'old'");
                                }

                                if (name.StartsWith("11"))
                                {
                                    tracingService.Trace("Supplier name is invalid1");
                                    throw new InvalidPluginExecutionException("Supplier name can not start with '11'");
                                }

                                tracingService.Trace("Supplier name is valid");
                                // await Task.Delay(10000);
                            }
                            else
                            {
                                throw new InvalidPluginExecutionException("Supplier name is required");
                            }
                        }
                    }

                    // Check if zzz_suppliername exists and get its value

                }
                else
                {
                    tracingService.Trace("SupplierValid plugin executed" + context.MessageName);
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
