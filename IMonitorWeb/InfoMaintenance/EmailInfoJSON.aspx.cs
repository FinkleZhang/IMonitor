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

public partial class InfoMaintenance_EmailInfoJSON : System.Web.UI.Page
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
                    DataSet ds = SqlHelper.GetEmailInformation();
                    int count = ds.Tables[0].Rows.Count;
                    for (int i = 0; i < count; i++)
                    {
                        EmailInformation email = new EmailInformation();
                        email.Level = ds.Tables[0].Rows[i]["level"].ToString();
                        email.EmailAddress = ds.Tables[0].Rows[i]["emailAddress"].ToString();

                        list.Add(email);
                    }
                }
            }
        }
        else if (Request.HttpMethod == "POST")
        {
            string oper = Request.Form["oper"].ToString();

            if (oper == "edit")
            {
                string level = Request.Form["Level"].ToString();
                string emailAddress = Request.Form["EmailAddress"].ToString();
                SqlHelper.UpdateEmailInformation(level, emailAddress);
            }
            else if (oper == "del")
            {
                string level = Request.Form["name"].ToString();
                SqlHelper.DeleteEmailInformation(level);
            }
            else if (oper == "add")
            {
                string level = Request.Form["Level"].ToString();
                string emailAddress = Request.Form["EmailAddress"].ToString();
                SqlHelper.InsertEmailInformation(level, emailAddress);
            }
        }

        JavaScriptSerializer json = new JavaScriptSerializer();
        str = json.Serialize(list);

        Response.Write(str);
        Response.End();
        return;
    }
}