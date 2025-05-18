using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DrinkServiceContract;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Net.Security;
using System.Net;


    public class LicenseServerProxy
    {
        ChannelFactory<ILicenseService> GetFactory()
        {
            ChannelFactory<ILicenseService> factory = new ChannelFactory<ILicenseService>();
            //factory.Credentials.Windows.ClientCredential=new NetworkCredential("Gast","");                

           

            ContractDescription contract = new ContractDescription("DrinkServiceContract.ILicenseService");
            WSHttpBinding nettcpBinding = new WSHttpBinding();
            //nettcpBinding.PortSharingEnabled = true;
            nettcpBinding.Security.Mode = SecurityMode.None;
            nettcpBinding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            nettcpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            nettcpBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            
            //factory.Credentials.UserName.UserName = "Reflex";
            //factory.Credentials.UserName.Password = "Reflex";
            
            nettcpBinding.MaxReceivedMessageSize = 2147483647;
            nettcpBinding.ReaderQuotas.MaxArrayLength = 2147483647;
            ServicePointManager.Expect100Continue = false;
            factory.Endpoint.Address = new EndpointAddress(new Uri("http://www.beursparty.net/LicenseService.svc"));
            //factory.Endpoint.Address = new EndpointAddress(new Uri("http://localhost:7228/LicenseService.svc"));
            //factory.Endpoint.Address = new EndpointAddress("net.tcp://localhost:8733/LicenseService/");
            factory.Endpoint.Contract.ContractType = typeof(ILicenseService);
            factory.Endpoint.Binding = nettcpBinding;

            return factory;
        }

		

        public T Exec<T>(Func<ILicenseService, T> Function)
        {
            var channel = GetFactory();
            try
            {
                ILicenseService IDrinkService = channel.CreateChannel();
                T result = Function.Invoke(IDrinkService);
                channel.Close();
                return result;

            }
            catch
            {
                try
                {
                    channel.Abort();
                }
                catch { }
            }
            return default(T);

        }

        public void Exec(Action<ILicenseService> Function)
        {
                var channel = GetFactory();
            try
            {
                ILicenseService IDrinkService = channel.CreateChannel();
                Function.Invoke(IDrinkService);
                channel.Close();
            }
            catch
            {
                try
                {
                    channel.Abort();
                }
                catch { }
            }
        }
    }

