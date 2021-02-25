using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using DurableFunctionsProject.Domain;
using System.Threading;

namespace DurableFunctionsProject.Activity
{
    public static class NotifyCustomerActivity
    {
        [FunctionName("NotifyCustomerActivity")]
        public static async Task NotifyCustomer(
            [ActivityTrigger] CustomerNotification customerNotification, 
            ILogger log)
        {
            log.LogInformation("NotifyCustomer begin");

            log.LogInformation($"Notifying customer {customerNotification.Customer.Name} on email {customerNotification.Customer.Email}");
            await SendEmail(customerNotification.Customer, customerNotification.Message, log);

            log.LogInformation("NotifyCustomer end");
        }

        public static async Task SendEmail(Customer customer, string message, ILogger log)
        {
            await Task.Delay(10000);
            log.LogInformation($"Email send to {customer.Email} with message: {message}");
        }
    }
    
}
