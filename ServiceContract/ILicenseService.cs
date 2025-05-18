using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using ServiceContract;

namespace DrinkServiceContract
{
    [ServiceContract]   
    public interface ILicenseService
    {
        [OperationContract]
        ActivationAnswer Activate(Guid Key);

        [OperationContract]
        void Log(int MinutesLeft);

        [OperationContract]
        void LogV2(int MinutesLeft,string Name);

        [OperationContract]
        void LogV2File(int MinutesLeft, byte[] LogFile);

        [OperationContract]
        void AddEvent(BeursEvent Event);

        [OperationContract]
        void SendDrinkOrderList(Guid Token,List<DrinkOrder> DrinkeOrderList);

        [OperationContract]
        void SyncDrinks(Guid Token, List<Drink> drinkList);

        [OperationContract]
        void Error(string error);

        [OperationContract]
        bool CanConnect();
    }
}
