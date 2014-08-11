using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IMonitorService.Code.V2
{
    public class PrinterInformation
    {
        public string StoreNo { get; set; }
        public string StoreRegion { get; set; }
        public string StoreType { get; set; }
        public string PrinterNetwork { get; set; }
        public string PrinterStatus { get; set; }
        public string TonerStatus { get; set; }
        public string PrinterType { get; set; }
        public string TonerType { get; set; }
        public string Date { get; set; }

        public string IP { get; set; }
        public string Url { get; set; }
        public string Html { get; set; }
        public string Status { get; set; }

        public int I { get; set; } // 代表Ping序列中的编号
        public int Total { get; set; }
        public int StoreCount { get; set; }
        public List<PrinterInformation> PrinterList { get; set; } // 引用外部对象
        public string[] PingCompletedFlag { get; set; } // 引用外部对象
    }
}
