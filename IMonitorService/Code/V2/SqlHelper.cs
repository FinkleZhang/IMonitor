using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace IMonitorService.Code.V2
{
    public class SqlHelper
    {
        #region 连接字符串

        //private static string connRemote = @"Data Source=10.15.130.78,51433;Initial Catalog=LUXERP;User ID=sa;Password=portal123;Max Pool Size = 512;Connection Timeout=15;";
        private static string connRemote = @"Data Source=10.15.123.110;Initial Catalog=LUXERP;User ID=sa;Password=iwooo2014;Max Pool Size = 512;Connection Timeout=15;";
        //private static string connLocal = @"Data Source=10.15.140.110;Initial Catalog=IMonitor;User ID=iwooo;Password=iwooo2013;Max Pool Size = 512;Connection Timeout=15;";
        private static string connLocal = @"Data Source=.;Initial Catalog=IMonitorV2;User ID=sa;Password=Sikong1986;Max Pool Size = 512;Connection Timeout=15;";
        
        #endregion

        #region 通用

        public static DataSet GetOpeningStores()
        {
            DataSet ds = new DataSet();
            using (SqlConnection con = new SqlConnection(connRemote))
            {
                string sql = "select StoreNo, Region, StoreType from dbo.tb_Stores where StoreState='900';";
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = new SqlCommand(sql, con);

                con.Open();
                da.Fill(ds);
                con.Close();                
            }
            return ds;
        }

        public static void CommonBulkInsert(DataTable dt, string tableName)
        {
            int count = dt.Rows.Count;
            if (count == 0)
            {
                return;
            }
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                using (SqlBulkCopy copy = new SqlBulkCopy(conn))
                {
                    copy.BatchSize = count;
                    copy.DestinationTableName = tableName;
                    for (int i = 0, l = dt.Columns.Count; i < l; i++)
                    {
                        copy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                    }
                    conn.Open();
                    copy.WriteToServer(dt);
                    conn.Close();
                }
            }
        }

        public static int ExecuteNonQuery(string spName, SqlParameter[] paras)
        {
            int rows;
            using (SqlConnection con = new SqlConnection(connLocal))
            {
                using (SqlCommand com = new SqlCommand())
                {
                    com.Connection = con;
                    com.CommandText = spName;
                    com.CommandType = CommandType.StoredProcedure;
                    com.CommandTimeout = 300;
                    if (paras != null)
                    {
                        com.Parameters.AddRange(paras);
                    }
                    con.Open();
                    rows = com.ExecuteNonQuery();
                    con.Close();
                }
            }
            return rows;
        }

        public static DataSet ExecuteDataSet(string spName, SqlParameter[] paras)
        {
            DataSet ds = new DataSet();
            using (SqlConnection con = new SqlConnection(connLocal))
            {
                using (SqlCommand com = new SqlCommand())
                {
                    SqlDataAdapter ad = new SqlDataAdapter();
                    com.Connection = con;
                    ad.SelectCommand = com;

                    com.CommandText = spName;
                    com.CommandType = CommandType.StoredProcedure;
                    com.CommandTimeout = 300;
                    if (paras != null)
                    {
                        com.Parameters.AddRange(paras);
                    }
                    con.Open();
                    ad.Fill(ds);
                    con.Close();
                }
            }
            return ds;
        }

        #endregion

        #region 门店基础信息操作

        private static void TruncateStoreBaseInformationTemp()
        {
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "truncate table dbo.StoreBaseInformationTemp;";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        // 需要VPN或者ERP数据库连接
        private static void InsertStoreBaseInformationTemp()
        {
            DataSet ds = SqlHelper.GetOpeningStores();
            List<StoreBaseInformation> list = new List<StoreBaseInformation>();
            int count = ds.Tables[0].Rows.Count;
            for (int i = 0; i < count; i++)
            {               
                StoreBaseInformation storeBase = new StoreBaseInformation();
                storeBase.StoreNo = ds.Tables[0].Rows[i]["StoreNo"].ToString();
                storeBase.StoreRegion = ds.Tables[0].Rows[i]["Region"].ToString();
                storeBase.StoreType = ds.Tables[0].Rows[i]["StoreType"].ToString();
                storeBase.EmailAddress = storeBase.StoreRegion == "HK" ? 
                    storeBase.StoreNo + "Store@luxottica.com.hk" : storeBase.StoreNo + "Store@luxottica.com.cn";
                storeBase.PrinterType = "";
                storeBase.TonerType = "";
                storeBase.LaptopCount = "2";                

                list.Add(storeBase);
            }

            string[] clist = { "storeNo", "storeRegion", "storeType", "emailAddress", "printerType", "tonerType", "laptopCount" };
            DataTable dt = new DataTable();
            foreach (string colname in clist)
            {
                dt.Columns.Add(colname);
            }
            int rowcount = list.Count;
            for (int i = 0; i < rowcount; i++)
            {
                DataRow row = dt.NewRow();
                row["storeNo"] = list[i].StoreNo;
                row["storeRegion"] = list[i].StoreRegion;
                row["storeType"] = list[i].StoreType;
                row["emailAddress"] = list[i].EmailAddress;
                row["printerType"] = list[i].PrinterType;
                row["tonerType"] = list[i].TonerType;
                row["laptopCount"] = list[i].LaptopCount;                
                dt.Rows.Add(row);
            }
            TruncateStoreBaseInformationTemp();
            SqlHelper.CommonBulkInsert(dt, "StoreBaseInformationTemp");
        }

        // 同步门店基础信息，同时更新门店详细信息表的相关信息
        public static void SyncStoreBaseInformation()
        {
            InsertStoreBaseInformationTemp();
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "insert dbo.StoreBaseInformation select * from dbo.StoreBaseInformationTemp where storeNo not in(select storeNo from dbo.StoreBaseInformation);"; // 添加新营业的店到门店基础信息表
                sql += "delete dbo.StoreBaseInformation where storeNo not in(select storeNo from dbo.StoreBaseInformationTemp);"; // 删除非营业的店，也会删除手工增加但不在ERP系统里的门店信息
                sql += "exec dbo.SyncStoreInformation;"; // 将门店基础信息更新到详细信息表
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }
        
        // 同时会更新门店详细信息表的相关信息
        public static void UpdateStoreBaseInformation(StoreBaseInformation storeBase)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@storeNo",storeBase.StoreNo),
                                        new SqlParameter("@storeRegion",storeBase.StoreRegion),
                                        new SqlParameter("@storeType",storeBase.StoreType),
                                        new SqlParameter("@emailAddress",storeBase.EmailAddress),
                                        new SqlParameter("@printerType",storeBase.PrinterType),
                                        new SqlParameter("@tonerType",storeBase.TonerType),
                                        new SqlParameter("@laptopCount",storeBase.LaptopCount)                                     
                                   };
            SqlHelper.ExecuteNonQuery("UpdateStoreBaseInformation", paras);
        }

        // 同时会删除门店详细信息表的相关信息
        public static void DeleteStoreBaseInformation(string storeNo)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@storeNo",storeNo)                                  
                                   };
            SqlHelper.ExecuteNonQuery("DeleteStoreBaseInformation", paras);
        }

        // 同时会增加门店详细信息表的相关信息
        public static void InsertStoreBaseInformation(StoreBaseInformation storeBase)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@storeNo",storeBase.StoreNo),
                                        new SqlParameter("@storeRegion",storeBase.StoreRegion),
                                        new SqlParameter("@storeType",storeBase.StoreType),
                                        new SqlParameter("@emailAddress",storeBase.EmailAddress),
                                        new SqlParameter("@printerType",storeBase.PrinterType),
                                        new SqlParameter("@tonerType",storeBase.TonerType),
                                        new SqlParameter("@laptopCount",storeBase.LaptopCount)                                     
                                   };
            SqlHelper.ExecuteNonQuery("InsertStoreBaseInformation", paras);
        }

        public static DataSet GetStoreBaseInformation()
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "select * from dbo.StoreBaseInformation order by storeNo;";
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = new SqlCommand(sql, conn);
                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        public static DataSet GetStoreBaseInformation(StoreBaseInformation storeBase)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@storeNo",storeBase.StoreNo),
                                        new SqlParameter("@storeRegion",storeBase.StoreRegion),
                                        new SqlParameter("@storeType",storeBase.StoreType),
                                        new SqlParameter("@printerType",storeBase.PrinterType),
                                        new SqlParameter("@tonerType",storeBase.TonerType),
                                        new SqlParameter("@laptopCount",storeBase.LaptopCount)
                                   };
            return SqlHelper.ExecuteDataSet("GetStoreBaseInformation", paras);
        }

        #endregion

        #region 门店详细信息操作

        public static void UpdateStoreInformation(string storeNo, string deviceID, string ip, string disabled, string stime, string etime, string cycle)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@storeNo",storeNo),
                                        new SqlParameter("@deviceID",deviceID),
                                        new SqlParameter("@ip",ip),
                                        new SqlParameter("@disabled",disabled),
                                        new SqlParameter("@stime",stime),
                                        new SqlParameter("@etime",etime),
                                        new SqlParameter("@cycle",cycle),
                                   };
            SqlHelper.ExecuteNonQuery("UpdateStoreInformation", paras);
        }

        // 用来填充StoreInformation类
        public static DataSet GetStoreInformation(string storeNo)
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "select * from dbo.StoreInformation where storeNo=@storeNo;";
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = new SqlCommand(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@storeNo", storeNo);

                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        // 不包含HK StoreInformation类
        public static DataSet GetStorePrinter()
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "select * from dbo.StoreInformation where deviceID='D07' and disabled=0 and storeRegion<>'HK';";
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = new SqlCommand(sql, conn);                

                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        // 用来填充Store类
        public static DataSet GetStore()
        {
            return SqlHelper.ExecuteDataSet("GetStore", null);           
        }

        // 用来填充Store类, 获取单条
        public static DataSet GetStore(string storeNo)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@storeNo",storeNo)                         
                                   };
            return SqlHelper.ExecuteDataSet("GetStore", paras);
        }

        #endregion

        #region 设备信息操作

        public static void InsertDeviceInformation(string deviceName, string ipRule, string disabled, string stime, string etime, string cycle)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@deviceName",deviceName),
                                        new SqlParameter("@ipRule",ipRule),
                                        new SqlParameter("@disabled",disabled),
                                        new SqlParameter("@stime",stime),
                                        new SqlParameter("@etime",etime),
                                        new SqlParameter("@cycle",cycle),
                                   };
            SqlHelper.ExecuteNonQuery("InsertDeviceInformation", paras);
        }

        public static void DeleteDeviceInformation(string deviceID)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@deviceID",deviceID)                  
                                   };
            SqlHelper.ExecuteNonQuery("DeleteDeviceInformation", paras);
        }

        public static void UpdateDeviceInformation(string deviceID, string deviceName, string ipRule, string disabled, string stime, string etime, string cycle)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@deviceID",deviceID),
                                        new SqlParameter("@deviceName",deviceName),
                                        new SqlParameter("@ipRule",ipRule),
                                        new SqlParameter("@disabled",disabled),
                                        new SqlParameter("@stime",stime),
                                        new SqlParameter("@etime",etime),
                                        new SqlParameter("@cycle",cycle),
                                   };
            SqlHelper.ExecuteNonQuery("UpdateDeviceInformation", paras);
        }

        public static DataSet GetDeviceInformation()
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "select * from dbo.DeviceInformation";
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = new SqlCommand(sql, conn);               

                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        // 系统初始化时调用
        public static void CreateDeviceMonitorTable()
        {
            SqlHelper.ExecuteNonQuery("CreateDeviceMonitorTable", null);
        }

        #endregion

        #region 设备监控信息操作

        public static void InsertDeviceMonitorInformation(DeviceMonitorInfomation dmi)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@storeNo",dmi.StoreNo),
                                        new SqlParameter("@storeRegion",dmi.StoreRegion),
                                        new SqlParameter("@storeType",dmi.StoreType), 
                                        new SqlParameter("@deviceID",dmi.DeviceID), 
                                        new SqlParameter("@deviceName",dmi.DeviceName),
                                        new SqlParameter("@deviceName",dmi.IP),
                                        new SqlParameter("@deviceNetwork",dmi.DeviceNetwork), 
                                        new SqlParameter("@recordTime",dmi.RecordTime.ToString()), 
                                        new SqlParameter("@ping",dmi.Ping), 
                                   };
            SqlHelper.ExecuteNonQuery("InsertDeviceMonitorInformation", paras);
        }

        public static DataSet GetDeviceUptimeReport(string deviceID, string currentDay, string week, string month, string year, string sdate, string edate)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@deviceID",deviceID),
                                        new SqlParameter("@currentDay",currentDay),
                                        new SqlParameter("@week",week), 
                                        new SqlParameter("@month",month), 
                                        new SqlParameter("@year",year), 
                                        new SqlParameter("@sdate",sdate), 
                                        new SqlParameter("@edate",edate)
                                   };
            return SqlHelper.ExecuteDataSet("GetDeviceUptimeReport", paras);
        }

        // 更新报警表信息表的Ping DOWN数量
        public static void UpdateDeviceAlertTable(string deviceID)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@deviceID",deviceID)
                                   };
            SqlHelper.ExecuteNonQuery("UpdateDeviceAlertTable", paras);
        }

        public static DataSet GetStoresDeviceIP(string deviceID)
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "select * from dbo.StoreInformation where deviceID=@deviceID and disabled = 0 and cast(convert(nvarchar(10),GETDATE(),108) as datetime) between cast(stime as datetime) and cast(etime as datetime) ";
                if (deviceID == "D02") 
                {
                    sql += " and storeType<>'iFocus' ";
                }
                else if (deviceID == "D04") // 根据笔记本数量排除笔记本2
                {
                    sql += " and laptopCount=2 ";
                }
                sql += " order by storeNo;";
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = new SqlCommand(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@deviceID", deviceID);

                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        // 获取需要发送报警邮件的设备信息, 填充StoreAlertEmail类
        public static DataSet GetDevicesAlert(string deviceID, string alertType)
        {
            string downCount = "";
            switch (alertType)
            {
                case "Alert2":
                    downCount = "2";
                    break;
                case "Alert30":
                    downCount = "30";
                    break;
                case "Alert0":
                    downCount = "";
                    break;
            }

            SqlParameter[] paras = {
                                        new SqlParameter("@deviceID",deviceID),
                                        new SqlParameter("@downCount",downCount)
                                   };
            return SqlHelper.ExecuteDataSet("GetDevicesAlert", paras);
        }

        // 更新Alert2邮件状态，Alert30邮件状态，Alert0 恢复邮件状态
        public static void UpdateDevicesAlert(List<StoreAlertEmail> storeAlert, string alertType)
        {
            if (storeAlert == null || storeAlert.Count == 0)
                return;

            string deviceID = storeAlert[0].DeviceID;
            string downCount = storeAlert[0].DownCount;
            string sql = "";
            switch (alertType)
            {
                case "Alert2":
                    sql = "update dbo.A" + deviceID.Substring(1, 2) + " set alert2  = 1 where downCount >= @downCount and downCount <= 29 and alert2 = 0 and storeNo in('',";
                    break;
                case "Alert30":
                    sql = "update dbo.A" + deviceID.Substring(1, 2) + " set alert30 = 1 where downCount >= @downCount and alert30 = 0 and storeNo in('',";
                    break;
                case "Alert0":
                    sql = "update dbo.A" + deviceID.Substring(1, 2) + " set alert2  = 0, alert30 = 0 where downCount=0 and (alert30 = 1 or alert2 = 1) and storeNo in('',";
                    break;
            }
            
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                foreach (StoreAlertEmail email in storeAlert)
                {
                    if (email.IsSend)
                    {
                        sql += "'" + email.StoreNo + "',";
                    }                    
                }
                sql = sql.TrimEnd(',') + ");";               
                
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@downCount", downCount);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        // 每周运行一次
        public static void InsertDeviceMonitorWeek(string deviceID, string week, string sdate, string edate)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@deviceID",deviceID),
                                        new SqlParameter("@week",week),
                                        new SqlParameter("@sdate",sdate),
                                        new SqlParameter("@edate",edate),
                                   };
            SqlHelper.ExecuteNonQuery("InsertDeviceMonitorWeek", paras);
        }

        // 每月运行一次
        public static void InsertDeviceMonitorMonth(string deviceID, string month, string sdate, string edate)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@deviceID",deviceID),
                                        new SqlParameter("@month",month),
                                        new SqlParameter("@sdate",sdate),
                                        new SqlParameter("@edate",edate),
                                   };
            SqlHelper.ExecuteNonQuery("InsertDeviceMonitorMonth", paras);
        }

        public static DataSet GetDeviceCurMonitor(string deviceID)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@deviceID",deviceID)
                                   };
            return SqlHelper.ExecuteDataSet("GetDeviceCurMonitor", paras);
        }

        #endregion

        #region 邮件信息操作

        public static void InsertEmailInformation(string level, string emailAddress)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@level",level),
                                        new SqlParameter("@emailAddress",emailAddress)
                                   };
            SqlHelper.ExecuteNonQuery("InsertEmailInformation", paras);
        }

        public static void DeleteEmailInformation(string level)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@level",level)
                                   };
            SqlHelper.ExecuteNonQuery("DeleteEmailInformation", paras);
        }

        public static void UpdateEmailInformation(string level, string emailAddress)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@level",level),
                                        new SqlParameter("@emailAddress",emailAddress)
                                   };
            SqlHelper.ExecuteNonQuery("UpdateEmailInformation", paras);
        }

        public static DataSet GetEmailInformation()
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "select * from dbo.EmailInformation order by level";
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = new SqlCommand(sql, conn);

                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }       

        #endregion

        #region 打印机基础信息操作

        public static void InsertPrinterBaseInformation(string printerType, string tonerType)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@printerType",printerType),
                                        new SqlParameter("@tonerType",tonerType)
                                   };
            SqlHelper.ExecuteNonQuery("InsertPrinterBaseInformation", paras);
        }

        public static void DeletePrinterBaseInformation(string printerType)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@printerType",printerType)
                                   };
            SqlHelper.ExecuteNonQuery("DeletePrinterBaseInformation", paras);
        }

        public static void UpdatePrinterBaseInformation(string printerType, string tonerType)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@printerType",printerType),
                                        new SqlParameter("@tonerType",tonerType)
                                   };
            SqlHelper.ExecuteNonQuery("UpdatePrinterBaseInformation", paras);
        }

        public static DataSet GetPrinterBaseInformation()
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "select * from dbo.PrinterBaseInformation";
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = new SqlCommand(sql, conn);

                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        public static void DelCurDatePrinterInformation()
        {
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "delete dbo.PrinterInformation where convert(nvarchar(10),date,127) = convert(nvarchar(10),GETDATE(),127)";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        #endregion

        #region 设备报警规则信息操作

        public static void InsertDeviceAlertInformation(string deviceID, string deviceName, string deviceEName, string levelCode, string alertCode, string alertMessage)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@deviceID",deviceID),
                                        new SqlParameter("@deviceName",deviceName),
                                        new SqlParameter("@deviceEName",deviceEName),
                                        new SqlParameter("@levelCode",levelCode),
                                        new SqlParameter("@alertCode",alertCode),
                                        new SqlParameter("@alertMessage",alertMessage)
                                   };
            SqlHelper.ExecuteNonQuery("InsertDeviceAlertInformation", paras);
        }

        public static void DeleteDeviceAlertInformation(string id)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@id",id)
                                   };
            SqlHelper.ExecuteNonQuery("DeleteDeviceAlertInformation", paras);
        }

        public static void UpdateDeviceAlertInformation(string id, string deviceEName, string levelCode, string alertCode, string alertMessage)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@id",id),
                                        new SqlParameter("@deviceEName",deviceEName),
                                        new SqlParameter("@levelCode",levelCode),
                                        new SqlParameter("@alertCode",alertCode),
                                        new SqlParameter("@alertMessage",alertMessage),
                                   };
            SqlHelper.ExecuteNonQuery("UpdateDeviceAlertInformation", paras);
        }

        public static DataSet GetDeviceAlertInformation()
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "select * from dbo.DeviceAlertInformation order by deviceID";
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = new SqlCommand(sql, conn);

                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        public static DataSet GetDeviceAlertInformation(string deviceID, string levelCode)
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "select * from dbo.DeviceAlertInformation where deviceID=@deviceID and levelCode=@levelCode order by deviceID";
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = new SqlCommand(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@deviceID", deviceID);
                da.SelectCommand.Parameters.AddWithValue("@levelCode", levelCode);

                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        #endregion

        #region 打印机邮件发送

        public static void SyncSendEmail()
        {
            SqlHelper.ExecuteNonQuery("SyncSendEmail", null);
        }

        // 更新所有邮件发送状态, 打印机墨水大于10%则isSend = 0
        public static void UpdateIsSend()
        {
            SqlHelper.ExecuteNonQuery("UpdateSendEmail", null);
        }

        // 邮件发送成功，isSend = 1 否则 isSend = 0
        public static void UpdateIsSend(SendEmail email)
        {
            string sql = string.Empty;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                if (email.IsSend)
                {
                    sql = "update dbo.SendEmail set isSend=1, date=GETDATE() where storeStatus='900' and storeNo=@storeNo;";
                }
                else
                {
                    sql = "update dbo.SendEmail set isSend=0, date=GETDATE() where storeStatus='900' and storeNo=@storeNo;";
                }
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@storeNo", email.StoreNo);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void UpdateIsSend(List<SendEmail> emailList)
        {
            string sql0 = "update dbo.SendEmail set isSend=0, date=GETDATE() where storeStatus='900' and storeNo in('',";
            string sql1 = "update dbo.SendEmail set isSend=1, date=GETDATE() where storeStatus='900' and storeNo in('',";

            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                foreach (SendEmail email in emailList)
                {
                    if (email.IsSend)
                    {
                        sql1 += "'" + email.StoreNo + "',";
                    }
                    else
                    {
                        sql0 += "'" + email.StoreNo + "',";
                    }
                }
                sql0 = sql0.TrimEnd(',') + ");";
                sql1 = sql1.TrimEnd(',') + ");";
                string sql = sql0 + sql1;
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        // 获取邮件发送状态
        public static bool GetEmailIsSend(string storeNo)
        {
            string sql = string.Empty;
            bool isSend = false;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                sql = "select isSend from dbo.SendEmail where storeNo=@storeNo;";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@storeNo", storeNo);
                conn.Open();
                isSend = (bool)cmd.ExecuteScalar();
                conn.Close();
            }
            return isSend;
        }

        public static DataSet GetEmailIsSend(List<string> storesNo)
        {
            DataSet ds = new DataSet();
            string sql = string.Empty;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                sql = "select e.storeNo, isSend, emailAddress, s.storeRegion, s.printerType, s.tonerType, p.printerStatus, dbo.GetNumber(tonerStatus) cnt from dbo.SendEmail e left join dbo.StoreBaseInformation s on e.storeNo=s.storeNo left join dbo.PrinterInformation p on e.storeNo=p.storeNo where e.storeNo in('',";
                foreach (string storeNo in storesNo)
                {
                    sql += "'" + storeNo + "',";
                }
                sql = sql.TrimEnd(',') + ") order by e.storeNo;";
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sql, conn);
                da.SelectCommand = cmd;
                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        public static DataSet GetEmailSendResult()
        {
            DataSet ds = new DataSet();
            string sql = string.Empty;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                sql = "select p.storeNo, s.isSend from dbo.PrinterInformation p left join dbo.SendEmail s on p.storeNo=s.storeNo where convert(nvarchar(10),p.date,127) = convert(nvarchar(10),GETDATE(),127) and printerNetwork='UP' and dbo.GetNumber(tonerStatus) <= 10 order by p.storeNo";
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sql, conn);
                da.SelectCommand = cmd;
                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        public static DataSet GetLowInkPrinter()
        {
            DataSet ds = new DataSet();
            string sql = string.Empty;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                sql = "select storeNo from dbo.PrinterInformation where convert(nvarchar(10),date,127) = convert(nvarchar(10),GETDATE(),127) and printerNetwork='UP' and (dbo.GetNumber(tonerStatus) between 1 and 10 or printerStatus='10.0000 SUPPLY MEMORY ERROR') order by storeNo";
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sql, conn);
                da.SelectCommand = cmd;
                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        public static int GetPrinterCount(PrinterCondition pc)
        {
            DataSet ds = new DataSet();
            string sql = string.Empty;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                switch (pc)
                {
                    case PrinterCondition.All:
                        sql = "select count(*) total from dbo.PrinterInformation where convert(nvarchar(10),date,127) = convert(nvarchar(10),GETDATE(),127)";
                        break;
                    case PrinterCondition.Up: // 墨盒低于10%的数量
                        {
                            sql = "select count(*) total from dbo.PrinterInformation where convert(nvarchar(10),date,127) = convert(nvarchar(10),GETDATE(),127) and printerNetwork='UP' ";
                            sql += "and dbo.GetNumber(tonerStatus) between 1 and 10";
                        }
                        break;
                    case PrinterCondition.Down:
                        sql = "select count(*) total from dbo.PrinterInformation where convert(nvarchar(10),date,127) = convert(nvarchar(10),GETDATE(),127) and printerNetwork='DOWN'";
                        break;
                }
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sql, conn);
                da.SelectCommand = cmd;
                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
        }

        public static DataSet GetPrinterInformation(PrinterCondition pc)
        {
            DataSet ds = new DataSet();
            string sql = string.Empty;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                switch (pc)
                {
                    case PrinterCondition.All:
                        sql = "select * from dbo.PrinterInformation where convert(nvarchar(10),date,127) = convert(nvarchar(10),GETDATE(),127) order by storeNo";
                        break;
                    case PrinterCondition.Up:
                        {
                            sql = "select * from dbo.PrinterInformation where convert(nvarchar(10),date,127) = convert(nvarchar(10),GETDATE(),127) and printerNetwork='UP' ";
                            sql += "order by dbo.GetNumber(tonerStatus), storeNo";
                        }
                        break;
                    case PrinterCondition.Down:
                        sql = "select * from dbo.PrinterInformation where convert(nvarchar(10),date,127) = convert(nvarchar(10),GETDATE(),127) and printerNetwork='DOWN' order by storeNo";
                        break;
                }
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sql, conn);
                da.SelectCommand = cmd;
                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }


        #endregion

    }
}
