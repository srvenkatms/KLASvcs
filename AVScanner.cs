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
using nClam;


namespace FrontEndAPI.Controllers
{	
    public static class AVScanService 
    {
		 [FunctionName("AVScanService")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post",Route = null)] HttpRequest req,
            ILogger log)
        {

			ScanResultModel scr = new ScanResultModel();
			Stream stream = new MemoryStream(); 
			if(req.Body == null)
			{
				stream = GenerateStreamFromString("a,b \n c,d");
			}
			 else
			{
				await req.Body.CopyToAsync(stream);
				stream.Seek(0, SeekOrigin.Begin);
			}
			
			 string clamip = Environment.GetEnvironmentVariable("clamip",EnvironmentVariableTarget.Process);
			//var clam = new ClamClient("13.86.246.40", 3310); // "clamav -server",
			// clam = new ClamClient("172.17.0.2", 3310); // "clamav -server",
			var clam = new ClamClient(clamip, 3310);
			scr.InfectedFilesCount = 0;
			var scanResult = await clam.SendAndScanFileAsync(stream);
			if(scanResult.InfectedFiles != null) scr.InfectedFilesCount = scanResult.InfectedFiles.Count;
		

			scr.RawResult = scanResult.RawResult;
			scr.Result = scanResult.Result.ToString();
			
			scr.IsComplete = true;
			
			//return CreatedAtAction("xyz", scr);
			string jsonData = JsonConvert.SerializeObject(scr);
			return new OkObjectResult(jsonData);
		}

		public static Stream GenerateStreamFromString(string s)
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(s);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}
	}
    public class ScanResultModel
	{
		public bool IsComplete { get; set; }
		public string RawResult {get; set;}
		public string Result {get; set;}

		public int InfectedFilesCount {get;set;}
	}
}
