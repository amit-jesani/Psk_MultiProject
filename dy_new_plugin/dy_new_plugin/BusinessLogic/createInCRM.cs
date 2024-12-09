using Microsoft.Xrm.Sdk;
using System;

namespace dy_new_plugin.BusinessLogic
{
    public class createInCRM
    {
        private Entity _target;
        private PluginContext<Entity> _context;
        private Entity _preImage;
        public createInCRM(PluginContext<Entity> context) 
        {
            _context = context;
            _target = context.Target;
            _preImage = context.GetPreImage("PreImage");
        }

        public void createdeMoFAbS()
        {
            //create a demo when the demo_fabs is created 
            if (_target.LogicalName == "dt_demo_fab" && _context.MessageName == "Create")
            {
                if (_context.MessageName == "Create")
                {
                    try
                    {
                        Entity demoentiti = new Entity("dt_demo_2");


                        demoentiti.SetValue("dt_demo_name", _target["dt_demo_name"]);
                        //demoentiti["dt_demo_refernace"] = _target["dt_testparentrefernace"];

                        // set lookup field
                                Guid id = _target.GetValue<EntityReference>("dt_testparentrefernace", null).Id;
                                    demoentiti.SetValue("dt_demo_refernace", new EntityReference("cr4fb_test", id));



                                Guid DEMOid = _context.Service.Create(demoentiti);

                        //if (contactId != Guid.Empty)
                        //{
                        //    EntityReference contactRef = new EntityReference("contact", contactId);
                        //    _target["primarycontactid"] = contactRef;
                        //}
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidPluginExecutionException(ex.Message);
                    }
             }
            }
                 }

        // update demo entity when the demo_fabs is updated
        
        public void updateDEmo()
        {
            if (_target.LogicalName == "dt_demo_fab" && _context.MessageName == "Update")
            {
                try
                {
                    EntityReference DemoEntityRef = _preImage.GetValue<EntityReference>("dt_demo_2id", null);
                    if (DemoEntityRef == null)
                    {
                        throw new InvalidPluginExecutionException("DemoEntityRef is null");
                    }
                    _context.Trace("DemoEntityRef: " + DemoEntityRef.Id);

                    Entity deMOEntity = _context.Retrieve(DemoEntityRef, new string[] { "dt_demo_name" });
                    if (deMOEntity == null)
                    {
                        throw new InvalidPluginExecutionException("deMOEntity is null");
                    }

                    deMOEntity["dt_demo_name"] = (_target.Contains("dt_demo_name") ? _target["dt_demo_name"] : _preImage["dt_demo_name"]);
                    _context.Trace("DemoEntityRef: " + deMOEntity["dt_demo_name"]);

                    //deMOEntity["dt_demo_refernace"] = (_target.Contains("dt_testparentrefernace") ? _target["dt_testparentrefernace"] : _preImage["dt_testparentrefernace"]);

                    // update lookup field using preimage
                    //Guid id = _target.GetAttributeValue<EntityReference>("dt_testparentrefernace").Id;
                    //deMOEntity["dt_demo_refernace"] = new EntityReference("dt_testparentrefernace", id);

                    _context.Service.Update(deMOEntity);
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


        //public void deleteaccount()
        //{
        //    if (_target.LogicalName == "account" && _context.MessageName == "Delete")
        //    {
        //        try
        //        {
        //            EntityReference contactEntityRef = _preImage.GetValue<EntityReference>("primarycontactid", null);
        //            if (contactEntityRef != null)
        //            {
        //                _context.Service.Delete(contactEntityRef.LogicalName, contactEntityRef.Id);
        //            }
        //        }

        //        catch (Exception ex)
        //        {
        //            throw new InvalidPluginExecutionException(ex.Message);
        //        }
        //    }

        //}
    }
}
