using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IMonitorService.Code.V2
{
    // Store类在数据库层面由StoreInformation类 列旋转行而来
    public class Store : StoreBaseInformation
    {
        public Dictionary<string, string> Devices { get; set; }
    }
}
