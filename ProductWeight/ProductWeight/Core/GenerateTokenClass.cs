using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProductWeight
{
    public class GenerateTokenClass
    {
        public class TokenResponce
        {
            public string token_type { get; set; }
            public string expires_in { get; set; }
            public string ext_expires_in { get; set; }
            public string expires_on { get; set; }
            public string not_before { get; set; }
            public string resource { get; set; }
            public string access_token { get; set; }
        }

        public static TokenResponce GetToken()
        {
            var client = new HttpClient();

            client.BaseAddress = new Uri(CommonQueries.azureLoginURL);
            var request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress);

            var requestContent = string.Format("grant_type={0}&client_id={1}&client_secret={2}&resource={3}", CommonQueries.grantType, CommonQueries.clientId, CommonQueries.clientSecret, CommonQueries.resource);
            request.Content = new StringContent(requestContent, Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = client.SendAsync(request);
            string result = response.Result.Content.ReadAsStringAsync().Result;
            TokenResponce tokenResponce = JsonConvert.DeserializeObject<TokenResponce>(result);

            if (response.Result.StatusCode == HttpStatusCode.OK) return tokenResponce;
            else return null;
        }
    }
}
