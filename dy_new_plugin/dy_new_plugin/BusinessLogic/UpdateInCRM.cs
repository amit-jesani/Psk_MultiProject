using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dy_new_plugin.BusinessLogic
{
    public class UpdateInCRM
    {
        private Entity _target;
        private PluginContext<Entity> _context;
        private Entity _preImage;
        public UpdateInCRM(PluginContext<Entity> context)
        {
            _context = context;
            _target = context.Target;
            _preImage = context.GetPreImage("PreImage");
        }
        public void updateaccount()
        {
            if (_target.LogicalName == "account" && _context.MessageName == "Update")
            {
                try
                {
                    EntityReference contactEntityRef = _preImage.GetValue<EntityReference>("primarycontactid", null);
                    //string nm = _target.GetValue<String>("name", "patel dv");
                    string nm = (_target.Contains("name") ? _target.GetValue<string>("name", "dv patel") : _preImage.GetValue<String>("name", "vd patel"));
                    string[] nameParts = nm.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    Entity ContactEntity = _context.Retrieve(contactEntityRef, new string[] { "fax" });
                    ContactEntity["fax"] = (_target.Contains("fax") ? _target["fax"] : _preImage["fax"]);
                    ContactEntity["firstname"] = nameParts[0];
                    ContactEntity["lastname"] = nameParts[1];
                    ContactEntity["mobilephone"] = (_target.Contains("telephone1") ? _target["telephone1"] : _preImage["telephone1"]);
                    ContactEntity["emailaddress1"] = (_target.Contains("emailaddress1") ? _target["emailaddress1"] : _preImage["emailaddress1"]);
                    ContactEntity["websiteurl"] = (_target.Contains("websiteurl") ? _target["websiteurl"] : _preImage["websiteurl"]);
                    ContactEntity["jobtitle"] = "CRM";

                    _context.Service.Update(ContactEntity);
                }

                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
            else
            {
                throw new InvalidPluginExecutionException("This plugin is only for account entity and contact can't be created");
            }
        }

    }
}
