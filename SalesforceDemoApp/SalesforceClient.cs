using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace SalesforceDemoApp
{
    internal class SalesforceClient
    {
        private const string apiEndpoint = "/services/data/v51.0/";
        public string loginEndpoint { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthToken { get; set; }
        public string InstanceUrl { get; set; }

        public SalesforceClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
        }

        public void login()
        {
            var jsonResponse = default(string); // will hold the JSON response Salseforce gives us
            using (var client = new HttpClient()) // HttpClient allows us to make web requests
            {
                // create a web form with key value pairs we can send to the server
                // with information that allows our application to be authenticated
                // and authorized.  A dictionary is simply a collection of key value pairs.
                var request = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"grant_type", "password" },
                    {"client_id", ClientId },
                    {"client_secret", ClientSecret },
                    {"username", Username },
                    {"password", $"{ Password }{ Token }" }, // note these are concatenated together
                });
                request.Headers.Add("X-PreetyPrint", "1"); // get formatted JSON from Salesforce
                var response = client.PostAsync(loginEndpoint, request).Result; // posting form
                jsonResponse = response.Content.ReadAsStringAsync().Result; // we asked for JSON
                Console.WriteLine($"Response: {jsonResponse}"); // we can see the output
                // this converts the JSON back to key value pairs so we can inspect it more easily
                var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);
                AuthToken = values["access_token"]; // We need our auth token for data requests
                InstanceUrl = values["instance_url"]; // We need this to parse URLs for our request
                Console.WriteLine($"AuthToken= {AuthToken}"); // displaying so we can see
                Console.WriteLine($"InstanceUrl= {InstanceUrl}"); // displaying so we can see
            }
        }

        public List<sfRecord> Query()
        {
            // 5000 should be the limit
            var result = new List<TopLayer> { };
            string soqlQuery = "SELECT AccountId, Email FROM Contact where Email != null Limit 5000";
            var isDone = false;
            string restRequest = $"{ InstanceUrl }{ apiEndpoint }query?q={ soqlQuery }";
            do
            {
                using (var client = new HttpClient())
                {
                    using (var request = GetNewHttpGetRequest(restRequest))
                    {
                        var response = GetResponse(request, client);
                        result.Add(response);
                        isDone = response.done;
                        // check to see what the next url to use is
                        if (!isDone) restRequest = $"{ InstanceUrl }{ response.nextRecordsUrl }"; 
                    }
                }
            } while (!isDone);
            return GetRecordsFromTopLayer(result);
        }

        private TopLayer GetResponse(HttpRequestMessage request, HttpClient client)
        {
            var response = client.SendAsync(request).Result;
            var content = response.Content.ReadAsStreamAsync().Result;
            var textReader = new System.IO.StreamReader(content);
            var jsonReader = new JsonTextReader(textReader);
            var jsonSerializer = JsonSerializer.CreateDefault();
            var result = jsonSerializer.Deserialize<TopLayer>(jsonReader);
            return result;
        }

        private HttpRequestMessage GetNewHttpGetRequest(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer { AuthToken }");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("X-PreetyPrint", "1");
            return request;
        }

        private List<sfRecord> GetRecordsFromTopLayer(List<TopLayer> data)
        {
            var result = new List<sfRecord> { };
            data.ForEach(c => result.AddRange(c.records));
            return result;
        }


    }
}
