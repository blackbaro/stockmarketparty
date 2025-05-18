using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ServiceModel;
using DrinkServiceContract;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Configuration;
using System.IO;

using Utilities;
using System.Threading;

namespace LicenseServerImplementation
{
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Single)]
    public class LicenseService : ILicenseService
    {

        DataBase.LicenseDBDataContext db;

        public LicenseService()
        {
            string connString = ConfigurationManager.ConnectionStrings["BeursFuifLicensesConnectionString"].ConnectionString;
            db = new DataBase.LicenseDBDataContext(connString);
        }

        public ActivationAnswer Activate(Guid Key)
        {
            string IP = GetIP();

            DataBase.LicenseRequest licenseRequest = new DataBase.LicenseRequest();
            licenseRequest.ID = Guid.NewGuid();
            licenseRequest.IP = IP;
            licenseRequest.RequestDate = DateTime.Now;
            licenseRequest.LicenseID = Key;
            db.LicenseRequests.InsertOnSubmit(licenseRequest);
            db.SubmitChanges();
            //check if license is valid
            ActivationAnswer activationAnswer = new ActivationAnswer();
            DataBase.License license = null;
            if (Key == Guid.Empty)
            {
                license = new DataBase.License();
                license.CreationDate = DateTime.Now;
                license.Email = "free";
                license.TestLicense = false;
                license.ResetLicense = true;
                license.Hours = 12;
                license.ID = Guid.NewGuid();
                db.Licenses.InsertOnSubmit(license);
                db.SubmitChanges();
            }
            else
            {
                license = db.Licenses.SingleOrDefault(d => d.ID == Key && d.ActivationDate == null);
            }

            if (license != null)
            {
                license.ActivationIP = IP;
                license.ActivationDate = DateTime.Now;
                db.SubmitChanges();
                activationAnswer.Hours = license.Hours;
                activationAnswer.Name = license.Email;
                activationAnswer.IsValid = true;
                activationAnswer.IsTest = license.TestLicense;
                activationAnswer.ResetLicense = license.ResetLicense.GetValueOrDefault();

                if (license.ImagePath != null)
                {
                    byte[] Logo = File.ReadAllBytes(license.ImagePath);
                    activationAnswer.Logo = Logo;
                }
            }
            else
            {
                activationAnswer.IsValid = false;
            }

            return activationAnswer;
        }

        private static string GetIP()
        {
            OperationContext context = OperationContext.Current;

            MessageProperties messageProperties = context.IncomingMessageProperties;

            RemoteEndpointMessageProperty endpointProperty = messageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

            string IP = endpointProperty.Address;
            return IP;
        }

        public void Log(int MinutesLeft)
        {
            string IP = GetIP();

            DataBase.Log log = new DataBase.Log();
            log.ID = Guid.NewGuid();
            log.MinutesLeft = MinutesLeft;
            log.IP = IP;
            log.LogDate = DateTime.Now;
            db.Logs.InsertOnSubmit(log);
            db.SubmitChanges();
        }

        public void LogV2(int MinutesLeft, string Name)
        {
            OperationContext context = OperationContext.Current;

            MessageProperties messageProperties = context.IncomingMessageProperties;

            RemoteEndpointMessageProperty endpointProperty = messageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

            string IP = endpointProperty.Address;

            DataBase.Log log = new DataBase.Log();
            log.ID = Guid.NewGuid();
            log.MinutesLeft = MinutesLeft;
            log.Email = Name;
            log.IP = IP;

            log.LogDate = DateTime.Now;
            db.Logs.InsertOnSubmit(log);
            db.SubmitChanges();
        }

        public void LogV2File(int MinutesLeft, byte[] LogFile)
        {
            string IP = GetIP();
            if (!Directory.Exists(@"c:\LogBeurs")) Directory.CreateDirectory(@"c:\LogBeurs");
            if (LogFile != null)
            {
                File.WriteAllBytes(String.Format(@"c:\LogBeurs\{0}_{1}.txt", DateTime.Now.ToString("yyyyMMddHHmmss"), IP), LogFile);
            }
            else
            {
                File.WriteAllText(String.Format(@"c:\LogBeurs\{0}_{1}.txt", DateTime.Now.ToString("yyyyMMddHHmmss"), IP), "Logfile is null");
            }

        }


        public void AddEvent(ServiceContract.BeursEvent Event)
        {
            
            DataBase.BeursEvent dbBeursEvent = new DataBase.BeursEvent();
            EmailHelper.SendMail("noreply@beursparty.net", "michael@demeersseman.be", "BeursEvent added " + Event.Name, "", null, null);
            dbBeursEvent.Country = Event.Country;
            dbBeursEvent.CreationDate = DateTime.Now;
            dbBeursEvent.EventDate = Event.Date;
            dbBeursEvent.Name = Event.Name;
            dbBeursEvent.Street = Event.Street;
            dbBeursEvent.StreetNumber = Event.StreetNumber;
            dbBeursEvent.Token = Event.Token;
            dbBeursEvent.Zipcode = Event.Zipcode;
            dbBeursEvent.BeursEventID = Guid.NewGuid();
            db.BeursEvents.InsertOnSubmit(dbBeursEvent);
            db.SubmitChanges();
        }

        public void SendDrinkOrderList(Guid Token, List<DrinkOrder> DrinkOrderList)
        {
            //EmailHelper.SendMail("noreply@beursparty.net", "michael@demeersseman.be", "Drinkorders sent " + DrinkOrderList.Count(), "", null, null);
            foreach (DrinkOrder drinkOrder in DrinkOrderList)
            {
                DataBase.DrinkOrder dbdrinkOrder = db.DrinkOrders.SingleOrDefault(d => d.DrinkOrderID == drinkOrder.DrinkOrderID);
                if (dbdrinkOrder == null)
                {
                    dbdrinkOrder = new DataBase.DrinkOrder();
                    dbdrinkOrder.DrinkOrderID = drinkOrder.DrinkOrderID;
                    dbdrinkOrder.DrinkName = drinkOrder.DrinkName;
                    dbdrinkOrder.OrderDate = drinkOrder.OrderDate;
                    dbdrinkOrder.Price = drinkOrder.Price;
                    dbdrinkOrder.Token = Token;
                    dbdrinkOrder.DrinkID = drinkOrder.DrinkID;
                    dbdrinkOrder.CreationDate = DateTime.Now;
                    db.DrinkOrders.InsertOnSubmit(dbdrinkOrder);
                }
                
            }

            db.SubmitChanges();
        }

        public void SyncDrinks(Guid Token, List<Drink> drinkList)
        {
            foreach (Drink drink in drinkList)
            {
                DataBase.Drink dbDrink = db.Drinks.SingleOrDefault(d => d.DrinkID == drink.ID);
                if (dbDrink == null)
                {
                    dbDrink = new DataBase.Drink();
                    db.Drinks.InsertOnSubmit(dbDrink);
                }

                dbDrink.CreationDate = DateTime.Now;
                dbDrink.CurrentPrice = drink.CurrentPrice;
                dbDrink.DrinkID = drink.ID;
                dbDrink.DrinkName = drink.DrinkName;
                dbDrink.Hotkey = drink.HotKey;
                dbDrink.LastUpdateDate = drink.LastEditDate;
                dbDrink.MaxPrice = drink.MaxPrice;
                dbDrink.MinPrice = drink.MinPrice;
                dbDrink.NextManualPrice = drink.NextManualPrice;
                dbDrink.NextPrice = drink.NextPrice;
                dbDrink.Price = drink.CurrentPrice;
                dbDrink.Token = Token;
                dbDrink.NormalPrice = drink.NormalPrice;

                db.SubmitChanges();
            }

        }


        public void Error(string error)
        {
            EmailHelper.SendMail("noreply@beursparty.net", "michael@demeersseman.be", "Beursparty error",error, null, null);
        }


        public bool CanConnect()
        {
            return true;
        }
    }


}
