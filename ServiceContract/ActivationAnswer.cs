using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DrinkServiceContract
{
	[DataContract]
	public class ActivationAnswer
	{
		[DataMember]
		public bool IsValid { get; set; }
		[DataMember]
		public int Hours { get; set; }
		[DataMember]
		public bool IsTest { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool ResetLicense { get; set; }

        [DataMember]
        public byte[] Logo { get; set; }
	}
}
