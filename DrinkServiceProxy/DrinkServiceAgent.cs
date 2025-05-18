using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DrinkServiceContract;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Net.Security;

namespace DrinkServiceProxy
{
    public class DrinkServiceAgent
    {
        
        public DrinkServiceAgent(string endpoint)
        {
            Endpoint = endpoint;
        }
        public string Endpoint { get; set; }
        ChannelFactory<IDrinkService> GetFactory()
        {
            if (Endpoint != null)
            {
                Endpoint = Endpoint.Trim();
            }
            ChannelFactory<IDrinkService> factory = new ChannelFactory<IDrinkService>();
            //factory.Credentials.Windows.ClientCredential=new NetworkCredential("Gast","");                

            factory.Credentials.UserName.UserName = "Reflex";
            factory.Credentials.UserName.Password = "Reflex";

            ContractDescription contract = new ContractDescription("DrinkServiceContract.IDrinkService");
            NetTcpBinding nettcpBinding = new NetTcpBinding();
            nettcpBinding.PortSharingEnabled = true;
            nettcpBinding.Security.Mode = SecurityMode.None;
            nettcpBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            nettcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
            nettcpBinding.Security.Transport.ProtectionLevel = ProtectionLevel.None;
            nettcpBinding.MaxReceivedMessageSize = 2147483647;
            nettcpBinding.ReaderQuotas.MaxArrayLength = 2147483647;
            nettcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;



            factory.Endpoint.Address = new EndpointAddress("net.tcp://" + Endpoint + ":5000/DrinkService/");

            //factory.Endpoint.Address = new EndpointAddress("net.tcp://localhost:8733/LicenseService/");
            factory.Endpoint.Contract.ContractType = typeof(IDrinkService);
            factory.Endpoint.Binding = nettcpBinding;

            return factory;
        }

        public T Exec<T>(Func<IDrinkService,T> Function) 
        {
            var channel= GetFactory();
            try
            {
                IDrinkService IDrinkService = channel.CreateChannel();
                T result = Function.Invoke(IDrinkService);
                channel.Close();     
                return result;
                                           
            }
            catch
            {
                channel.Abort();
            }
            return default(T);
            
        }

        public void Exec(Action<IDrinkService> Function)
        {
            var channel = GetFactory();
            try
            {
                IDrinkService IDrinkService = channel.CreateChannel();
                Function.Invoke(IDrinkService);
                channel.Close();
            }
            catch
            {
                channel.Abort();
            }
        }
    }
}
