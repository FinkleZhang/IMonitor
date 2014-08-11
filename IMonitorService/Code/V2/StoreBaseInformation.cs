using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IMonitorService.Code.V2
{
    public class StoreBaseInformation
    {
        public string StoreNo { get; set; }
        public string StoreRegion { get; set; }
        public string StoreType { get; set; }
        public string EmailAddress { get; set; }
        public string PrinterType { get; set; }
        public string TonerType { get; set; }
        public string LaptopCount { get; set; }        


        public StoreBaseInformation() { }

        public StoreBaseInformation(string storeNo, string storeRegion, string storeType, 
            string emailAddress, string printerType, string tonerType, string laptopCount)
        {
            StoreNo = storeNo;
            StoreRegion = storeRegion;
            StoreType = storeType;
            EmailAddress = emailAddress;
            PrinterType = printerType;
            TonerType = tonerType;
            LaptopCount = laptopCount;            
        }
    }
}
