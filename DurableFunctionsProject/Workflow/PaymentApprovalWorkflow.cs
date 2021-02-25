using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DurableFunctionsProject.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableFunctionsProject.Workflows
{
    public static class PaymentApprovalWorkflow
    {
        public static string InstanceId { get; set; }

        [FunctionName("PaymentApprovalWorkflow")]
        public static async Task<bool> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            log.LogInformation($"PaymentApprovalWorkflow begin instanceId: {context.InstanceId}");
            InstanceId = context.InstanceId;
            var payment = context.GetInput<Payment>();
            bool paymentApproved = false;

            if (payment.Amount < 50)
            {
                log.LogInformation("PaymentApprovalWorkflow approved under 50$");
                return await Task.FromResult<bool>(true);
            }

            using (var timeoutCancellationToken = new CancellationTokenSource())
            {
                DateTime expiration = context.CurrentUtcDateTime.AddSeconds(200);
                Task timeoutTask = context.CreateTimer(expiration, timeoutCancellationToken.Token);

                Task<bool> approvalResponseTask = context.WaitForExternalEvent<bool>("ManagerPaymentApproval");

                Task winnerTask = await Task.WhenAny(timeoutTask, approvalResponseTask);

                if (winnerTask == approvalResponseTask)
                {
                    paymentApproved = approvalResponseTask.Result;
                    log.LogInformation($"PaymentApprovalWorkflow ManagerPaymentApproval = {paymentApproved}");
                }

                if (winnerTask == timeoutTask)
                {
                    log.LogInformation($"PaymentApprovalWorkflow TIMED OUT");
                }

                if (!timeoutTask.IsCompleted)
                {
                    timeoutCancellationToken.Cancel();
                }

            }

            log.LogInformation($"PaymentApprovalWorkflow end result: {paymentApproved}");
            return await Task.FromResult<bool>(paymentApproved);
        }

        [FunctionName("ManagerApprovalActivity")]
        public static async Task ManagerApproval(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            log.LogInformation("ManagerApproval begin");            

            await starter.RaiseEventAsync(InstanceId, "ManagerPaymentApproval", await req.Content.ReadAsAsync<bool>());

            log.LogInformation("ManagerApproval end");
        }
    }
}