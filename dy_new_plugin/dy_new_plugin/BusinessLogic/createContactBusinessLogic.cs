using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dy_new_plugin;
using System.Runtime.Remoting.Services;
namespace dy_new_plugin.BusinessLogic
{
    public class createContactBusinessLogic
    {
        private Entity _target;
        private PluginContext<Entity> _context;
        private Entity _preImage;
        public createContactBusinessLogic(PluginContext<Entity> context) 
        {
            _context = context;
            _target = context.Target;
            _preImage = context.GetPreImage("PreImage");
        }

        public void createContact()
        {
            //write a code to create a contact when the account is created with the same name and all attribute values
            if (_target.LogicalName == "account" && _context.MessageName == "Create")
            {
                try
                {
                    Entity contact = new Entity("contact");


                    string nm = _target.GetValue<String>("name", "patel dv");
                    string[] nameParts = nm.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    contact["firstname"] = nameParts[0];
                    contact["lastname"] = nameParts[1];
                    contact["mobilephone"] = _target["telephone1"];
                    contact["fax"] = _target["fax"];
                    contact["emailaddress1"] = _target["emailaddress1"];
                    contact["websiteurl"] = _target["websiteurl"];
                    contact["jobtitle"] = "CRM";
                    // contact["customertypecode"] = new OptionSetValue(Convert.ToInt32(_target.GetAttributeValue<string>("customertypecode"))); // Assuming customertypecode is a picklist field
                    contact["createdon"] = _target["createdon"];



                    Guid contactId = _context.Service.Create(contact);

                    if (contactId != Guid.Empty)
                    {

                        EntityReference contactRef = new EntityReference("contact", contactId);
                        _target["primarycontactid"] = contactRef;
                    }
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
        public void updateaccount() {
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
        public void deleteaccount() {
            if (_target.LogicalName == "account" && _context.MessageName == "Delete")
            {
                try
                {
                    EntityReference contactEntityRef = _preImage.GetValue<EntityReference>("primarycontactid", null);
                    if (contactEntityRef != null)
                    {
                        _context.Service.Delete(contactEntityRef.LogicalName, contactEntityRef.Id);
                    }
                }

                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }

        }
            
     }
 }

