<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="DeviceInfo.aspx.cs" Inherits="InfoMaintenance_DeviceInfo" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <title>设备IP规则维护</title>
    <link href="/contents/css/jquery.datetimepicker.css" rel="stylesheet" />
    <style type="text/css">
        .ui-jqgrid .ui-pg-input { height:20px;font-size:.8em; margin: 0;} 
    </style>
    <script src="/contents/js/jquery.datetimepicker.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div class="row">  
        <div class="col-md-1"></div>      
        <div class="col-md-11">
          <div>                
            <table id="tbDinfo"></table>  
            <div id="pager"></div>                               
          </div>                      
        </div>        
    </div>

    <script type="text/javascript">
        $('#mstores').addClass('active');
        $('#dinfo').addClass('active');

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
            var sel_id = $('#tbDinfo').jqGrid('getGridParam', 'selrow');
            var value = $('#tbDinfo').jqGrid('getCell', sel_id, 'DeviceID');
            return value;
        };

        function ipcheck(value, colname) {
            var ip1 = value.split(";")[0];
            var ip2 = value.split(";")[1];
            var exp = /^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$/;
            
            if (value.indexOf("XX") == -1) 
                return [false, "请使用XX代替相应的IP位"];
            else if (ip1.replace("XX", "11").replace("XX", "11").match(exp) == null)
                return [false, "IP1不合法"];
            else if (ip2 != undefined && ip2.replace("XX", "11").replace("XX", "11").match(exp) == null)
                return [false, "IP2不合法"];
            else 
                return [true,""];
        }

        function numbercheck(value, colname) {            
            var exp = /^\d{1,2}$/;

            if (value.match(exp) == null)
                return [false, "请输入一到两位数字"];            
            else
                return [true, ""];
        }

        var editproperty = {
            closeAfterEdit: true,
            height: 320,
            width: 380,
            onInitializeForm: function (formid) {
                $('#STime').datetimepicker({
                    datepicker: false,
                    value: '09:00',
                    format: 'H:i',
                    step: 5
                });
                $('#ETime').datetimepicker({
                    datepicker: false,
                    value: '22:30',
                    format: 'H:i',
                    step: 5
                });
            }
        };

        $('#tbDinfo').jqGrid({
            url: '/InfoMaintenance/DeviceInfoJSON.aspx?status=all',
            datatype: "json",
            colNames: ["设备ID", "设备名称", "IP规则", "是否禁用", "监控开始时间", "监控结束时间", "监控周期(分)"],
            colModel: [
                { name: "DeviceID", index: "DeviceID", width: 100, align: "center", editable: true, editoptions: { readonly: true} },
                { name: "DeviceName", index: "DeviceName", width: 200, align: "center", editable: true },
                { name: "IPRule", index: "IPRule", width: 200, align: "center", editable: true, editrules: { custom: true, custom_func: ipcheck } },
                {
                    name: "Disabled", index: "Disabled", width: 150, align: "center", editable: true, edittype: "select", editoptions: { value: "0:启用;1:禁用" }, formatter: function (cellvalue, options, rowObject) {
                        if (cellvalue == '禁用') {
                            return "<span class='btn btn-danger'></span> " + cellvalue;
                        } else {
                            return "<span class='btn btn-success'></span> " + cellvalue;
                        }
                    }
                },
                { name: "STime", index: "STime", width: 150, align: "center", editable: true },
                { name: "ETime", index: "ETime", width: 150, align: "center", editable: true },
                { name: "Cycle", index: "Cycle", width: 100, align: "center", editable: true, editrules: { custom: true, custom_func: numbercheck } },
            ],
            rowNum: 500,
            sortname: 'DeviceID',
            viewrecords: true,
            sortorder: "desc",
            caption: "设备IP规则维护",
            height: 500,
            scrollrows: true,
            gridComplete: function () {
                captionCenter('#tbDinfo');
            },
            ondblClickRow: function (rowid, iRow, iCol, e) {
                $('#tbDinfo').editGridRow(rowid, editproperty);                
            },            
            toppager: true,
            editurl: "/InfoMaintenance/DeviceInfoJSON.aspx",
        });

        $('#tbDinfo').jqGrid('navGrid', "#pager", { edit: true, add: true, del: true, search: false, refresh: false, cloneToTop: true },
                              {
                                  closeAfterEdit: true,
                                  height: 320,
                                  width: 380,
                                  onInitializeForm: function (formid) {
                                      $('#STime').datetimepicker({
                                          datepicker: false,
                                          format: 'H:i',
                                          step: 5
                                      });
                                      $('#ETime').datetimepicker({
                                          datepicker: false,
                                          format: 'H:i',
                                          step: 5
                                      });
                                  }
                              }, // edit parameters
                              {
                                  onInitializeForm: function (formid) {
                                      $('#STime').datetimepicker({
                                          datepicker: false,
                                          format: 'H:i',
                                          step: 5
                                      });
                                      $('#ETime').datetimepicker({
                                          datepicker: false,
                                          format: 'H:i',
                                          step: 5
                                      });
                                  }
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

