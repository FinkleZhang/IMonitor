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

public partial class InfoMaintenance_StoreBaseInformationJSON : System.Web.UI.Page
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
                    DataSet ds;

                    if (search == "false")
                    {
                        ds = SqlHelper.GetStoreBaseInformation();                        
                    }
                    else
                    {
                        string searchField = Request.QueryString["searchField"].ToString();
                        string searchString = Request.QueryString["searchString"].ToString();

                        StoreBaseInformation sb = new StoreBaseInformation();
                        sb.StoreNo = searchField == "StoreNo" ? searchString : "";
                        sb.StoreRegion = searchField == "StoreRegion" ? searchString : "";
                        sb.StoreType = searchField == "StoreType" ? searchString : "";
                        sb.PrinterType = searchField == "PrinterType" ? searchString : "";
                        sb.TonerType = searchField == "TonerType" ? searchString : "";
                        sb.LaptopCount = searchField == "LaptopCount" ? searchString : "";

                        ds = SqlHelper.GetStoreBaseInformation(sb);                        
                    }
                    int count = ds.Tables[0].Rows.Count;
                    for (int i = 0; i < count; i++)
                    {
                        StoreBaseInformation storeBase = new StoreBaseInformation();
                        storeBase.StoreNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                        storeBase.StoreRegion = ds.Tables[0].Rows[i]["storeRegion"].ToString();
                        storeBase.StoreType = ds.Tables[0].Rows[i]["storeType"].ToString();
                        storeBase.EmailAddress = ds.Tables[0].Rows[i]["emailAddress"].ToString();
                        storeBase.PrinterType = ds.Tables[0].Rows[i]["printerType"].ToString();
                        storeBase.TonerType = ds.Tables[0].Rows[i]["tonerType"].ToString();
                        storeBase.LaptopCount = ds.Tables[0].Rows[i]["laptopCount"].ToString();                        

                        list.Add(storeBase);
                    }
                }                
                else if (query.ToUpper() == "SYNC")
                {
                    SqlHelper.SyncStoreBaseInformation();
                    Response.Write("同步成功！");
                    Response.End();
                    return;
                }
                else if (query.ToUpper() == "P")
                {
                    DataSet ds = SqlHelper.GetPrinterBaseInformation();
                    int count = ds.Tables[0].Rows.Count;
                    string ptype = "";
                    string ttype = "";
                    for (int i = 0; i < count; i++)
                    {
                        string p = ds.Tables[0].Rows[i]["printerType"].ToString();
                        string t = ds.Tables[0].Rows[i]["tonerType"].ToString();

                        ptype += p + ":" + p + ";";
                        ttype += t + ":" + t + ";";
                    }
                    Response.Write(ptype.TrimEnd(';') + "|" + ttype.TrimEnd(';'));
                    Response.End();
                    return;
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
                StoreBaseInformation storeBase = new StoreBaseInformation();
                storeBase.StoreNo = Request.Form["StoreNo"].ToString();
                storeBase.StoreRegion = Request.Form["StoreRegion"].ToString();
                storeBase.StoreType = Request.Form["StoreType"].ToString();
                storeBase.EmailAddress = Request.Form["EmailAddress"].ToString();
                storeBase.PrinterType = Request.Form["PrinterType"].ToString();
                storeBase.TonerType = Request.Form["TonerType"].ToString();
                storeBase.LaptopCount = Request.Form["LaptopCount"].ToString();                

                SqlHelper.UpdateStoreBaseInformation(storeBase);
            }
            else if (oper == "del")
            {
                string storeNo = Request.Form["name"].ToString();
                SqlHelper.DeleteStoreBaseInformation(storeNo);
            }
            else if (oper == "add")
            {
                StoreBaseInformation storeBase = new StoreBaseInformation();
                storeBase.StoreNo = Request.Form["StoreNo"].ToString();
                storeBase.StoreRegion = Request.Form["StoreRegion"].ToString();
                storeBase.StoreType = Request.Form["StoreType"].ToString();
                storeBase.EmailAddress = Request.Form["EmailAddress"].ToString();
                storeBase.PrinterType = Request.Form["PrinterType"].ToString();
                storeBase.TonerType = Request.Form["TonerType"].ToString();
                storeBase.LaptopCount = Request.Form["LaptopCount"].ToString();                

                SqlHelper.InsertStoreBaseInformation(storeBase);
            }            
        }

        JavaScriptSerializer json = new JavaScriptSerializer();
        str = json.Serialize(list);

        Response.Write(str);
        Response.End();
        return;
    }
}