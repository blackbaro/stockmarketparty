using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DrinkServiceContract;
using DrinkServiceImplementation;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading;

namespace DrinkServiceHost
{
    public class Host
    {
        ServiceHost host;
        IDrinkService licenseService;
        public void Start()
        {          
                licenseService = new Service1();
                licenseService.Start();
                host = new ServiceHost(licenseService);

                NetTcpBinding binding = new NetTcpBinding();
                binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
                binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.None;
                binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
                binding.Security.Mode = SecurityMode.None;
                binding.MaxReceivedMessageSize = 2147483647;
                binding.ReaderQuotas.MaxArrayLength = 2147483647;
                host.AddServiceEndpoint(typeof(IDrinkService), binding, "net.tcp://localhost:5000/DrinkService/");
                host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.MembershipProvider;
                //host.Credentials.UserNameAuthentication.MembershipProvider = new BeursMemberShipProvider();
                host.Open();
                
            
        }
        public void BeginStart()
        {
            Thread thread = new Thread(new ThreadStart(Start));
            thread.IsBackground = true;
            thread.Start();
            
        }
        public void Stop()
        {
            if (host != null)
            {
                host.Close();
                
                licenseService.Stop();
            }
        }
    }
}
