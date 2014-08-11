using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Data;
using IMonitorService.Code.V2;
using System.Web.Script.Serialization;

public partial class InfoMaintenance_DeviceInfoJSON : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string url = Request.Url.ToString();
        ArrayList list = new ArrayList();
        string str = string.Empty;

        if (Request.HttpMethod == "GET")
        {
            if (url.IndexOf("status") != -1)
            {
                string query = Request.QueryString["status"].ToString();
                if (query.ToUpper() == "ALL")
                {
                    DataSet ds = SqlHelper.GetDeviceInformation();
                    int count = ds.Tables[0].Rows.Count;
                    for (int i = 0; i < count; i++)
                    {
                        DeviceInformation device = new DeviceInformation();
                        device.DeviceID = ds.Tables[0].Rows[i]["deviceID"].ToString();
                        device.DeviceName = ds.Tables[0].Rows[i]["deviceName"].ToString();
                        device.IPRule = ds.Tables[0].Rows[i]["ipRule"].ToString();
                        device.Disabled = ds.Tables[0].Rows[i]["disabled"].ToString() == "False" ? "启用" : "禁用";
                        device.STime = ds.Tables[0].Rows[i]["stime"].ToString();
                        device.ETime = ds.Tables[0].Rows[i]["etime"].ToString();
                        device.Cycle = ds.Tables[0].Rows[i]["cycle"].ToString();
                        list.Add(device);
                    }
                }
            }
            else
            {
                Response.Write("This is Iwooo Monitor System");
                Response.End();
                return;
            }
        }
        else if (Request.HttpMethod == "POST")
        {
            string oper = Request.Form["oper"].ToString();

            if (oper == "edit")
            {
                string deviceID = Request.Form["DeviceID"].ToString();
                string deviceName = Request.Form["DeviceName"].ToString();
                string ipRule = Request.Form["IPRule"].ToString();
                string disabled = Request.Form["Disabled"].ToString();
                string stime = Request.Form["STime"].ToString();
                string etime = Request.Form["ETime"].ToString();
                string cycle = Request.Form["Cycle"].ToString();
                SqlHelper.UpdateDeviceInformation(deviceID, deviceName, ipRule, disabled, stime, etime, cycle);
            }
            else if (oper == "del")
            {
                string deviceID = Request.Form["name"].ToString();
                SqlHelper.DeleteDeviceInformation(deviceID);
            }
            else if (oper == "add")
            {
                string deviceName = Request.Form["DeviceName"].ToString();
                string ipRule = Request.Form["IPRule"].ToString();
                string disabled = Request.Form["Disabled"].ToString();
                string stime = Request.Form["STime"].ToString();
                string etime = Request.Form["ETime"].ToString();
                string cycle = Request.Form["Cycle"].ToString();
                SqlHelper.InsertDeviceInformation(deviceName, ipRule, disabled, stime, etime, cycle);
            }
        }

        JavaScriptSerializer json = new JavaScriptSerializer();
        str = json.Serialize(list);

        Response.Write(str);
        Response.End();
        return;
    }
}