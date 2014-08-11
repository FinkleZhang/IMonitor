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


public partial class InfoMaintenance_PrinterInfoJSON : System.Web.UI.Page
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
                    DataSet ds = SqlHelper.GetPrinterBaseInformation();
                    int count = ds.Tables[0].Rows.Count;
                    for (int i = 0; i < count; i++)
                    {
                        PrinterBaseInformation printer = new PrinterBaseInformation();
                        printer.PrinterType = ds.Tables[0].Rows[i]["printerType"].ToString();
                        printer.TonerType = ds.Tables[0].Rows[i]["tonerType"].ToString();

                        list.Add(printer);
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
                string printerType = Request.Form["PrinterType"].ToString();
                string tonerType = Request.Form["TonerType"].ToString();
                SqlHelper.UpdatePrinterBaseInformation(printerType, tonerType);
            }
            else if (oper == "del")
            {
                string printerType = Request.Form["name"].ToString();
                SqlHelper.DeletePrinterBaseInformation(printerType);
            }
            else if (oper == "add")
            {
                string printerType = Request.Form["PrinterType"].ToString();
                string tonerType = Request.Form["TonerType"].ToString();
                SqlHelper.InsertPrinterBaseInformation(printerType, tonerType);
            }
        }

        JavaScriptSerializer json = new JavaScriptSerializer();
        str = json.Serialize(list);

        Response.Write(str);
        Response.End();
        return;
    }
}