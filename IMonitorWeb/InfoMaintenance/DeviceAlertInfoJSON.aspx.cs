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

public partial class InfoMaintenance_DeviceAlertInfoJSON : System.Web.UI.Page
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
                    DataSet ds = SqlHelper.GetDeviceAlertInformation();
                    int count = ds.Tables[0].Rows.Count;
                    for (int i = 0; i < count; i++)
                    {
                        DeviceAlertInformation device = new DeviceAlertInformation();
                        device.ID = ds.Tables[0].Rows[i]["id"].ToString();
                        device.DeviceID = ds.Tables[0].Rows[i]["deviceID"].ToString();
                        device.DeviceName = ds.Tables[0].Rows[i]["deviceName"].ToString();
                        device.DeviceEName = ds.Tables[0].Rows[i]["deviceEName"].ToString();
                        device.LevelCode = ds.Tables[0].Rows[i]["levelCode"].ToString();
                        device.AlertCode = ds.Tables[0].Rows[i]["alertCode"].ToString();
                        device.AlertMessage = ds.Tables[0].Rows[i]["alertMessage"].ToString();
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
                string id = Request.Form["ID"].ToString().Split(',')[0];                
                string deviceEName = Request.Form["DeviceEName"].ToString();
                string levelCode = Request.Form["LevelCode"].ToString();
                string alertCode = Request.Form["AlertCode"].ToString();
                string alertMessage = Request.Form["AlertMessage"].ToString();
                SqlHelper.UpdateDeviceAlertInformation(id, deviceEName, levelCode, alertCode, alertMessage);
            }
            else if (oper == "del")
            {
                string id = Request.Form["name"].ToString();
                SqlHelper.DeleteDeviceAlertInformation(id);
            }
            else if (oper == "add")
            {
                string deviceID = Request.Form["DeviceID"].ToString();
                string deviceName = Request.Form["DeviceName"].ToString();
                string deviceEName = Request.Form["DeviceEName"].ToString();
                string levelCode = Request.Form["LevelCode"].ToString();
                string alertCode = Request.Form["AlertCode"].ToString();
                string alertMessage = Request.Form["AlertMessage"].ToString();

                SqlHelper.InsertDeviceAlertInformation(deviceID, deviceName, deviceEName, levelCode, alertCode, alertMessage);
            }
        }

        JavaScriptSerializer json = new JavaScriptSerializer();
        str = json.Serialize(list);

        Response.Write(str);
        Response.End();
        return;
    }
}