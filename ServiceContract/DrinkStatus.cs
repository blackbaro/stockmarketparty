using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DrinkServiceContract
{
    public class DrinkStatus
    {
        public int SecondsLeft { get; set; }
        public int CrashSecondsLeft { get; set; }

        public DateTime LastCalculation { get; set; }
    }
}
