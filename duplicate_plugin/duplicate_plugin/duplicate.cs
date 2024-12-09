using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace duplicate_plugin
{
    public class duplicate : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service and execution context
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.MessageName.ToLower() == "create" && context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity targetEntity = (Entity)context.InputParameters["Target"];

                // Check if the target entity is of type "contact"
                if (targetEntity.LogicalName.ToLower() == "contact")
                {
                    // Check if the "firstname" and "lastname" attributes are present in the target entity
                    if (targetEntity.Attributes.Contains("firstname") && targetEntity.Attributes.Contains("lastname") &&
                        targetEntity["firstname"] != null && targetEntity["lastname"] != null)
                    {
                        string firstName = targetEntity.GetAttributeValue<string>("firstname");
                        string lastName = targetEntity.GetAttributeValue<string>("lastname");

                        // Check if another contact with the same first name and last name already exists
                        if (ContactExistsWithSameName(service, firstName, lastName))
                        {
                            tracingService.Trace($"Contact with the name '{firstName} {lastName}' already exists. Cannot create a duplicate contact.");
                            throw new InvalidPluginExecutionException($"A contact with the name '{firstName} {lastName}' already exists. Cannot create a duplicate contact.");
                        }
                    }
                }
            }
        }
        private bool ContactExistsWithSameName(IOrganizationService service, string firstName, string lastName)
        {
            QueryExpression query = new QueryExpression("contact");
            query.ColumnSet = new ColumnSet(false); // Only fetch the record count, not the full attributes
            query.Criteria.AddCondition("firstname", ConditionOperator.Equal, firstName);
            query.Criteria.AddCondition("lastname", ConditionOperator.Equal, lastName);

            EntityCollection result = service.RetrieveMultiple(query);

            // If there is at least one contact with the same first name and last name, return true; otherwise, return false
            return result.Entities.Count > 0;
        }
    }
}