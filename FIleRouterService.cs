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

namespace KLADemo
{
    public static class FileRouterService
    {
        [FunctionName("FileRouterService")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log,ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["storagename"];
            if(req != null)
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                
                name = name ?? data?.storagename;
            }
           
            
            log.LogInformation(context.FunctionDirectory);
            log.LogInformation($"{Directory.GetParent(context.FunctionDirectory).FullName}\\router.json");
            string path =context.FunctionDirectory+"\\router.json";
            if(!File.Exists(path))
            {
                  log.LogInformation("1 routerfile exists");
                path = Directory.GetParent(context.FunctionDirectory).FullName+ "\\router.json";
            }
            if (File.Exists(path))
            {
                log.LogInformation("2 routerfile exists");
            using (StreamReader file = File.OpenText(path)){
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                JObject jRoute = (JObject) JToken.ReadFrom(reader);
               
                JArray array = (JArray) jRoute["Router"];
                log.LogInformation(array.Count.ToString());
                }
            }
            }
            string responseMessage = "dest";
            return new OkObjectResult(responseMessage);
        }
    }
}
