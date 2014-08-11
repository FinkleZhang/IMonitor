using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IMonitorService.Code.V2
{
    public class StoreInformation : StoreBaseInformation
    {
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string IP { get; set; }
        public string Disabled { get; set; }
        public string STime { get; set; }
        public string ETime { get; set; }
        public string Cycle { get; set; }

    }
}
