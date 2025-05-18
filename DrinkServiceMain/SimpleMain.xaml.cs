using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Utilities;
using System.Timers;
using DrinkServiceContract;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using DrinkServiceHost;
using DrinkServiceProxy;
using System.ServiceModel;
using ServiceContract;
using System.Threading;



namespace DrinkServiceMain
{
    /// <summary>
    /// Interaction logic for SimpleMain.xaml
    /// </summary>
    public partial class SimpleMain : Window
    {
        delegate void MethodInvoker();



        Host m_drinkService = new Host();
        bool closing = false;
        DrinkServiceAgent m_drinkServiceAgent;
        ServerContact serverContact;
        public SimpleMain()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            InitializeComponent();

            if (new Disclaimer().ShowDialog().GetValueOrDefault())
            {
                serverContact = new ServerContact();
                if (IsApplicationAlreadyRunning())
                {
                    MessageBox.Show("Het beursfuifprogramma is reeds gestart");
                    this.Close();
                    return;
                }

                m_drinkService.BeginStart();

                //m_drinkService = new DrinkServiceImplementation.Service1(AppDomain.CurrentDomain.BaseDirectory + "/DB/");

                string endpoint = txtServer.Text;

                m_drinkServiceAgent = new DrinkServiceAgent(endpoint);
                pubDatePattern.Content = Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern.ToUpper();
                this.Closing += new System.ComponentModel.CancelEventHandler(SimpleMain_Closing);

            }
            else
            {
                this.Close();
            }
        }

        bool IsApplicationAlreadyRunning()
        {
            //return true;
            string proc = Process.GetCurrentProcess().ProcessName;
            Process[] processes = Process.GetProcessesByName(proc);
            if (processes.Length > 1)
                return true;
            else
                return false;
        }

        void SimpleMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            serverContact.Close();
            closing = true;
            m_drinkService.Stop();
        }

      

        private void MnuExitApplication(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnManual(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://install.beursparty.net/manual.pdf");
        }




        public void BtnStartKassa(object sender, RoutedEventArgs e)
        {

            m_drinkServiceAgent.Endpoint = txtServer.Text;
            if (m_drinkServiceAgent.Exec(d => d.CanConnect()))
            {
                DrinkKassaClient.Window1 m_drinkKassa = new DrinkKassaClient.Window1(m_drinkServiceAgent);
                m_drinkKassa.Show();
            }
            else
            {
                MessageBox.Show("Kon niet met server verbinden");
            }
        }

        public void StartProjector(object sender, RoutedEventArgs e)
        {            
            m_drinkServiceAgent.Endpoint = txtServer.Text;
            if (m_drinkServiceAgent.Exec(d => d.CanConnect()))
            {

                
                DrinkStatsClient2.Window1 m_drinkStats = new DrinkStatsClient2.Window1(m_drinkServiceAgent, txtCodeEE.Text.ToLower()=="reflex");
                m_drinkStats.Show();
            }
            else
            {
                MessageBox.Show("Kon niet met server verbinden");
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = e.ExceptionObject as Exception;
                Logger.LogMessage(ex.Message + "|" + ex.StackTrace);
                Console.Error.WriteLine("Error occured");
                Console.Error.Flush();
                MessageBox.Show("Er heeft zich een fout voorgedaan. Gelieve de geopende errorlog op te sturen naar info@beursparty.net.");
                LoadLog();
                
                this.Close();
            }
            catch(Exception ex)
            {
                LicenseServerProxy licenseServerProxy = new LicenseServerProxy();
                licenseServerProxy.Exec(d => d.Error(ex.Message + ex.StackTrace));
            }
        }
        
        private void Button_Click_Test(object sender, RoutedEventArgs e)
        {
            m_drinkServiceAgent.Endpoint = txtServer.Text;
            if (m_drinkServiceAgent.Exec(d => d.CanConnect()))
            {
                MessageBox.Show("Connectie ok");                
            }
            else
            {
                MessageBox.Show("Er kan niet geconnecteerd worden met de service. Controleer of de applicatie op de ander draait en dat de firewall afstaat");
            }
        }

        private static string GetCurrentIP()
        {
            string ip = string.Empty;

            System.Net.IPAddress[] a = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].ToString().IndexOf(":") == -1)
                {
                    ip += a[i].ToString() + " of ";
                }
            }
            return ip;
        }

     

    
        private static void LoadLog()
        {
            string location = AppDomain.CurrentDomain.BaseDirectory + "//" + AppDomain.CurrentDomain.FriendlyName + "ErrorLog.txt";
            
            if (File.Exists(location))
            {
                System.Diagnostics.Process.Start(location);
            }
            else
            {
                MessageBox.Show("Er is geen log file aanwezig");
            }
        }

        private void OpenLog_Click_1(object sender, RoutedEventArgs e)
        {
            LoadLog();
        }
        bool pubError = false;
        private void BtnPublicate_Click_1(object sender, RoutedEventArgs e)
        {
            pubError = false;
            BeursEvent beursEvent = new BeursEvent();
            beursEvent.Name = CheckEntry(pubName);
            beursEvent.Street = CheckEntry(pubStreet);
            beursEvent.StreetNumber = CheckEntry(pubStreetNumber);
            beursEvent.Token = GetToken();
            beursEvent.Zipcode = CheckEntry(pubZipcode);
            beursEvent.Date = CheckDateEntry(pubDate);
            beursEvent.Country = CheckEntry(pubCountry);
            beursEvent.City = CheckEntry(pubCity);
            

            if (!pubError)
            {
                LicenseServerProxy proxy = new LicenseServerProxy();
                
                serverContact.Show();
                proxy.Exec(d => d.AddEvent(beursEvent));
                serverContact.Hide();
                MessageBox.Show("Uw event werd gepubliceerd");
                pubName.Text = pubStreet.Text = pubStreetNumber.Text = pubZipcode.Text = pubDate.Text = pubCountry.Text = pubCity.Text = "";
                
            }
        }

        private DateTime CheckDateEntry(TextBox textbox)
        {
            DateTime date = new DateTime();
            if (!DateTime.TryParse(textbox.Text, out date) || date<DateTime.Now.Date)
            {
                textbox.Background = Brushes.Pink;
                pubError = true;
            }
            else
            {                
                textbox.Background = Brushes.White;
            }
            return date;
        }

        private string CheckEntry(TextBox textbox)
        {

            if (String.IsNullOrEmpty(textbox.Text))
            {
                textbox.Background = Brushes.Pink;
                pubError = true;
            }
            else
            {
                textbox.Background = Brushes.White;
            }
            
            return textbox.Text;
        }

        private Guid GetToken()
        {
            return m_drinkServiceAgent.Exec(d => d.GetConfig()).Token;
        }
       
    }
}
