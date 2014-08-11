using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IMonitorService.Code.V2
{
    public class DeviceAlertInformation
    {
        public string ID { get; set; }
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string DeviceEName { get; set; }
        public string LevelCode { get; set; }
        public string AlertCode { get; set; }
        public string AlertMessage { get; set; }
    }
}
