using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace IMonitorService.Code.V2
{
    public class Common
    {       
        public static int indexCount = 0;
        

        #region 门店设备Ping

        private static AutoResetEvent flag = new AutoResetEvent(false);

        // 获取每家门店某个监控设备的IP信息
        public static List<DeviceMonitorInfomation> GetStoreMonitorDevices(string deviceID)
        {
            List<DeviceMonitorInfomation> list = new List<DeviceMonitorInfomation>();
            DataSet ds = SqlHelper.GetStoresDeviceIP(deviceID); // 会去掉禁用该设备ID的门店信息
            int rows = ds.Tables[0].Rows.Count;
            for (int i = 0; i < rows; i++)
            {
                DeviceMonitorInfomation storeDevice = new DeviceMonitorInfomation();
                storeDevice.StoreNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                storeDevice.StoreRegion = ds.Tables[0].Rows[i]["storeRegion"].ToString();
                storeDevice.StoreType = ds.Tables[0].Rows[i]["storeType"].ToString();
                storeDevice.DeviceID = ds.Tables[0].Rows[i]["deviceID"].ToString();
                storeDevice.DeviceName = ds.Tables[0].Rows[i]["deviceName"].ToString();
                storeDevice.IP = ds.Tables[0].Rows[i]["IP"].ToString();
                storeDevice.IPCount = ds.Tables[0].Rows[i]["IP"].ToString().Split(';').Length;
                storeDevice.I = i;

                list.Add(storeDevice);
            }
            return list;
        }        
                
        // 多线程异步Ping
        public static bool PingDevicesAsync(string deviceID)
        {
            List<DeviceMonitorInfomation> list = GetStoreMonitorDevices(deviceID);
            if (list.Count == 0) return false;          

            const int storesCount = 30; // 每个线程30个门店

            int numberOfThreads = (int)Math.Ceiling(list.Count / (double)storesCount); 
            ManualResetEvent[] manualEvents = new ManualResetEvent[numberOfThreads];
            State  stateInfo;

            for (int i = 0; i < numberOfThreads; i++)
            {
                List<DeviceMonitorInfomation> l = list.Where(x => x.I >= (0 + storesCount * i)).ToList<DeviceMonitorInfomation>();
                l = l.Where(x => x.I < (storesCount + storesCount * i)).ToList<DeviceMonitorInfomation>();

                manualEvents[i] = new ManualResetEvent(false);
                stateInfo = new State(l, deviceID, manualEvents[i]);
                ThreadPool.QueueUserWorkItem(new WaitCallback(PingDevicesAsyncBase), stateInfo);
            }
            WaitHandle.WaitAll(manualEvents);
            Console.WriteLine("All Done!");
            return true;
        }

        // 多线程辅助类
        class State
        {
            public List<DeviceMonitorInfomation> list;
            public string deviceID;
            public ManualResetEvent manualEvent;

            public State(List<DeviceMonitorInfomation> list, string deviceID, ManualResetEvent manualEvent)
            {
                this.list = list;
                this.deviceID = deviceID;
                this.manualEvent = manualEvent;
            }
        }

        // 用来多线程跑
        private static void PingDevicesAsyncBase(object state)
        {
            State stateInfo = (State)state;
            List<DeviceMonitorInfomation> list = stateInfo.list;
            string deviceID = stateInfo.deviceID;

            List<DeviceMonitorInfomation> devices = new List<DeviceMonitorInfomation>();
            int count = list.Count;
            string[] completedFlag = new string[count];
            for (int i = 0; i < count; i++)
            {
                list[i].I = i;
                list[i].Total = count;
                list[i].DeviceList = devices;
                list[i].PingCompletedFlag = completedFlag;
                DeviceAssist(list[i]);
            }
            while (true)
            {
                if (count == PingCompletedFlagCount(completedFlag))
                {
                    string[] clist = { "storeNo", "storeRegion", "storeType", "deviceID", "deviceName", "ip", "deviceNetwork", "recordTime", "ping" };
                    DataTable dt = new DataTable();
                    foreach (string colName in clist)
                    {
                        dt.Columns.Add(colName);
                    }
                    int rowCount = devices.Count;
                    for (int i = 0; i < rowCount; i++)
                    {
                        DataRow row = dt.NewRow();
                        row["storeNo"] = devices[i].StoreNo;
                        row["storeRegion"] = devices[i].StoreRegion;
                        row["storeType"] = devices[i].StoreType;
                        row["deviceID"] = devices[i].DeviceID;
                        row["deviceName"] = devices[i].DeviceName;
                        row["ip"] = devices[i].IP;
                        row["deviceNetwork"] = devices[i].DeviceNetwork;
                        row["recordTime"] = devices[i].RecordTime;
                        row["ping"] = devices[i].Ping;
                        dt.Rows.Add(row);
                    }
                    SqlHelper.CommonBulkInsert(dt, deviceID);
                    Console.WriteLine("写入数据库成功！");
                    stateInfo.manualEvent.Set();
                    break;
                }
            }
        }

        private static int PingCompletedFlagCount(string[] flags)
        {
            int length = flags.Length;
            int count = 0;
            for (int i = 0; i < length; i++)
            {
                if (flags[i] == "True")
                    count++;
            }
            return count;
        }

        private static void DeviceAssist(DeviceMonitorInfomation device)
        {
            Ping p = new Ping();
            p.PingCompleted += DeviceCallback;

            string ip = device.IPCount == 1 ? device.IP : device.IP.Split(';')[0];  
            p.SendAsync(ip, 1000, device); // 超时1秒， 也就是每个DOWN都是1秒，如果DOWN过多，整个Ping就会很耗时                     
            flag.WaitOne();
        }

        private static void DeviceCallback(object sender, PingCompletedEventArgs e)
        {
            DeviceMonitorInfomation device = (DeviceMonitorInfomation)e.UserState; 
            try
            {
                device.DeviceNetwork = (e.Reply.Status == IPStatus.Success) ? "UP" : "DOWN";
                device.Ping = e.Reply.RoundtripTime.ToString();

                if (device.IPCount > 1 && device.DeviceNetwork == "DOWN")
                {
                    device.IP = device.IP.Split(';')[1];
                    device.IPCount = device.IPCount - 1;
                    flag.Set();
                    DeviceAssist(device);                    
                }
                else
                {
                    device.IP = device.IP.Split(';')[0];

                    device.RecordTime = DateTime.Now.ToString();
                    device.PingCompletedFlag[device.I] = "True";
                    device.DeviceList.Add(device);
                    Console.WriteLine((device.I + 1).ToString() + "/" + device.Total.ToString() + " " + device.StoreNo + ": " + device.DeviceNetwork);
                    flag.Set(); 
                }
            }
            catch (Exception ex)
            {
                device.DeviceNetwork = "DOWN";
                device.Ping = "0";

                if (device.IPCount > 1)
                {
                    device.IP = device.IP.Split(';')[1];
                    device.IPCount = device.IPCount - 1;
                    flag.Set();
                    DeviceAssist(device);                    
                }
                else
                {
                    Console.Write("{0} ", ex.Message);
                    device.RecordTime = DateTime.Now.ToString();
                    device.PingCompletedFlag[device.I] = "True";
                    device.DeviceList.Add(device);
                    Console.WriteLine((device.I + 1).ToString() + "/" + device.Total.ToString() + " " + device.StoreNo + ": " + device.DeviceNetwork);
                    flag.Set(); 
                }                
            }
        }

        #endregion

        #region 邮件发送

        public static void SendDeviceAlertMail(string deviceID, string levelCode, Dictionary<string, string> addresses)
        {
            DataSet ds = SqlHelper.GetDevicesAlert(deviceID, levelCode); // 获取DOWN数量的记录用来发送报警  

            DataSet mi = SqlHelper.GetDeviceAlertInformation(deviceID, levelCode); // 获取设备报警级别码的信息   
            string alertCode = mi.Tables[0].Rows[0]["alertCode"].ToString(); // 报警码
            string message = mi.Tables[0].Rows[0]["alertMessage"].ToString(); // 待发送的报警信息

            string emailAddress = addresses["Level 0"]; // 测试的时候发Level 0，正式的时候改为Level 1 
            List<string> cc = new List<string>();  

            List<StoreAlertEmail> list = new List<StoreAlertEmail>();
            int rows = ds.Tables[0].Rows.Count;
            for (int i = 0; i < rows; i++)
            {
                StoreAlertEmail alert = new StoreAlertEmail();
                alert.StoreNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                alert.StoreRegion = ds.Tables[0].Rows[i]["storeRegion"].ToString();
                alert.StoreType = ds.Tables[0].Rows[i]["storeType"].ToString();
                alert.DeviceID = ds.Tables[0].Rows[i]["deviceID"].ToString();
                alert.DeviceName = ds.Tables[0].Rows[i]["deviceName"].ToString();
                alert.DownCount = ds.Tables[0].Rows[i]["downCount"].ToString();
                alert.Alert2 = ds.Tables[0].Rows[i]["alert2"].ToString();
                alert.Alert30 = ds.Tables[0].Rows[i]["alert30"].ToString();
                alert.EmailAddress = ds.Tables[0].Rows[i]["emailAddress"].ToString();

                string subject = alert.StoreNo + "门店 " + alert.DeviceName + "报警 状态：" + alertCode;
                EmailFrom emailFrom = new EmailFrom("iMonitor@iwooo.com ", "1q2w3e4r", "59.60.9.101", 25);
                if (levelCode == "Alert30" || (levelCode == "Alert0" && alert.Alert30 == "1"))
                {
                    if (!cc.Contains(addresses["Level 4"])) cc.Add(addresses["Level 4"]); // 测试的时候发Level 4，正式的时候发Level 2
                }
                EmailHelper email = new EmailHelper(emailFrom, emailAddress, cc);

                if ((levelCode == "Alert2" && alert.Alert2 == "False") ||
                    (levelCode == "Alert30" && alert.Alert30 == "False") ||
                    (levelCode == "Alert0" && (alert.Alert2 == "True" || alert.Alert30 == "True")))
                {
                    if (email.SendMail(subject, message) == true)
                    {
                        alert.IsSend = true;
                        Console.WriteLine("{0} 发送成功！", alert.StoreNo);
                    }
                    else
                    {
                        alert.IsSend = false;
                        Console.WriteLine("{0} 发送失败！", alert.StoreNo);
                    }
                }
                else
                {
                    Console.WriteLine("{0} 已经发送过了！", alert.StoreNo);
                }
                list.Add(alert);
            }

            if(rows != 0) SqlHelper.UpdateDevicesAlert(list, levelCode); // 邮件发送状态更新到报警表
        }

        #endregion

        #region 打印机信息获取     
   
        const int defaultTimeout = 3 * 1000; // 打印机抓取超时，5秒

        // 获取不包含香港门店的打印机信息
        private static List<PrinterInformation> GetAllPrinter() 
        {
            List<PrinterInformation> list = new List<PrinterInformation>();
            DataSet ds = SqlHelper.GetStorePrinter(); 
            int rows = ds.Tables[0].Rows.Count;
            for (int i = 0; i < rows; i++)
            {
                PrinterInformation p = new PrinterInformation();
                p.StoreNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                p.StoreRegion = ds.Tables[0].Rows[i]["storeRegion"].ToString();
                p.StoreType = ds.Tables[0].Rows[i]["storeType"].ToString();
                p.PrinterType = ds.Tables[0].Rows[i]["printerType"].ToString();
                p.TonerType = ds.Tables[0].Rows[i]["tonerType"].ToString();

                p.IP = ds.Tables[0].Rows[i]["IP"].ToString();
                p.Url = "http://" + ds.Tables[0].Rows[i]["IP"].ToString();
                p.Html = "";
                p.I = i;
                p.StoreCount = rows;

                list.Add(p);
            }
            return list;
        }

        class RequestState
        {
            public HttpWebRequest Request { get; set; }            
            public PrinterInformation Printer { get; set; }
            public PrinterType PrinterType { get; set; }
            public bool IsIndexQuery { get; set; }

            public List<PrinterInformation> List;           
            public ManualResetEvent manualEvent;

            public RequestState(List<PrinterInformation> list, ManualResetEvent manualEvent)
            {
                this.List = list;                
                this.manualEvent = manualEvent;
            }

            public RequestState() { }
        }

        private static void GetPrinterType(RequestState state)
        {
            string html = state.Printer.Html;
            if (!String.IsNullOrEmpty(html))
            {
                int ps, pe;
                ps = html.IndexOf("<title>");
                pe = html.IndexOf("</title>");
                html = html.Substring(ps + 7, pe - ps - 7).Replace("&nbsp;", " ").ToUpper();
                if (html != "DEVICE STATUS") // HP1300第二次抓取忽略打印机类型
                {
                    state.Printer.PrinterType = html.Substring(0, html.IndexOf("10.1")).Trim();
                }

                if (html.IndexOf("HP LASERJET 400") != -1)
                {
                    state.PrinterType = PrinterType.HPM401;
                }
                else if (html.IndexOf("HP LASERJET 13") != -1)
                {
                    state.PrinterType = PrinterType.HP1300;
                    state.Printer.Url = "http://" + state.Printer.IP + "/hp/device/info_deviceStatus.html";
                    BeginResponse(state);
                }
                else if (html.IndexOf("HP LASERJET PROFESSIONAL P1606DN") != -1)
                {
                    state.PrinterType = PrinterType.HP1606;
                }
                else if (html.IndexOf("HP LASERJET P20") != -1)
                {
                    state.PrinterType = PrinterType.HP2000;
                }
            }
            else
            {
                state.PrinterType = PrinterType.NONE;
            }
        }

        private static void GetPrinterStatus(RequestState state, Pattern pat)
        {
            string printerStatus, tonerStatus, percent = string.Empty;
            int ps, pe, ts, te;
            string html = state.Printer.Html;
            PrinterInformation printer = state.Printer;
            try
            {
                printerStatus = html.Substring(html.IndexOf(pat.SearchString1)).Replace("&nbsp;", " ");
                tonerStatus = printerStatus.Substring(printerStatus.IndexOf(pat.SearchString2, pat.SearchStartIndex));
                if (!String.IsNullOrEmpty(pat.SearchString3))
                {
                    int idx = tonerStatus.IndexOf(pat.SearchString3) == -1 ? tonerStatus.IndexOf(pat.SearchString3N) : tonerStatus.IndexOf(pat.SearchString3);
                    percent = tonerStatus.Substring(idx);
                }

                ps = printerStatus.IndexOf(pat.Anchor1);
                pe = printerStatus.IndexOf(pat.Anchor2);
                printer.PrinterStatus = printerStatus.Substring(ps + pat.Anchor1.Length, pe - ps - pat.Anchor1.Length).Trim().Replace("<br>", "");

                ts = tonerStatus.IndexOf(pat.Anchor3);
                te = tonerStatus.IndexOf(pat.Anchor4);
                printer.TonerStatus = tonerStatus.Substring(ts + pat.Anchor3.Length, te - ts - pat.Anchor3.Length).Trim().Replace(pat.ReplaceString, pat.ReplaceString + " ");

                if (!string.IsNullOrEmpty(pat.SearchString3) && !string.IsNullOrEmpty(pat.Anchor5) && !string.IsNullOrEmpty(pat.Anchor6))
                {
                    int idxs, idxe;
                    idxs = percent.IndexOf(pat.Anchor5);
                    idxe = percent.IndexOf(pat.Anchor6);
                    percent = " " + percent.Substring(idxs + pat.Anchor5.Length, idxe - idxs - pat.Anchor5.Length).Trim();
                    printer.TonerStatus += percent;
                }
                printer.PrinterNetwork = "UP";
            }
            catch (System.Exception ex)
            {
                printer.PrinterStatus = ex.Message.ToString();
                printer.TonerStatus = ex.Message.ToString();
                printer.PrinterNetwork = "DOWN";
            }
        }

        private static void GetPrinterStatus(RequestState state)
        {
            string html = state.Printer.Html;
            Pattern pattern;
            switch (state.PrinterType)
            {
                case PrinterType.HP1606:
                    {
                        pattern = new Pattern("itemLargeFont", "mainContentArea", string.Empty, string.Empty, ">", "</td>", @"2"">", "</td>", string.Empty, string.Empty, 0, "Cartridge");
                        GetPrinterStatus(state, pattern);
                    }
                    break;
                case PrinterType.HP1300:
                    {
                        pattern = new Pattern(@"<font class=""if"">", @"<font class=""if"">", string.Empty, string.Empty, ">", "</font>", ">", "</font>", string.Empty, string.Empty, 50, "Cartridge");
                        GetPrinterStatus(state, pattern);
                    }
                    break;
                case PrinterType.HPM401:
                    {
                        pattern = new Pattern("itemLargeFont", "mainContentArea", @"<td class=""alignRight valignTop"">", string.Empty, ">", "<br>", "<td>", "<br>", ">", "</td>", 0, "Cartridge");
                        GetPrinterStatus(state, pattern);
                    }
                    break;
                case PrinterType.HP2000:
                    {
                        pattern = new Pattern("itemLargeFont", @"<td class=""tableDataCellStand width30"">", "<td>", @"<td class=""tableDataCellStand width25"" style=""vertical-align: bottom"">", ">", "</td>", ">", "</td>", ">", "</td>", 0, "Cartridge");
                        GetPrinterStatus(state, pattern);
                    }
                    break;
                default:
                    {
                        state.Printer.PrinterStatus = "不支持该打印机信息抓取";
                        state.Printer.TonerStatus = "不支持该打印机信息抓取";
                        state.Printer.PrinterNetwork = "DOWN";
                    }
                    break;
            }
            state.Printer.Date = DateTime.Now.ToString();
        }

        private static void TimeoutCallback(object state, bool timeOut)
        {
            if (timeOut)
            {
                HttpWebRequest request = (state as RequestState).Request;
                if (request != null)
                {
                    request.Abort();
                }
            }
        }

        private static void BeginResponse(RequestState state)
        {
            HttpWebRequest request = WebRequest.Create(state.Printer.Url) as HttpWebRequest;
            state.Request = request;
            IAsyncResult result = request.BeginGetResponse(new AsyncCallback(OnResponse), state);
            ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), state, defaultTimeout, true);
        }

        private static void OnResponse(IAsyncResult ar)
        {
            RequestState state = ar.AsyncState as RequestState;
            try
            {
                HttpWebRequest request = state.Request;
                HttpWebResponse response = request.EndGetResponse(ar) as HttpWebResponse;
                using (StreamReader read = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    string html = read.ReadToEnd();
                    state.Printer.Html = html;
                    GetPrinterType(state);
                    GetPrinterStatus(state);

                    Regex replaceSpace = new Regex(@"[\s]+", RegexOptions.IgnoreCase); // 去掉连续空格
                    string pstatus, tstatus;
                    pstatus = state.Printer.PrinterStatus.Replace("<br>", "").Replace("\n", "").Replace("</pre>", "").Trim();
                    tstatus = state.Printer.TonerStatus.Replace("<br>", "").Replace("\n", "").Replace("*", "").Trim();
                    pstatus = replaceSpace.Replace(pstatus, " ");
                    tstatus = replaceSpace.Replace(tstatus, " ");

                    state.Printer.PrinterStatus = pstatus;
                    state.Printer.TonerStatus = tstatus;
                    if (pstatus.IndexOf("StartIndex") == -1)
                    {
                        if (state.IsIndexQuery)
                        {
                            indexCount++;
                        }
                        else
                        {
                            state.Printer.PingCompletedFlag[state.Printer.I] = "True";
                            state.Printer.PrinterList.Add(state.Printer);
                            Console.WriteLine(state.Printer.I.ToString() + "/" + state.Printer.Total.ToString() + " " + state.Printer.StoreNo + ": " + state.Printer.PrinterStatus + " " + state.Printer.TonerStatus);
                        }
                    }
                }
                response.Close();
            }
            catch (System.Exception ex)
            {
                state.Printer.PrinterStatus = ex.Message;
                state.Printer.TonerStatus = ex.Message;
                state.Printer.PrinterNetwork = "DOWN";
                state.Printer.Date = DateTime.Now.ToString();
                if (state.IsIndexQuery)
                {
                    indexCount++;
                }
                else
                {
                    state.Printer.PingCompletedFlag[state.Printer.I] = "True";
                    state.Printer.PrinterList.Add(state.Printer);
                    Console.WriteLine(state.Printer.I.ToString() + "/" + state.Printer.Total.ToString() + " " + state.Printer.StoreNo + ": " + ex.Message.ToString());
                }
            }
        }

        public static void PingPrinterAsyncBase(object state)
        {
            RequestState requestState = (RequestState)state;
            List<PrinterInformation> printerList = requestState.List;
            int count = printerList.Count;
            string[] completedFlag = new string[count];
            
            List<PrinterInformation> list = new List<PrinterInformation>(); ;
            int storeCount = printerList.Count;        
   
            
            for (int i = 0; i < printerList.Count; i++)
            {
                RequestState rstate = new RequestState();
                PrinterInformation printer = new PrinterInformation();
                rstate.Printer = printer;

                rstate.Printer.StoreNo = printerList[i].StoreNo;
                rstate.Printer.StoreRegion = printerList[i].StoreRegion;
                rstate.Printer.StoreType = printerList[i].StoreType;
                rstate.Printer.PrinterType = printerList[i].PrinterType;
                rstate.Printer.TonerType = printerList[i].TonerType;                
                rstate.Printer.IP = printerList[i].IP;
                rstate.Printer.Url = printerList[i].Url;                
                rstate.Printer.StoreCount = printerList[i].StoreCount;

                rstate.Printer.I = i;
                rstate.Printer.Total = printerList.Count;
                rstate.Printer.PrinterList = list;
                rstate.Printer.PingCompletedFlag = completedFlag;

                rstate.IsIndexQuery = false;
                
                try
                {
                    if (new Ping().Send(rstate.Printer.IP).Status == IPStatus.Success)
                    {
                        BeginResponse(rstate);
                    }
                    else
                    {
                        rstate.Printer.PingCompletedFlag[rstate.Printer.I] = "True";
                        rstate.Printer.PrinterStatus = "打印机无法连接";
                        rstate.Printer.TonerStatus = "打印机无法连接";
                        rstate.Printer.PrinterNetwork = "DOWN";
                        rstate.Printer.Date = DateTime.Now.ToString();
                        rstate.Printer.PrinterList.Add(rstate.Printer);
                        Console.WriteLine(rstate.Printer.I.ToString() + "/" + rstate.Printer.Total.ToString() + " " + rstate.Printer.StoreNo + ": 打印机无法连接");
                    }
                }
                catch (System.Exception ex)
                {
                    rstate.Printer.PingCompletedFlag[rstate.Printer.I] = "True";
                    rstate.Printer.PrinterStatus = ex.Message;
                    rstate.Printer.TonerStatus = ex.Message;
                    rstate.Printer.PrinterNetwork = "DOWN";
                    rstate.Printer.Date = DateTime.Now.ToString();
                    rstate.Printer.PrinterList.Add(rstate.Printer);
                    Console.WriteLine(rstate.Printer.I.ToString() + "/" + rstate.Printer.Total.ToString() + " " + rstate.Printer.StoreNo + ": " + ex.Message.ToString());
                    continue;
                }
            }
            while (true)
            {
                if (count == PingCompletedFlagCount(completedFlag))
                {
                    string[] clist = { "storeNo", "storeRegion", "storeType", "printerNetwork", "printerStatus", "tonerStatus", "printerType", "tonerType", "date" };
                    DataTable dt = new DataTable();
                    foreach (string colname in clist)
                    {
                        dt.Columns.Add(colname);
                    }
                    int rowsCount = list.Count;
                    for (int i = 0; i < rowsCount; i++)
                    {
                        DataRow row = dt.NewRow();
                        row["storeNo"] = list[i].StoreNo;
                        row["storeRegion"] = list[i].StoreRegion;
                        row["storeType"] = list[i].StoreType;
                        row["printerNetwork"] = list[i].PrinterNetwork;
                        row["printerStatus"] = list[i].PrinterStatus;
                        row["tonerStatus"] = list[i].TonerStatus;
                        row["printerType"] = list[i].PrinterType;
                        row["tonerType"] = list[i].TonerType;
                        row["date"] = list[i].Date;
                        dt.Rows.Add(row);
                    }                    
                    SqlHelper.CommonBulkInsert(dt, "PrinterInformation");
                    Console.WriteLine("写入数据库成功！");
                    requestState.manualEvent.Set();                    
                    break;
                }
            }
        }

        // 多线程异步Ping
        public static bool PingPrinterAsync()
        {
            SqlHelper.DelCurDatePrinterInformation(); // 清掉上一次打印机数据

            List<PrinterInformation> list = GetAllPrinter();
            if (list.Count == 0) return false;

            const int storesCount = 30; // 每个线程30个门店

            int numberOfThreads = (int)Math.Ceiling(list.Count / (double)storesCount);
            ManualResetEvent[] manualEvents = new ManualResetEvent[numberOfThreads];
            RequestState stateInfo;

            for (int i = 0; i < numberOfThreads; i++)
            {
                List<PrinterInformation> l = list.Where(x => x.I >= (0 + storesCount * i)).ToList<PrinterInformation>();
                l = l.Where(x => x.I < (storesCount + storesCount * i)).ToList<PrinterInformation>();

                manualEvents[i] = new ManualResetEvent(false);
                stateInfo = new RequestState(l, manualEvents[i]);
                ThreadPool.QueueUserWorkItem(new WaitCallback(PingPrinterAsyncBase), stateInfo);
            }
            WaitHandle.WaitAll(manualEvents);
            Console.WriteLine("All Done!");
            return true;
        }

        #endregion

        #region 缺墨邮件

        public static void SendLowinkEmailPerStore()
        {
            // 同步门店信息，当日发送邮件状态同步
            SqlHelper.SyncSendEmail();
            SqlHelper.UpdateIsSend();

            // 获取缺墨的门店
            List<string> storeNos = new List<string>();
            DataSet lowink = SqlHelper.GetLowInkPrinter();
            for (int i = 0; i < lowink.Tables[0].Rows.Count; i++)
            {
                storeNos.Add(lowink.Tables[0].Rows[i]["storeNo"].ToString());
            }

            // 获取缺墨门店的发送邮件状态
            DataSet ds = SqlHelper.GetEmailIsSend(storeNos);
            int count = ds.Tables[0].Rows.Count;
            bool[] status = new bool[count]; // 记录发送邮件的状态

            EmailFrom emailFrom = new EmailFrom("iMonitor@iwooo.com ", "1q2w3e4r", "59.60.9.101", 25);

            List<string> cc = new List<string>();            
            cc.Add("iwooomonitor@163.com");
            //cc.Add("HelpDesk.IT@lrgc.com.cn");


            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                string storeNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                string isSend = ds.Tables[0].Rows[i]["isSend"].ToString();
                //string emailAddress = ds.Tables[0].Rows[i]["emailAddress"].ToString(); // 测试环境发给自己，正式环境发给门店
                string emailAddress = "zhanggb@iwooo.com";
                string region = ds.Tables[0].Rows[i]["storeRegion"].ToString();
                string printerType = ds.Tables[0].Rows[i]["printerType"].ToString();
                string tonerType = ds.Tables[0].Rows[i]["tonerType"].ToString();
                string printerStatus = ds.Tables[0].Rows[i]["printerStatus"].ToString();
                string cnt = ds.Tables[0].Rows[i]["cnt"].ToString();

                string subject = storeNo + " 门店缺墨提醒";
                string mailBody = " <span style=\"color: rgb(255, 0, 0); font-size: 32px;\">系统邮件，不用回复！</span><br>" + storeNo + "门店墨盒不足10%，请注意更换！ 如果需要采购新的硒鼓墨盒，请按以下格式填写信息后发送给（HelpDesk.IT@lrgc.com.cn） <br><hr><br>";
                if (cnt == "1")
                {
                    mailBody = " <span style=\"color: rgb(255, 0, 0); font-size: 32px;\">系统邮件，不用回复！</span><br>" + storeNo + "门店墨盒不足<span style=\"color: rgb(255, 0, 0); font-size: 32px;\">1%</span>，请尽快更换！ 如果需要采购新的硒鼓墨盒，请按以下格式填写信息后发送给（HelpDesk.IT@lrgc.com.cn） <br><hr><br>";
                }                
                mailBody += MailBodyMessage(storeNo, region, tonerType);

                string attach = null;
                string cd = System.IO.Directory.GetCurrentDirectory();
                int idx = cd.IndexOf("IMonitorAssist");
                if (printerStatus == "10.0000 SUPPLY MEMORY ERROR")
                {
                    subject = storeNo + " 门店硒鼓信息无法读取";
                    mailBody = " <span style=\"color: rgb(255, 0, 0); font-size: 32px;\">系统邮件，不用回复！</span><br>" + storeNo + "门店墨盒信息无法读取 <br><hr><br>";
                    mailBody += "由于后台检测发现硒鼓出现 10.0000 SUPPLY MEMORY ERROR 错误信息，请按照附件的图片检查是否将 硒鼓黄色拉环 去除。";
                    attach = cd.Substring(0,idx) + "IMonitorWeb\\contents\\XEFA.jpg";
                }

                EmailHelper email = new EmailHelper(emailFrom, emailAddress, cc);
                if (isSend == "False")
                {
                    if (email.SendMail(subject, mailBody, attach) == true)
                    {
                        status[i] = true;
                        Console.WriteLine("{0} 发送成功！", storeNo);
                    }
                    else
                    {
                        status[i] = false;
                        Console.WriteLine("{0} 发送失败！", storeNo);
                    }
                }
                else
                {
                    status[i] = true;
                    Console.WriteLine("{0} 已经发送过了！", storeNo);
                }
            }

            // 更新发送邮件状态到数据库
            List<SendEmail> sendEmail = new List<SendEmail>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                SendEmail sendemail = new SendEmail();
                sendemail.StoreNo = ds.Tables[0].Rows[i][0].ToString();
                sendemail.IsSend = status[i];
                sendEmail.Add(sendemail);
            }
            SqlHelper.UpdateIsSend(sendEmail);
        }

        public static string MailBodyMessage(string storeNo, string storeRegion, string tonerType)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sa = new StringBuilder();
            sa.Append("请给: " + storeNo + "店寄:    个硒豉，谢谢！ <br><br>");
            sa.Append("店号： " + storeNo + "<br>");
            sa.Append("店铺地址：<br>");
            sa.Append("联系电话：<br>");
            sa.Append("数量：<br>");
            sa.Append("硒鼓型号： " + tonerType + "<br>");
            sa.Append("联系人：<br>");
            switch (storeRegion)
            {
                case "SH":
                    {
                        sb.Append("上海区：供应商; 'Yang Qing' yangqing@tayee.com <br>");
                        sb.Append("HI杨生：<br>");
                        sb.Append(sa.ToString());
                        sb.Append("开票信息为：斯泽塔塞眼镜商贸(上海)有限公司<br>");
                        sb.Append("发票备注：汪乔芸 13969<br>");
                        sb.Append("请门店把旧的硒鼓寄回上海办公室：上海市徐汇区虹桥路3号港汇中心2座7层 IT部 刘文浩（收），电话：021-24113958.谢谢<br>");
                    }
                    break;
                case "BJ":
                    {
                        sb.Append("北京区：供应商：zhaofeng feng.zhao@servicesn.com，抄送Jingli.Du@luxotticaretail.com.cn <br>");
                        sb.Append("HI Feng:<br>");
                        sb.Append(sa.ToString());
                        sb.Append("开票信息为：斯泽塔塞眼镜商贸(北京)有限公司<br>");
                        sb.Append("发票备注：陈晓菲 11502<br>");
                        sb.Append("请门店把旧的硒鼓寄回上海办公室：上海市徐汇区虹桥路3号港汇中心2座7层 IT部 刘文浩（收），电话：021-24113958.谢谢<br>");
                    }
                    break;
                case "GD":
                    {
                        sb.Append("广东区：供应商：朋 376040021@qq.com <br>");
                        sb.Append("Hi 鹏:<br>");
                        sb.Append(sa.ToString());
                        sb.Append("开票信息：广州明廊眼镜技术有限公司<br>");
                        sb.Append("发票备注：Carman.Lei 13009<br>");
                    }
                    break;
                default:
                    {
                        sb.Append("西区：供应商；'Yang Qing' yangqing@tayee.com <br>");
                        sb.Append("HI 杨生:<br>");
                        sb.Append(sa.ToString());
                        sb.Append("开票信息：广州明廊眼镜技术有限公司<br>");
                        sb.Append("发票备注：吴春蓉 10798<br>");
                        sb.Append("请门店把旧的硒鼓寄回上海办公室：上海市徐汇区虹桥路3号港汇中心2座7层 IT部 刘文浩（收），电话：021-24113958.谢谢<br>");
                    }
                    break;
            }
            if (storeNo == "6171")
            {
                sb.Clear();
                sb.Append("北京区：供应商：zhaofeng feng.zhao@servicesn.com，抄送Jingli.Du@luxotticaretail.com.cn <br>");
                sb.Append("HI Feng:<br>");
                sb.Append(sa.ToString());
                sb.Append("开票信息为：北京斯明德商贸有限公司<br>");
                sb.Append("发票备注：陈晓菲11502<br>");
                sb.Append("请门店把旧的硒鼓寄回上海办公室：上海市徐汇区虹桥路3号港汇中心2座7层 IT部 刘文浩（收），电话：021-24113958.谢谢<br>");
            }
            if (storeNo == "6638" || storeNo == "6642")
            {
                sb.Clear();
                sb.Append("北京区：供应商：zhaofeng feng.zhao@servicesn.com，抄送Jingli.Du@luxotticaretail.com.cn <br>");
                sb.Append("HI Feng:<br>");
                sb.Append(sa.ToString());
                sb.Append("开票信息为：斯泽塔塞眼镜商贸(北京)有限公司<br>");
                sb.Append("发票备注：陈晓菲11502<br>");
                sb.Append("请门店把旧的硒鼓寄回上海办公室：上海市徐汇区虹桥路3号港汇中心2座7层 IT部 刘文浩（收），电话：021-24113958.谢谢<br>");
            }
            if (storeNo == "6560" || storeNo == "6456")
            {
                sb.Clear();
                sb.Append("上海区：供应商; 'Yang Qing' yangqing@tayee.com <br>");
                sb.Append("HI Feng:<br>");
                sb.Append(sa.ToString());
                sb.Append("开票信息为：斯泽塔塞眼镜商贸(北京)有限公司<br>");
                sb.Append("发票备注：汪乔芸 13969<br>");
                sb.Append("请门店把旧的硒鼓寄回上海办公室：上海市徐汇区虹桥路3号港汇中心2座7层 IT部 刘文浩（收），电话：021-24113958.谢谢<br>");
            }

            return sb.ToString();

        }

        #endregion
    }
}
