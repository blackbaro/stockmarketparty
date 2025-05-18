using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DrinkServiceContract;
using System.ServiceModel;

namespace DrinkServiceContract
{
    [ServiceContract]
    public interface IDrinkService
    {
        [OperationContract]
        void PublishAllOrders();

        [OperationContract]
        DrinkConfig GetConfig();

        [OperationContract]
        List<DrinkServiceContract.Drink> GetDrinkList();

        [OperationContract]
        List<DrinkServiceContract.Drink> GetPrices();

        [OperationContract]
        DrinkStatus GetStatus();

        [OperationContract]
        int GetTotalTurnover();

        [OperationContract]
        void DeleteDrink(Guid ID);

        [OperationContract]
        void DeleteDrinkList(System.Collections.Generic.List<Guid> IDList);

        [OperationContract]
        void OrderDrinkList(System.Collections.Generic.List<Guid> IDList);

        [OperationContract]
        void Recalculate();

        [OperationContract]
        void ResetPoints();

        [OperationContract]
        void SaveConfig(DrinkServiceContract.DrinkConfig Config);

        [OperationContract]
        void SaveDrink(DrinkServiceContract.Drink drink);

      

        [OperationContract]
        void SetCrash();

        [OperationContract]
        void Start();

        [OperationContract]
        void Stop();       

        [OperationContract]
        bool CanConnect();

        [OperationContract]
        byte[] GetLogo();

        [OperationContract]
        void SetLogo(byte[] Logo);
        
        
    }
}
