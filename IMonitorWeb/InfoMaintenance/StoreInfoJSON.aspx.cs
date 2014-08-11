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

public partial class InfoMaintenance_StoreInfoJSON : System.Web.UI.Page
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
                string search = url.IndexOf("_search") != -1 ? Request.QueryString["_search"].ToString() : "";

                if (query.ToUpper() == "ALL")
                {
                    DataSet ds = new DataSet();

                    if (search == "true")
                    {                        
                        string searchString = Request.QueryString["searchString"].ToString();                        

                        ds = SqlHelper.GetStoreInformation(searchString);
                        int count = ds.Tables[0].Rows.Count;
                        for (int i = 0; i < count; i++)
                        {
                            StoreInformation storeInfo = new StoreInformation();
                            storeInfo.StoreNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                            storeInfo.StoreRegion = ds.Tables[0].Rows[i]["storeRegion"].ToString();
                            storeInfo.StoreType = ds.Tables[0].Rows[i]["storeType"].ToString();
                            storeInfo.EmailAddress = ds.Tables[0].Rows[i]["emailAddress"].ToString();
                            storeInfo.PrinterType = ds.Tables[0].Rows[i]["printerType"].ToString();
                            storeInfo.TonerType = ds.Tables[0].Rows[i]["tonerType"].ToString();
                            storeInfo.LaptopCount = ds.Tables[0].Rows[i]["laptopCount"].ToString();
                            storeInfo.Disabled = ds.Tables[0].Rows[i]["disabled"].ToString() == "False" ? "启用" : "禁用";
                            storeInfo.STime = ds.Tables[0].Rows[i]["stime"].ToString();
                            storeInfo.ETime = ds.Tables[0].Rows[i]["etime"].ToString();
                            storeInfo.Cycle = ds.Tables[0].Rows[i]["cycle"].ToString();
                            storeInfo.DeviceID = ds.Tables[0].Rows[i]["deviceID"].ToString();
                            storeInfo.DeviceName = ds.Tables[0].Rows[i]["deviceName"].ToString();
                            storeInfo.IP = ds.Tables[0].Rows[i]["IP"].ToString();

                            list.Add(storeInfo);
                        }
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
                string storeNo = Request.Form["StoreNo"].ToString();
                string deviceID = Request.Form["DeviceID"].ToString();
                string ip = Request.Form["IP"].ToString();
                string disabled = Request.Form["Disabled"].ToString();
                string stime = Request.Form["STime"].ToString();
                string etime = Request.Form["ETime"].ToString();
                string cycle = Request.Form["Cycle"].ToString();  

                SqlHelper.UpdateStoreInformation(storeNo, deviceID, ip, disabled, stime, etime, cycle);
            }            
        }

        JavaScriptSerializer json = new JavaScriptSerializer();
        str = json.Serialize(list);

        Response.Write(str);
        Response.End();
        return;
    }
}