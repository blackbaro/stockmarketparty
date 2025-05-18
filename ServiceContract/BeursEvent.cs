using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceContract
{
    public class BeursEvent
    {
        public Guid Token { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Street { get; set; }
        public string StreetNumber { get; set; }        
        public string Name { get; set; }
        public DateTime Date { get; set; }

        public string Zipcode { get; set; }
    }
}
