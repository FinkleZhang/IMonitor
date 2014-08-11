using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IMonitorService.Code.V2
{
    public class DeviceMonitorInfomation
    {
        public string StoreNo { get; set; }
        public string StoreRegion { get; set; }
        public string StoreType { get; set; }
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string IP { get; set; }
        public string DeviceNetwork { get; set; }
        public string RecordTime { get; set; }
        public string Ping { get; set; } // 数据库层面为int
        public string N { get; set; }

        public int I { get; set; } // 代表Ping序列中的编号
        public int Total { get; set; }
        public List<DeviceMonitorInfomation> DeviceList { get; set; } // 引用外部对象
        public string[] PingCompletedFlag { get; set; } // 引用外部对象
        public int IPCount { get; set; }
    }
}
