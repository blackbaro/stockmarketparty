using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DrinkServiceContract
{
    [DataContract]
    public class DrinkConfig
    {
        public DrinkConfig()
        {
            SecondsBetweenRecalculation = 300;
            CrashSeconds = 100;
            SecondsTillNextCrash = 3600;
            Sensibility =70;
            PriceInterval = 10;
            FontSize = 18;
            LastChangeDate = DateTime.Now;
        }

        [DataMember]
        public Guid Token { get; set; }

        [DataMember]
        public DateTime LastChangeDate { get; set; }

        [DataMember]
        public int SecondsBetweenRecalculation { get; set; }      

        [DataMember]
        public float Sensibility { get; set; }

        [DataMember]
        public int CrashSeconds { get; set; }

        [DataMember]
        public int PriceInterval { get; set; }

        [DataMember]
        public int FontSize { get; set; }

        [DataMember]
        public bool? TurnLogo { get; set; }

        [DataMember]
        public int SecondsTillNextCrash { get; set; }
    }
}
