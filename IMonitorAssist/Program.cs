using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using IMonitorService.Code;
using System.Diagnostics;
using System.Threading;
using System.Net.NetworkInformation;
using V2 = IMonitorService.Code.V2;


namespace IMonitorAssist
{
    class Program
    {           
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                switch (args[0].ToUpper())
                {
                    case "PRINT":
                        {
                            Common.DoGetPrinterInfomationTask();                            
                        }
                        break;
                    case "ROUTER":
                        {
                            Common.GetRouterTask();
                        }
                        break;
                    case "LAPTOP":
                        {
                            Common.DoGetLaptopInformationTask();
                        }
                        break;
                    case "INDEXQUERY":
                        {
                            string storeNo = args[1].ToString();
                            IndexQuery iq = Common.GetIndexData(storeNo);
                            SqlHelper.DeleteIndexQuery();
                            SqlHelper.InsertIndexQuery(iq);                            
                        }
                        break;
                    case "SENDEMAIL":
                        {
                            Common.SendLowinkEmailPerStore();
                        }
                        break;
                    case "PSBOTH":
                        {
                            Common.DoGetPrinterInfomationTask();
                            Common.SendLowinkEmailPerStore();
                        }
                        break;
                }
            }
            else
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                #region OldTest
                //Common.DoGetLaptopInformationTask();
                
                //Common.GetPrinterService("10.160.14.50");

                //IndexQuery iq = Common.GetIndexData("6608");
                //SqlHelper.DeleteIndexQuery();
                //SqlHelper.InsertIndexQuery(iq);
                //Console.WriteLine("StoreNo: " + iq.StoreNo);
                //Console.WriteLine("Router: " + iq.RouterNetwork);
                //Console.WriteLine("Printer: " + iq.PrinterNetwork);
                //Console.WriteLine("PStatus: " + iq.PrinterStatus);
                //Console.WriteLine("TStatus: " + iq.TonerStatus);
                //Console.WriteLine("Laptop: " + iq.LaptopNetwork);
                //Console.WriteLine("LapIP: " + iq.LaptopIP);

                //Common.DoGetPrinterInfomationTask();
                //Common.DoGetRouterInformationTask();
                //Common.DoGetLaptopInformationTask();
                

                //Common.SendLowinkEmailPerStore();

                //DataSet ds = SqlHelper.TonerSumReport("03", "2014", false);
                //for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                //{
                //    string storeNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                //    string storeRegion = ds.Tables[0].Rows[i]["storeRegion"].ToString();
                //    string storeType = ds.Tables[0].Rows[i]["storeType"].ToString();
                //    string tonerCount = ds.Tables[0].Rows[i]["tonerCount"].ToString();
                //    string storeStatus = ds.Tables[0].Rows[i]["storeStatus"].ToString();
                //}
                #endregion


                V2.Common.PingPrinterAsync();
                V2.Common.SendLowinkEmailPerStore();

                //string cd = System.IO.Directory.GetCurrentDirectory();
                //int idx = cd.IndexOf("IMonitorAssist");
                //string attach = cd.Substring(0, idx) + "IMonitorWeb\\contents\\XEFA.jpg";
                //Console.WriteLine(System.IO.Directory.GetCurrentDirectory());
                //Console.WriteLine(cd.Substring(0, idx) + "IMonitorWeb\\contents\\XEFA.jpg");

                //V2.EmailFrom emailFrom = new V2.EmailFrom("iMonitor@iwooo.com ", "1q2w3e4r", "59.60.9.101", 25);
                //V2.EmailHelper email = new V2.EmailHelper(emailFrom, "zhanggb@iwooo.com", null);
                //email.SendMail("Test", "Hello", attach);

                //int count = 31;
                //while (count != 0)
                //{
                //    count--;
                //    bool isDone = V2.Common.PingDevicesAsync("D01"); // 产生Ping数据
                //    if (isDone)
                //    {
                //        V2.SqlHelper.UpdateDeviceAlertTable("D01"); // 更新相应的报警表记录DOWN数量

                //        DataSet ad = V2.SqlHelper.GetEmailInformation(); // 获取所有邮件地址
                //        Dictionary<string, string> dict = new Dictionary<string, string>();
                //        for (int i = 0; i < ad.Tables[0].Rows.Count; i++)
                //        {
                //            dict.Add(ad.Tables[0].Rows[i]["level"].ToString(), ad.Tables[0].Rows[i]["emailAddress"].ToString());
                //        }


                //        V2.Common.SendDeviceAlertMail("D01", "Alert2", dict);
                //        V2.Common.SendDeviceAlertMail("D01", "Alert30", dict);
                //        V2.Common.SendDeviceAlertMail("D01", "Alert0", dict);
                //    }

                //    Thread.Sleep(2 * 60 * 1000);
                //}               
                
                        

                #region Store填充
                //List<IMonitorService.Code.V2.Store> stores = new List<IMonitorService.Code.V2.Store>();
                //DataSet ds = IMonitorService.Code.V2.SqlHelper.GetStore("1525");
                //int rows = ds.Tables[0].Rows.Count;
                //int cols = ds.Tables[0].Columns.Count;
                //for (int i = 0; i < rows; i++)
                //{
                //    IMonitorService.Code.V2.Store store = new IMonitorService.Code.V2.Store();
                //    store.StoreNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                //    store.StoreRegion = ds.Tables[0].Rows[i]["storeRegion"].ToString();
                //    store.StoreType = ds.Tables[0].Rows[i]["storeType"].ToString();
                //    store.EmailAddress = ds.Tables[0].Rows[i]["emailAddress"].ToString();
                //    store.PrinterType = ds.Tables[0].Rows[i]["printerType"].ToString();
                //    store.TonerType = ds.Tables[0].Rows[i]["tonerType"].ToString();
                //    store.LaptopCount = (int)ds.Tables[0].Rows[i]["laptopCount"];
                //    store.HaltDevice = ds.Tables[0].Rows[i]["haltDevice"].ToString();
                //    store.Devices = new Dictionary<string, string>();
                //    for (int j = 8; j < cols; j++)
                //    {
                //        string colName = ds.Tables[0].Columns[j].ColumnName;
                //        store.Devices.Add(colName, ds.Tables[0].Rows[i][j].ToString());
                                               
                //    }
                //    stores.Add(store);
                //    break;
                //}
                //Console.WriteLine(rows); 
                #endregion

                sw.Stop();
                double s = sw.ElapsedMilliseconds / 1000.0;
                Console.WriteLine(s.ToString());
            }
             
        }

       
    }
}
