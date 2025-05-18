using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DrinkServiceContract
{
    [DataContract]
    public class DrinkOrder
    {
        public DrinkOrder()
        {
            DrinkOrderID = Guid.NewGuid();
        }
        [DataMember]
        public Guid DrinkOrderID { get; set; }

        [DataMember]
        public Guid DrinkID { get; set; }

        [DataMember]
        public string DrinkName { get; set; }

        [DataMember]
        public DateTime OrderDate { get; set; }

        [DataMember]
        public Decimal Price { get; set; }

        [DataMember]
        public bool Published { get; set; }
    }
}
