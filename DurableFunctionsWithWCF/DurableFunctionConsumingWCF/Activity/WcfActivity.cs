using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DurableFunctionConsumingWCF.Activity
{
    public static class WcfActivity
    {
        [FunctionName("WcfActivity3")]
        public static async Task<string> RunWCF3(
            [ActivityTrigger] int number, ILogger log)
        {            

            try
            {
                //WcfServiceTest.Service1Client client = new WcfServiceTest.Service1Client(binding, address);                
                Services.ServiceContract service = new Services.ServiceContract();

                //await Task.Delay(2000);                

                var response = await service.GetApproval(number);
                
                response = GetStringResponseFromSoap(response);

                log.LogInformation($"{Environment.NewLine}-> GetDataAsync = {response}{Environment.NewLine}");

                return response;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public class GetApprovalResponse
        {
            public string response { get; set; }
        }

        public static string GetStringResponseFromSoap(string soapXml)
        {
            var xdoc = XDocument.Parse(soapXml);
            XNamespace soapEnv = "http://schemas.xmlsoap.org/soap/envelope/";
            return xdoc.Element(soapEnv + "Envelope").Value; ;
        }


        [FunctionName("WcfActivity2")]
        public static async Task<string> RunWCF2(
            [ActivityTrigger] int number, ILogger log)
        {

            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            var address = new EndpointAddress(Environment.GetEnvironmentVariable("WcfUrl2"));

            try
            {
                //WcfServiceTest.Service1Client client = new WcfServiceTest.Service1Client(binding, address);                
                ServiceWcf.ServiceContractClient client = new ServiceWcf.ServiceContractClient(binding, address);                

                //await Task.Delay(2000);

                var response = await client.GetApprovalAsync(number);

                log.LogInformation($"{Environment.NewLine}-> GetDataAsync = {response}{Environment.NewLine}");

                return response;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        [FunctionName("WcfActivity")]
        public static async Task<string> RunWCF(
            [ActivityTrigger] int number, ILogger log)
        {

            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            var address = new EndpointAddress(Environment.GetEnvironmentVariable("WcfUrl"));

            try
            {
                WcfServiceTest.Service1Client client = new WcfServiceTest.Service1Client(binding, address);

                var request = new WcfServiceTest.GetDataRequest(number);

                //await Task.Delay(2000);

                var response = await client.GetDataAsync(request);

                log.LogInformation($"{Environment.NewLine}-> GetDataAsync = {response.GetDataResult}{Environment.NewLine}");

                return response.GetDataResult;

            } catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [FunctionName("ComplexWcfActivity")]
        public static async Task<string> RunComplexWCF(
            [ActivityTrigger] string something, ILogger log)
        {
            WcfServiceTest.Service1Client client = new WcfServiceTest.Service1Client();

            var compositeObject = new WcfServiceTest.CompositeType();
            compositeObject.BoolValue = true;
            compositeObject.StringValue = something;

            var request = new WcfServiceTest.GetDataUsingDataContractRequest(compositeObject);

            await Task.Delay(2000);

            var response = await client.GetDataUsingDataContractAsync(request);

            log.LogInformation($"{Environment.NewLine}->GetDataUsingDataContractAsync = {response.GetDataUsingDataContractResult.StringValue}{Environment.NewLine}");

            return response.GetDataUsingDataContractResult.StringValue;
        }

    }
}