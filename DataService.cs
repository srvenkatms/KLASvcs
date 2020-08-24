using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Collections.Generic;
namespace KLADemo
{
    public static class DataService
    {
        [FunctionName("DataService")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post",Route = null)] HttpRequest req,
            ILogger log)
        {
           // RouteAttribute = Route = "DataService/{filename}"
           //?filename=uploaded.jpg
            log.LogInformation("C# HTTP trigger function processed a request.");

            //Upload a file to Azure blob
            string storageConnectionString = Environment.GetEnvironmentVariable("KLAConnString",EnvironmentVariableTarget.Process);
            string contentType = req.ContentType;


         
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            string containerString = Environment.GetEnvironmentVariable("Container",EnvironmentVariableTarget.Process);
            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerString);
            CloudBlockBlob blockBlob ;
            
                    // Retrieve query parameters
            IEnumerable<KeyValuePair<string, string>> values = req.GetQueryParameterDictionary();
            string filename = "";
            //Write query parameters to log
            foreach (KeyValuePair<string, string> val in values)
            {
               log.LogInformation($"Parameter: {val.Key}\nValue: {val.Value}\n");
                filename=val.Value;
           }
           log.LogInformation( "Length :" +req.Body.Length.ToString());
            if(string.IsNullOrEmpty(filename))
            {
                Guid g =  Guid.NewGuid();
           
                 blockBlob = container.GetBlockBlobReference( g.ToString()+".pdf");
            }else
            {
                 blockBlob = container.GetBlockBlobReference(filename);
            }
            blockBlob.Properties.ContentType = contentType;
             Stream ms = new MemoryStream(); 
             await req.Body.CopyToAsync(ms);
             ms.Seek(0, SeekOrigin.Begin);
             await blockBlob.UploadFromStreamAsync(ms);
             

            string responseMessage = " This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
