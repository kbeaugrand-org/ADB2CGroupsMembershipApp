
namespace ADB2CGroupsMembershipApp
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using System.Net.Http;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using System.Net.Http.Headers;

    public static class GetGroups
    {
        [FunctionName("GetGroups")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            string objectId = req.Query["objectId"];

            var access_token = await GetAccessToken();

            var groupsList = await GetObjectGroups(access_token, objectId);

            return new OkObjectResult(new
            {
                groups = groupsList
            });
        }

        private static async Task<string> GetAccessToken()
        {
            var client = HttpClientFactory.Create();
            client.BaseAddress = new Uri($"https://login.microsoftonline.com");

            var request = new HttpRequestMessage(HttpMethod.Post, $"/{Environment.GetEnvironmentVariable("TENANT_ID")}/oauth2/v2.0/token?api-version=1.0");

            var parameters = new Dictionary<string, string>();

            parameters.Add("grant_type", "client_credentials");
            parameters.Add("client_id", Environment.GetEnvironmentVariable("CLIENT_ID"));
            parameters.Add("client_secret", Environment.GetEnvironmentVariable("CLIENT_SECRET"));
            parameters.Add("scope", "https://graph.microsoft.com/.default");

            request.Content = new FormUrlEncodedContent(parameters);

            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsAsync<dynamic>();

            return Convert.ToString(responseBody.access_token);
        }

        // method to call MS Graph
        private static async Task<List<string>> GetObjectGroups(string access_token, string objectId)
        {
            var client = HttpClientFactory.Create();
            client.BaseAddress = new Uri("https://graph.microsoft.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);

            var response = await client.GetAsync($"/v1.0/users/{objectId}/memberOf");

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsAsync<JObject>();
            var groups = new List<string>();

            if (!responseBody.ContainsKey("value"))
            {
                return groups;
            }

            foreach (JObject g in responseBody["value"])
            {
                if (!g.ContainsKey("@odata.type") || !g.ContainsKey("displayName"))
                {
                    continue;
                }

                var type = g["@odata.type"].Value<string>();

                if (type == "#microsoft.graph.group")
                {
                    var groupDN = g["displayName"].Value<string>();

                    groups.Add(groupDN);
                }
            }

            return groups;
        }
    }
}
