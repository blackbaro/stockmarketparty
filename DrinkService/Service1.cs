using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using DrinkServiceContract;
using System.Timers;
using System.IO;
using System.Net;
using Utilities;




namespace DrinkServiceImplementation
{
    // NOTE: If you change the class name "Service1" here, you must also update the reference to "Service1" in App.config.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Service1 : DrinkServiceContract.IDrinkService
    {
        DrinkManager m_drinkManager;
        List<DrinkOrder> m_drinkOrders;
        DateTime m_lastCalculation;
        DrinkConfig m_config;

        DateTime m_nextCrash = DateTime.Now;
        ListSaver m_listSaver;
        string m_databasePath = "";
        Random rnd = new Random();
        int m_secondsTillRecalculation = 0;

        //license timer
        int m_secondsLeftInCurrentMinute = 60;
        Timer timer = new Timer(1000);
        delegate void LogMinutesDelegate(int MinutesLeft, string Name);


        public Service1()
        {
            string DataBasePath = Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/Beursparty/";
            //System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(StartHttpListener));
            //thread.Start();
            Logger.LogEvent("Initialising service");

            m_listSaver = new ListSaver(DataBasePath);

            m_drinkOrders = m_listSaver.GetList<DrinkOrder>("Orders.xml", true);
            foreach (var drinkorder in m_drinkOrders)
            {
                if (drinkorder.DrinkOrderID == Guid.Empty)
                {
                    drinkorder.DrinkOrderID = Guid.NewGuid();
                }
            }
            m_config = m_listSaver.GetObject<DrinkConfig>("ConfigV2.xml");


            if (m_config == null)
            {
                m_config = new DrinkConfig();                
            }
            if (m_config.Token == Guid.Empty)
            {
                m_config.Token = Guid.NewGuid();
                m_listSaver.SaveObject(m_config, "ConfigV2.xml");
            }
            m_nextCrash = DateTime.Now.AddSeconds(m_config.SecondsTillNextCrash);
            m_drinkManager = new DrinkManager();
            m_drinkManager.DrinkList = m_listSaver.GetList<Drink>("DrinksV2.xml", true);
            m_drinkManager.Sensitivity = (int)m_config.Sensibility;
            m_drinkManager.PriceInterval = (int)m_config.PriceInterval;
            m_databasePath = DataBasePath;

            KillDoubleDrinks();
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            Logger.LogEvent("Service initialised");
        }

        private void KillDoubleDrinks()
        {
            bool hasDoubles = false;
            var doubles = m_drinkManager.DrinkList.GroupBy(d => d.HotKey).Where(d => d.Count() > 1);
            foreach (var doubledrink in doubles)
            {
                for (int i = 1; i < doubledrink.Count(); i++)
                {
                    DeleteDrink(doubledrink.ElementAt(i).ID);
                    hasDoubles = true;
                }
            }
            if (hasDoubles)
            {
                Logger.LogMessage("doubles deleted");
            }

        }

        public void Start()
        {
            timer.Start();
        }
        public void Stop()
        {
            timer.Stop();
        }

        void SaveLists()
        {
            m_listSaver.SaveObject(m_drinkManager.DrinkList, "DrinksV2.xml");
            m_listSaver.SaveObject(m_drinkOrders, "Orders.xml");
            m_listSaver.SaveObject(m_config, "ConfigV2.xml");
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            m_secondsLeftInCurrentMinute--;
            m_secondsTillRecalculation--;

            if (m_secondsTillRecalculation <= 0)
            {
                Recalculate();
            }

            if (m_secondsLeftInCurrentMinute == 0)
            {
                DeductLicense();
                m_secondsLeftInCurrentMinute = 60;
            }
        }

        public DrinkStatus GetStatus()
        {
            return new DrinkStatus()
            {
                SecondsLeft = m_secondsTillRecalculation,
                CrashSecondsLeft = (int)m_nextCrash.Subtract(DateTime.Now).TotalSeconds,
                LastCalculation = m_lastCalculation

            };
        }

        public List<Drink> GetPrices()
        {
            KillDoubleDrinks();
            return m_drinkManager.DrinkList;
        }

        void DeductLicense()
        {         
            LogMinutesDelegate del = new LogMinutesDelegate(LogMinutes);
            del.BeginInvoke(1, "free", null, null);

        }
        
        void LogMinutes(int MinutesLeft, string Name)
        {
            lock (this)
            {
                try
                {
                    LicenseServerProxy licenseServiceProxy = new LicenseServerProxy();
                    //licenseServiceProxy.Exec(d=>d.LogV2(MinutesLeft, Name));
                }
                catch
                {

                }
            }
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private void GetFileList(string Dir, StringBuilder sb)
        {
            foreach (string file in Directory.GetFiles(Dir))
            {
                sb.AppendLine(file);                
            }

            foreach (string directory in Directory.GetDirectories(Dir))
            {
                GetFileList(directory, sb);
            }
        }
        
        public void SaveConfig(DrinkConfig Config)
        {
            if (Config.SecondsTillNextCrash != m_config.SecondsTillNextCrash)
            {
                m_nextCrash = DateTime.Now.AddSeconds(Config.SecondsTillNextCrash);
            }
            Config.LastChangeDate = DateTime.Now;
            m_config = Config;
            m_drinkManager.Sensitivity = (int)Config.Sensibility;
            m_drinkManager.PriceInterval = (int)Config.PriceInterval;

            SaveLists();
        }

        public DrinkConfig GetConfig()
        {
            return m_config;
        }
        
        public void Recalculate()
        {
            m_lastCalculation = DateTime.Now;
            if (m_nextCrash <= DateTime.Now)
            {
                m_secondsTillRecalculation = m_config.CrashSeconds;
                m_drinkManager.SetCrash();
                m_nextCrash = DateTime.Now.AddSeconds(m_config.SecondsTillNextCrash);
            }
            else
            {
                m_secondsTillRecalculation = m_config.SecondsBetweenRecalculation;
                m_drinkManager.SetNextPrices();
            }
            SaveLists();
        }

        public List<Drink> GetDrinkList()
        {
            KillDoubleDrinks();
            List<Drink> result = m_drinkManager.DrinkList.OrderBy(d => d.HotKey).ToList();


            return result.GroupBy(d => d.HotKey).Select(d => d.First()).ToList();

        }

        void StartHttpListener()
        {
            for (; ; )
            {
                if (!HttpListener.IsSupported)
                {
                    Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                    return;
                }
                // URI prefixes are required,
                // for example "http://localhost.com:8030".


                // Create a listener.
                HttpListener listener = new HttpListener();
                // Add the prefixes.

                listener.Prefixes.Add("http://localhost/index/");


                listener.Start();
                Console.WriteLine("Listening...");
                // Note: The GetContext method blocks while waiting for a request. 
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                // Obtain a response object.
                HttpListenerResponse response = context.Response;
                // Construct a response.
                string responseString = "<HTML><BODY><table><tr><td>DrinkName</td><td>DrinkPrice</td><td>Points</td></tr>";

                foreach (Drink drink in m_drinkManager.DrinkList)
                {
                    responseString += String.Format("<tr><td nowrap>{0}</td><td>{1}</td><td>{2}</td></tr>", drink.DrinkName, drink.CurrentPrice, m_drinkManager.DrinkList);
                }

                responseString += "</table></BODY></HTML>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
                listener.Stop();
            }
        }

        public void SaveDrink(Drink drink)
        {
            drink.LastEditDate = DateTime.Now;
            m_drinkManager.SaveDrink(drink);

            SaveLists();
        }
        
        public void DeleteDrink(Guid ID)
        {
            m_drinkManager.DeleteDrink(ID);
            SaveLists();

        }

        public void DeleteDrinkList(List<Guid> IDList)
        {
            foreach (Guid drinkToDelete in IDList)
            {
                DeleteDrink(drinkToDelete);
            }
        }

        public void OrderDrinkList(List<Guid> IDList)
        {
            List<DrinkOrder> orders = m_drinkManager.OrderDrinkList(IDList);
            m_drinkOrders.AddRange(orders);
            SaveDrinkOrders();
            PublishAllOrders();
        }

        private void SaveDrinkOrders()
        {
            lock (m_drinkOrders)
            {
                m_listSaver.SaveObject(m_drinkOrders, "Orders.xml");
            }
        }

        public void ResetPoints()
        {
            foreach (Drink drink in m_drinkManager.DrinkList)
            {
                drink.NextPrice = drink.NormalPrice;
            }

            Recalculate();
        }

        public void SetCrash()
        {
            m_nextCrash = DateTime.Now;
        }

        public int GetTotalTurnover()
        {
            try
            {
                return (int)m_drinkOrders.Sum(d => d.Price);
            }
            catch { }
            return 0;

        }

        public void PublishAllOrdersAsync()
        {
            LicenseServerProxy licenseServerProxy = new LicenseServerProxy();
            if (licenseServerProxy.Exec(d => d.CanConnect()))
            {
                List<DrinkOrder> drinkOrderListToPublish = m_drinkOrders.Where(d => d.Published == false).ToList();
                licenseServerProxy.Exec(d => d.SendDrinkOrderList(m_config.Token, m_drinkOrders));
                drinkOrderListToPublish.ForEach(d => d.Published = true);
                SaveDrinkOrders();
            }
        }

        public void PublishAllOrders()
        {
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(PublishAllOrdersAsync));
            thread.Start();          

        }

        public bool CanConnect()
        {
            return true;
        }

        public byte[] GetLogo()
        {
            string logoPath = m_databasePath + "/Logo.png";
            if (File.Exists(logoPath))
            {
                return File.ReadAllBytes(logoPath);
            }
            return null;
        }

        public void SetLogo(byte[] Logo)
        {
            string logoPath = m_databasePath + "/Logo.png";

            if (Logo != null)
            {
                File.WriteAllBytes(logoPath, Logo);
            }
            else
            {
                if (File.Exists(logoPath))
                {
                    File.Delete(logoPath);
                }
            }
        }
    }
}
