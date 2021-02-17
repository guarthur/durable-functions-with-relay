using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableFunctionConsumingWCF
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();
            var input = context.GetInput<int>();

            // Replace "hello" with the name of your Durable Activity Function.
            //outputs.Add(await context.CallActivityAsync<string>("Function1_Hello", "TEST"));
            outputs.Add(await context.CallActivityAsync<string>("WcfActivity3", input));
            //outputs.Add(await context.CallActivityAsync<string>("ComplexWcfActivity", "mytest1"));
            //outputs.Add(await context.CallActivityAsync<string>("WcfActivity3", 55));
            //outputs.Add(await context.CallActivityAsync<string>("ComplexWcfActivity", "mytest2"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("Function1_Hello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName("Function1_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            int amount;
            try
            {
                amount = await req.Content.ReadAsAsync<int>();
            }
            catch (UnsupportedMediaTypeException)
            {
                amount = 0;
            }

            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Function1", null, amount);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}