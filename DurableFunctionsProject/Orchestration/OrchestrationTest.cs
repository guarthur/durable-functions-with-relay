using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DurableFunctionsProject.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableFunctionsProject
{
    public static class OrchestrationTest
    {
        [FunctionName("OrchestrationTest")]
        public static async Task<string> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var tasks = new List<Task>();
            Payment payment = context.GetInput<Payment>();
            string paymentResult;

            if (payment is null)
            {
                throw new System.Exception("Invalid payment");
            }

            if (payment.Customer is null)
            {
                throw new System.Exception("Invalid Customer");
            }

            var paymentProcessingNotification = new CustomerNotification(payment.Customer, "Payment processing");

            var PaymentApprovalWorkflowTask = context.CallSubOrchestratorAsync<bool>("PaymentApprovalWorkflow", payment);

            tasks.Add(context.CallActivityAsync<Task>("NotifyCustomerActivity", paymentProcessingNotification));
            tasks.Add(PaymentApprovalWorkflowTask);

            await Task.WhenAll(tasks);

            paymentResult = (PaymentApprovalWorkflowTask.Result) ? "Payment Approved" : "Payment Rejected";

            return paymentResult;
        }

        [FunctionName("OrchestrationTest_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {

            var payment = await req.Content.ReadAsAsync<Payment>();

            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("OrchestrationTest", payment);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}