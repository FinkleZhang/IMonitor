using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IMonitorService.Code.V2
{
    public class StoreAlertEmail
    {
        public string StoreNo { get; set; }
        public string StoreRegion { get; set; }
        public string StoreType { get; set; }
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string DownCount { get; set; }
        public string Alert2 { get; set; }
        public string Alert30 { get; set; }
        public string EmailAddress { get; set; }

        public bool IsSend { get; set; }
    }
}
