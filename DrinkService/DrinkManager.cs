using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DrinkServiceContract;

namespace DrinkServiceImplementation
{
    public class DrinkManager
    {
        public List<Drink> DrinkList { get; set; }
        public int Sensitivity { get; set; }
        public int PriceInterval { get; set; }
        public void SaveDrink(Drink drink)
        {

            Drink drinkToEdit = DrinkList.SingleOrDefault(d => d.ID == drink.ID);
            if (drinkToEdit == null)
            {
                
                DrinkList.Add(drink);
            }
            else
            {   
                drinkToEdit.CurrentPrice = drink.CurrentPrice;
                drinkToEdit.DrinkName = drink.DrinkName;
                drinkToEdit.HotKey = drink.HotKey;
                drinkToEdit.MaxPrice = drink.MaxPrice;
                drinkToEdit.MinPrice = drink.MinPrice;
                drinkToEdit.NextManualPrice = drink.NextManualPrice;
                if (drinkToEdit.NextManualPrice.HasValue)
                {
                    drinkToEdit.NextManualPrice = getIntervalPrice(drink.NextManualPrice.Value);
                }
                drinkToEdit.NextPrice = drink.NextPrice;
                drinkToEdit.NormalPrice = drink.NormalPrice;
                drinkToEdit.LastEditDate = drink.LastEditDate;
            }
        }
        public void SaveDrinkList(List<Drink> NewDrinkList)
        {
            List<Drink> drinkListToDelete = DrinkList.Except(NewDrinkList).ToList(); //current drinks that are not present in the new list have to be deleted properly
            foreach (Drink drinkToDelete in drinkListToDelete)
            {
                DeleteDrink(drinkToDelete.ID);
            }
            foreach (Drink DrinkToSave in NewDrinkList)
            {
                SaveDrink(DrinkToSave);
            }
        }
        public void DeleteDrink(Guid ID)
        {
            DrinkList.RemoveAll(d=>d.ID==ID);
            
        }
        public List<DrinkOrder> OrderDrinkList(List<Guid> IDList)
        {

            List<DrinkOrder> drinkOrders = new List<DrinkOrder>();
            foreach (Guid ID in IDList)
            {
                DrinkOrder order = new DrinkOrder();
                drinkOrders.Add(order);

                Drink drinkToOrder = DrinkList.Single(d => d.ID == ID);
                drinkToOrder.OrdersSinceLastCalculation++;
                
                order.DrinkID = drinkToOrder.ID;
                order.DrinkName = drinkToOrder.DrinkName;
                order.OrderDate = DateTime.Now;
                order.Price = drinkToOrder.CurrentPrice;
            }

            Decimal upDelta = Sensitivity;
            Decimal downDelta = -1 * Sensitivity; // from -25% to 25 procent
            Decimal deltaPerDrink = (upDelta - downDelta) / DrinkList.Count();
            Decimal currentDelta = upDelta;
            List<Drink> drinkListOrdered = DrinkList.OrderByDescending(d => d.OrdersSinceLastCalculation).ToList();
            var groupedByOrders = (from drink in DrinkList
                                   group drink by drink.OrdersSinceLastCalculation into grouped
                                   select new { orders=grouped.Key,drink=grouped }).ToList();
            groupedByOrders = groupedByOrders.OrderByDescending(d => d.orders).ToList();

            foreach (var group in groupedByOrders)
            {
                Decimal deltaForThisGroup = 0;
                foreach (var drink in group.drink)
                {
                    deltaForThisGroup += currentDelta;
                    currentDelta -= deltaPerDrink;
                }
                deltaForThisGroup = deltaForThisGroup / group.drink.Count();

                Decimal multiplier = deltaForThisGroup / 100m;
                foreach (var drink in group.drink)
                {
                    drink.LastEditDate = DateTime.Now;
                    Decimal specDrinkMultiplier = multiplier / Math.Max((0.4m * drink.NormalPrice), 1m);
                    drink.NextPrice = drink.CurrentPrice + (drink.NormalPrice * specDrinkMultiplier);
                    drink.NextPrice = Math.Min(drink.MaxPrice, drink.NextPrice);
                    drink.NextPrice = Math.Max(drink.MinPrice, drink.NextPrice);
                }
                
                
            }
            //keep average ok
            Decimal averageNormal = DrinkList.Average(d => d.NormalPrice);
            Decimal averageNext = DrinkList.Average(d => d.NextPrice);
            Decimal averageDelta = averageNormal - averageNext;
            foreach (Drink drink in DrinkList)
            {
                drink.NextPrice += averageDelta;
                drink.NextPrice = getIntervalPrice(drink.NextPrice);
            }
            
            //round it
            

            return drinkOrders;

        }

        private Decimal getIntervalPrice(decimal Price)
        {
            Price = (Decimal)Math.Round(Price, 2);
            int mod = (int)((Price * 100) % PriceInterval);
            float half = PriceInterval / 2;
            if (mod < half)
            {
                Price -= (Decimal)mod / 100;
            }
            else
            {
                Price += (Decimal)(PriceInterval - mod) / 100;
            }
            Price = (Decimal)Math.Round(Price, 2);
            return Price;
        }

        public void SetNextPrices()
        {
            foreach (Drink drink in DrinkList)
            {
                drink.LastEditDate = DateTime.Now;
                drink.CurrentPrice = drink.NextPrice;
                drink.OrdersSinceLastCalculation = 0;
                if (drink.NextManualPrice != null)
                {
                    drink.CurrentPrice = drink.NextManualPrice.Value;
                }
            }
      
        }
        public void SetCrash()
        {
            DrinkList.ForEach(d => d.CurrentPrice = d.MinPrice);
        }
    }
}
