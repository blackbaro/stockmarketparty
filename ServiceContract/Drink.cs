using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DrinkServiceContract
{
    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class Drink
    {

        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string DrinkName { get; set; }
        Decimal m_currentPrice;

        [DataMember]
        public Decimal CurrentPrice
        {
            get { return Math.Max(MinPrice, m_currentPrice); }
            set { m_currentPrice = value; }
        }

        [DataMember]
        public Decimal? NextManualPrice { get; set; }

        [DataMember]
        public Decimal NextPrice { get; set; }

        [DataMember]
        public Decimal NormalPrice { get; set; }

        [DataMember]
        public Decimal MinPrice { get; set; }

        [DataMember]
        public int OrdersSinceLastCalculation { get; set; }

        [DataMember]
        public Decimal MaxPrice { get; set; }

        [DataMember]
        public int HotKey { get; set; }

        [DataMember]
        public DateTime LastEditDate { get; set; }

        public override bool Equals(object obj)
        {
            return ((Drink)obj).ID == ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
