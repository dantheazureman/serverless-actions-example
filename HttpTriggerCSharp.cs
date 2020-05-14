using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq; 

namespace My.Functions
{
    public static class GreatestICFlavas
    {
        const string getProductUri = "https://serverlessohproduct.trafficmanager.net/api/GetProduct?productId={0}";
        const string getUserUri = "https://serverlessohuser.trafficmanager.net/api/GetUser?userId={0}";

        [FunctionName("SaveName")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, 
            [Queue("outqueue"),StorageAccount("AzureWebJobsStorage")] ICollector<string> msg,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;
            string responseMessage;
            if(string.IsNullOrEmpty(name)) {
                responseMessage = "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response.";
            } else {
                responseMessage = $"Hello, {name}. This HTTP triggered function executed successfully.";
                msg.Add(string.Format("Name passed to the function: {0}", name));
            }

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("GetFlava")]
        public static async Task<IActionResult> GetFlava(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, 
            [Queue("outqueue"),StorageAccount("AzureWebJobsStorage")] ICollector<string> msg,
            ILogger log)
        {
            log.LogInformation("Get Ice Cream Flava triggered");

            string id = req.Query["productId"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            id = id ?? data?.id;
            string responseMessage;
            if(string.IsNullOrEmpty(id)) {
                responseMessage = "You need to enter in the product Id to get the flavor";
            } else {
                responseMessage = $"The product name for your product id {id} is Starfruit Explosion";
                msg.Add(string.Format("Product ID passed to the function: {0}", id));
            }

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("CreateRating")]
        public static async Task<IActionResult> CreateRating(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, 
            [Queue("outqueue"),StorageAccount("AzureWebJobsStorage")] ICollector<string> msg,
            ILogger log)
        {
            log.LogInformation("Create Ice Cream Rating triggered");
            
            //check valid user and product id
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            JToken obj = JObject.Parse(requestBody);
            string uId = (string)obj.SelectToken("userId");
            string pId = (string)obj.SelectToken("productId");      
            var httpClient = new System.Net.Http.HttpClient();
            var userResp = await httpClient.GetAsync(String.Format(getUserUri, uId));
            var prodResp = await httpClient.GetAsync(String.Format(getProductUri, pId));

            try {
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                data.id = new Guid();
                data.timeStamp = new DateTime().ToUniversalTime();
                
                return new OkObjectResult(JsonConvert.SerializeObject(data));

            } catch {
                return new OkObjectResult("Failed to retrieve either the userId or the productId");
            }

            
        }
    }
}
