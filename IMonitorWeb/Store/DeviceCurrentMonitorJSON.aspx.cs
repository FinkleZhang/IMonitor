using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using IMonitorService.Code.V2;
using System.Data;
using System.Web.Script.Serialization;

public partial class Store_DeviceCurrentMonitorJSON : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string url = Request.Url.ToString();
        ArrayList list = new ArrayList();
        string str = string.Empty;

        if (url.IndexOf("status") != -1)
        {
            string query = Request.QueryString["status"].ToString();
            if (query.ToUpper() == "QUERY")
            {
                string deviceID = Request.QueryString["device"].ToString();                

                DataSet ds = SqlHelper.GetDeviceCurMonitor(deviceID);
                int count = ds.Tables[0].Rows.Count;
                for (int i = 0; i < count; i++)
                {
                    DeviceMonitorInfomation report = new DeviceMonitorInfomation();
                    report.StoreNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                    report.StoreRegion = ds.Tables[0].Rows[i]["storeRegion"].ToString();
                    report.StoreType = ds.Tables[0].Rows[i]["storeType"].ToString();
                    report.DeviceID = ds.Tables[0].Rows[i]["deviceID"].ToString();
                    report.DeviceName = ds.Tables[0].Rows[i]["deviceName"].ToString();
                    report.IP = ds.Tables[0].Rows[i]["ip"].ToString();
                    report.DeviceNetwork = ds.Tables[0].Rows[i]["deviceNetwork"].ToString();
                    report.RecordTime = ds.Tables[0].Rows[i]["recordTime"].ToString();
                    report.Ping = ds.Tables[0].Rows[i]["ping"].ToString();
                    report.N = ds.Tables[0].Rows[i]["N"].ToString();

                    list.Add(report);
                }
            }
            else if (query.ToUpper() == "DEVICE")
            {
                DataSet ds = SqlHelper.GetDeviceInformation();
                int count = ds.Tables[0].Rows.Count;
                for (int i = 0; i < count; i++)
                {
                    DeviceInformation di = new DeviceInformation();
                    di.DeviceID = ds.Tables[0].Rows[i]["deviceID"].ToString();
                    di.DeviceName = ds.Tables[0].Rows[i]["deviceName"].ToString();

                    list.Add(di);
                }
            }            
        }
        else
        {
            Response.Write("This is Iwooo Monitor System");
            Response.End();
            return;
        }

        JavaScriptSerializer json = new JavaScriptSerializer();
        str = json.Serialize(list);

        Response.Write(str);
        Response.End();
        return;
    }
}