using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IMonitorService.Code.V2
{
    public class DeviceInformation
    {
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string IPRule { get; set; }
        public string Disabled { get; set; }
        public string STime { get; set; }
        public string ETime { get; set; }
        public string Cycle { get; set; }

        public DeviceInformation() { }

        public DeviceInformation(string deviceID, string deviceName, string ipRule)
        {
            DeviceID = deviceID;
            DeviceName = deviceName;
            IPRule = ipRule;
        }
    }
}
