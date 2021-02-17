
using Microsoft.Azure.ServiceBus.Primitives;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DurableFunctionConsumingWCF.Services
{
    class ServiceContract : IServiceContract
    {
        //public string GetApproval(int value)
        //{
        //    //string s = "test";
        //    //ChannelFactory<IServiceChannel> channelFactory;
        //    //var address = new EndpointAddress("https://azurerelaytest.servicebus.windows.net/azurerelayservice");           
        //    //channelFactory = new ChannelFactory<IServiceChannel>(new BasicHttpRelayBinding(), address);            
        //    //channelFactory.Endpoint.EndpointBehaviors.Add(new TransportClientEndpointBehavior
        //    //{
        //    //    TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider("SharedAccessKey", "5YEwr1YFY3V7AThi2k/Mldaezr+m6lg5PP5cumlkYUo=")
        //    //});
        //    //// Create the Communication to the WCF
        //    ////IServiceChannel channel = channelFactory.CreateChannel();

        //    ////return channel.GetApproval(value);

        //    return s;
        //}

        public async Task<string> GetApproval(int value)
        {
            var relayUrl = "https://azurerelaytest.servicebus.windows.net/azurerelayservice";
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider("SharedAccessKey", "5YEwr1YFY3V7AThi2k/Mldaezr+m6lg5PP5cumlkYUo=");
            SecurityToken token = await tokenProvider.GetTokenAsync(relayUrl, new TimeSpan(1, 0, 0));

            //Setup HttpClient to communicate with WCF Service 
            using (var httpClient = new HttpClient())
            {
                //This header is required for Service Bus Relay
                httpClient.DefaultRequestHeaders.Add("ServiceBusAuthorization", token.TokenValue);

                //Here we setup the SOAP Action - schema: http://tempuri.org/<Interface>/<OperationName>
                httpClient.DefaultRequestHeaders.Add("SOAPAction", "http://tempuri.org/IServiceContract/GetApproval");
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/xml"));

                //Setup soap message body
                string soapMessageBody = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/""><soapenv:Header /><soapenv:Body><tem:GetApproval><tem:value>" + value + "</tem:value></tem:GetApproval></soapenv:Body></soapenv:Envelope>";

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(relayUrl),
                    Method = HttpMethod.Post,
                    Content = new StringContent(soapMessageBody, Encoding.UTF8)
                };


                var response = await httpClient.SendAsync(request);
                return await response.Content.ReadAsStringAsync();

            }
            
        }        


        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            throw new NotImplementedException();
        }
    }
}
