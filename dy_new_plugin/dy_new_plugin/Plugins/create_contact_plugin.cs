using dy_new_plugin.BusinessLogic;
using Microsoft.Xrm.Sdk;
using System;
namespace dy_new_plugin
{
    public class create_contact_plugin : PluginBase, IPlugin
    {
       new  public  void Execute(IServiceProvider serviceProvider)
        {
            base.Execute(serviceProvider);
        }

        public override void OnExecute(PluginContext<Entity> context)
        {
                var navu = new createContactBusinessLogic(context);
            if (context.MessageName == "Create")
            {
                navu.createContact();
            }
            if(context.MessageName == "Update")
            {
                navu.updateaccount();
               
            }
            if(context.MessageName == "Delete")
            {
                navu.deleteaccount();
            }

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
