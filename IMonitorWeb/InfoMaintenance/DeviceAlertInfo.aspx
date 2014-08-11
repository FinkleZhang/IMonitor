<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="DeviceAlertInfo.aspx.cs" Inherits="InfoMaintenance_DeviceAlertInfo" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <title>设备报警规则维护</title>
    <style type="text/css">
        .ui-jqgrid .ui-pg-input { height:20px;font-size:.8em; margin: 0;} 
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div class="row">  
        <div class="col-md-1"></div>      
        <div class="col-md-11">
          <div>                
            <table id="tbDAinfo"></table>  
            <div id="pager"></div>                               
          </div>                      
        </div>        
    </div>

    <script type="text/javascript">
        $('#mstores').addClass('active');
        $('#dainfo').addClass('active');


        var lastsel;
        var alertbar = $('#alertbar');

        function captionCenter(gridName) {
            $(gridName).closest("div.ui-jqgrid-view")
                      .children("div.ui-jqgrid-titlebar")
                      .css("text-align", "center")
                      .children("span.ui-jqgrid-title")
                      .css("float", "none");
        };

        function deviceID() {
            var sel_id = $('#tbDAinfo').jqGrid('getGridParam', 'selrow');
            var id = $('#tbDAinfo').jqGrid('getCell', sel_id, 'ID');           
            return id;
        };

        function getDeviceInfo() {
            $.ajax({
                type: "get",
                url: "/InfoMaintenance/DeviceInfoJSON.aspx?status=all",
                beforeSend: function (XMLHttpRequest) {

                },
                success: function (data, textStatus) {                                
                    window.pt = eval(data);
                },
                complete: function (XMLHttpRequest, textStatus) {
                    //HideLoading();
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    //请求出错处理                     
                }
            });
        };

        function dtype() {
            var s = "";
            var d = window.pt;
            for (var i = 0; i < d.length; i++) {
                s += d[i].DeviceName + ":" + d[i].DeviceName + ";";
            }
            return s.substr(0, s.length - 1);
        }

        function pat() {
            var d = window.pt;
            var obj = {};
            for (var i = 0; i < d.length; i++) {
                obj[d[i].DeviceName] = d[i].DeviceID;
            }
            return obj;
        }
                
        function check(value, colname) {
            if (value == "") {                
                return [false, "设备ID不能为空"];
            }            
            else
                return [true, ""];
        }

        function levelCodeValue() {           
            return 'Alert2:Alert2;Alert30:Alert30;Alert0:Alert0;:'
        }

        $('#tbDAinfo').jqGrid({
            url: '/InfoMaintenance/DeviceAlertInfoJSON.aspx?status=all',
            datatype: "json",
            colNames: ["ID", "设备ID", "设备名称", "设备英文名", "级别码", "报警码", "报警信息"],
            colModel: [
                { name: "ID", index: "ID", width: 20, align: "center", editable: true, editoptions: { readonly: true } },
                { name: "DeviceID", index: "DeviceID", width: 80, align: "center", editable: true, editoptions: { readonly: true }, editrules: { custom: true, custom_func: check } },
                { name: "DeviceName", index: "DeviceName", width: 100, align: "center", editable: true,  edittype: "select", editoptions: { value: dtype, dataEvents: [{ type: "change", fn: function (e) { var obj = pat(); $("#DeviceID").val(obj[this.value]); } }] } },
                { name: "DeviceEName", index: "DeviceEName", width: 100, align: "center", editable: true },
                { name: "LevelCode", index: "LevelCode", width: 80, align: "center", editable: true, edittype: "select", editoptions: { value: levelCodeValue } },
                { name: "AlertCode", index: "AlertCode", width: 80, align: "center", editable: true, },
                { name: "AlertMessage", index: "AlertMessage", width: 750, align: "left", editable: true, edittype: "textarea", editoptions: { rows: "2", cols: "20" } },
            ],
            rowNum: 500,
            sortname: 'DeviceID',
            viewrecords: true,
            sortorder: "desc",
            caption: "设备报警规则维护",
            height: 500,
            scrollrows: true,
            gridComplete: function () {
                captionCenter('#tbDAinfo');
            },
            beforeRequest: function () {
                getDeviceInfo();
            },
            ondblClickRow: function (rowid, iRow, iCol, e) {
                $('#tbDAinfo').editGridRow(rowid, {
                    closeAfterEdit: true,                    
                });
            },
            toppager: true,
            editurl: "/InfoMaintenance/DeviceAlertInfoJSON.aspx",
        });

        $('#tbDAinfo').jqGrid('navGrid', "#pager", { edit: true, add: true, del: true, search: false, refresh: false, cloneToTop: true },
                              {
                                  closeAfterEdit: true,                                 
                              }, // edit parameters
                              {

                              }, // add parameters
                              {
                                  delData: {
                                      name: deviceID
                                  }
                              },
                              {},
                              {});
  </script>
</asp:Content>

