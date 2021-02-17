using Microsoft.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            ChannelFactory<IServiceChannel> channelFactory;
            channelFactory = new ChannelFactory<IServiceChannel>(new BasicHttpRelayBinding(), "https://azurerelaytest.servicebus.windows.net/azurerelayservice");
            channelFactory.Endpoint.Behaviors.Add(new TransportClientEndpointBehavior
            {
                TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider("SharedAccessKey", "5YEwr1YFY3V7AThi2k/Mldaezr+m6lg5PP5cumlkYUo=")
            });
            // Create the Communication to the WCF
            IServiceChannel channel = channelFactory.CreateChannel();
            // Call the Service Method using Channel
            Console.WriteLine("Approval for 50? ->" + channel.GetApproval(50));
            Console.WriteLine("Approval for 500? ->" + channel.GetApproval(500));
            Console.ReadLine();
        }
    }
}
