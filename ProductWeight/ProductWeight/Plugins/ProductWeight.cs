using Microsoft.Xrm.Sdk;
using ProductWeight.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProductWeight
{
    public class ProductWeight : PluginBase, IPlugin
    {
        new public void Execute(IServiceProvider serviceProvider)
        {
            base.Execute(serviceProvider);
        }

        public override void OnExecute(PluginContext<Entity> context)
        {
            var logic = new ProductWeightBusinessLogic(context);

            if (context.MessageName == "Create" && context.LocalContext.Depth == 1)
            {
                logic.CreateProductWeight();
            }

            if (context.MessageName == "Update" && context.LocalContext.Depth == 1)
            { 
                logic.UpdateProductWeight();
            }

            if (context.MessageName == "Delete")
            {
                logic.UpdateNetWeightOnDelete();
            }
        }
    }
}
