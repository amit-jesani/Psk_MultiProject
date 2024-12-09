using System;

public class Class1
{


        public void deleteaccount()
        {
            if 
            (_target.LogicalName == "account" && _context.MessageName == "Delete")
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
