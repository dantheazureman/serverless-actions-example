using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace My.Functions
{
    public static class GreatestICFlavas
    {
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

        [FunctionName("GetFlavaName")]
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
                responseMessage = $"The product name for your product id {id} is Starfruit Explosion!! ";
                msg.Add(string.Format("Product ID passed to the function: {0}", id));
            }

            return new OkObjectResult(responseMessage);
        }
    }
}
