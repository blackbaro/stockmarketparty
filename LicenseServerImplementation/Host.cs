using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Security;

using DrinkServiceContract;
using System.Diagnostics;
using System.Configuration;

namespace LicenseServerImplementation
{
    public class Host
    {
        ServiceHost host;
        string sSource;
        string sLog;
        string sEvent;



        public void Start()
        {
            //Set current directory to assembly folder
            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            string connString = ConfigurationManager.ConnectionStrings["BeursFuifLicensesConnectionString"].ConnectionString;

            sSource = "LicenseService";
            sLog = "Application";
            sEvent = "Starting license server. Connecting to database: " + connString;

            //if (!EventLog.SourceExists(sSource))
            //    EventLog.CreateEventSource(sSource, sLog);
            //EventLog.WriteEntry(sSource, sEvent,EventLogEntryType.Information);

            DataBase.LicenseDBDataContext db = new DataBase.LicenseDBDataContext(connString);
            if (!db.DatabaseExists())
            {				
					db.CreateDatabase();		
            }
            //test connection
            int requestCount=db.LicenseRequests.Count();
            sEvent = "LicenseRequests.Count()=" + requestCount;
            //EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Information);

            //ILicenseService licenseService = new LicenseService();
            host = new ServiceHost(typeof(LicenseService));

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.None;
            binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            binding.Security.Mode = SecurityMode.None;
            binding.MaxReceivedMessageSize = 2147483647;
            binding.ReaderQuotas.MaxArrayLength = 2147483647;
            host.AddServiceEndpoint(typeof(ILicenseService), binding, "net.tcp://localhost:8733/LicenseService/");
            host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.MembershipProvider;
            //host.Credentials.UserNameAuthentication.MembershipProvider = new BeursMemberShipProvider();
            host.Open();
            
            
        }

        public void Stop()
        {
            host.Close();
        }
    }
}
