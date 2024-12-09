using System;
using Microsoft.Xrm.Sdk;
using dy_new_plugin.BusinessLogic;

namespace dy_new_plugin.Plugins
{
    public class CRUDdemoPlugin : PluginBase, IPlugin
    {
        new public void Execute(IServiceProvider serviceProvider)
        {
            base.Execute(serviceProvider);
        }

        public override void OnExecute(PluginContext<Entity> context)
        {
            var demovar = new createInCRM(context);
            if (context.MessageName == "Create")
            {
                demovar.createdeMoFAbS();
            }
            if (context.MessageName == "Update")
            {
                demovar.updateDEmo();
            }
        }
    }
   
}
