#region using
//using Microsoft.InformationProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks; 
#endregion

namespace Common
{
    public class BankCoordinate : EntityBase
    {
        public BankCoordinate() { }

        [DataMember]
        public string CountryCode { get; set; }
        [DataMember]
        public string ABI { get; set; }
        [DataMember]
        public string CAB { get; set; }
        [DataMember]
        public string CIN { get; set; }
        [DataMember]
        public int? CHECK { get; set; }
        [DataMember]
        public SwiftCoordinate BIC { get; set; }
        [DataMember]
        public string IBAN { get; set; }
        [DataMember]
        public string Account { get; set; }
        [DataMember]
        public bool ICBPICard { get; set; }
    }

    public class SwiftCoordinate : EntityBase
    {
        public SwiftCoordinate() { }

        [DataMember]
        public string Bank { get; set; }
        [DataMember]
        public string Country { get; set; }
        [DataMember]
        public string Town { get; set; }
        [DataMember]
        public string Branch { get; set; }
        [DataMember]
        public string SwiftCode { get; set; }
    }
}
