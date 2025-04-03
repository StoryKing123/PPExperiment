using Microsoft.Xrm.Sdk;

namespace CustomPlugin
{
    public  abstract class BasePluginClass : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
             ExecuteDataversePluginAsync(serviceProvider).GetAwaiter().GetResult();
            // throw new NotImplementedException();
        }

        public virtual async Task ExecuteDataversePluginAsync(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }

}


