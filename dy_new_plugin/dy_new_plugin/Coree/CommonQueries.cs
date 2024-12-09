using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dy_new_plugin
{
    public class CommonQueries
    {
        #region CouponCode Validation
        public static string azureLoginURL = "https://login.microsoftonline.com/c89852de-dbc5-49c6-ad0e-b0890f3e99f9/oauth2/token";
        public static string grantType = "client_credentials";
        public static string clientId = "0c21f800-53cf-4161-8086-7d76d338b8f0";
        public static string clientSecret = "MLoz5G~64Gt.3qjsPntSZkCA44~_O8s.90";
        public static string resource = "https://gnlncdsbuildef1f7a0184bd7f66aos.cloudax.dynamics.com";
        public static string couponCodeAPI = "https://gnlncdsbuildef1f7a0184bd7f66aos.cloudax.dynamics.com/api/services/GNLNCouponCodeServiceGroup/GNLNCouponCodeAPIService/getDiscount";
        #endregion

        #region Clone SalesOrder
        public static string[] doNotCopySalesOrderAttributes = { "statuscode", "statecode", "msdyn_processingstatus", "salesorderid","ordernumber","msdyn_salesordernumber" };
        public static string[] doNotCopySalesOrderProductAttributes = { "statuscode", "statecode", "msdyn_linestatus", "salesorderdetailid","salesorderid" };
        #endregion
    }
}
