using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IMonitorService.Code.V2
{
    public class DeviceMonitorReport
    {
        public string StoreNo { get; set; }
        public string StoreRegion { get; set; }
        public string StoreType { get; set; }
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string Uptime { get; set; }
        public string Downtime { get; set; }
        public string Ratio { get; set; }
        public string Ping { get; set; }        
    }
}
