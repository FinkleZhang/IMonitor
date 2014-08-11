<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="StoreInfo.aspx.cs" Inherits="InfoMaintenance_StoreInfo" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <title>门店IP信息维护</title>
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
            <table id="tbSinfo"></table>  
            <div id="pager"></div>                               
            </div>                      
        </div>        
    </div>

    <script type="text/javascript">
        $('#mstores').addClass('active');
        $('#sinfo').addClass('active');


        var lastsel;
        var alertbar = $('#alertbar');       

        function captionCenter(gridName) {
            $(gridName).closest("div.ui-jqgrid-view")
                      .children("div.ui-jqgrid-titlebar")
                      .css("text-align", "center")
                      .children("span.ui-jqgrid-title")
                      .css("float", "none");
        };

        function numbercheck(value, colname) {
            var exp = /^\d{1,2}$/;

            if (value.match(exp) == null)
                return [false, "请输入一到两位数字"];
            else
                return [true, ""];
        }

        function ipcheck(value, colname) {
            var ip1 = value.split(";")[0];
            var ip2 = value.split(";")[1];
            var exp = /^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$/;

            if (ip1.match(exp) == null)
                return [false, "IP1不合法"];
            else if (ip2 != undefined && ip2.match(exp) == null)
                return [false, "IP2不合法"];
            else
                return [true, ""];
        }

        var editproperty = {
            closeAfterEdit: true,
            height: 380,
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
        };
        
        $('#tbSinfo').jqGrid({
            url: '/InfoMaintenance/StoreInfoJSON.aspx?status=all',
            datatype: "json",
            colNames: ["店号", "是否禁用", "设备ID", "设备名称", "IP", "监控开始时间", "监控结束时间", "监控周期(分)"],
            colModel: [
                { name: "StoreNo", index: "StoreNo", width: 100, align: "center", editable: true, editoptions: { readonly: true } },
                {
                    name: "Disabled", index: "Disabled", width: 100, align: "center", search: false, editable: true, edittype: "select", editoptions: { value: "0:启用;1:禁用" }, formatter: function (cellvalue, options, rowObject) {
                        if (cellvalue == '禁用') {
                            return "<span class='btn btn-danger'></span> " + cellvalue;
                        } else {
                            return "<span class='btn btn-success'></span> " + cellvalue;
                        }
                    }
                },
                { name: "DeviceID", index: "DeviceID", width: 100, align: "center", search: false, editable: true, editoptions: { readonly: true } },
                { name: "DeviceName", index: "DeviceName", width: 200, align: "center", search: false, editable: true, editoptions: { readonly: true } },
                { name: "IP", index: "IP", width: 300, align: "center", search: false, editable: true, editoptions: { size: 40 }, editrules: { custom: true, custom_func: ipcheck } },
                { name: "STime", index: "STime", width: 100, align: "center", search: false, editable: true, editoptions: { size: 30 } },
                { name: "ETime", index: "ETime", width: 100, align: "center", search: false, editable: true, editoptions: { size: 30 } },
                { name: "Cycle", index: "Cycle", width: 100, align: "center", search: false, editable: true, editoptions: { size: 30 }, editrules: { custom: true, custom_func: numbercheck } },
            ],
            rowNum: 500,
            sortname: 'StoreNo',
            viewrecords: true,
            sortorder: "desc",
            caption: "门店IP信息维护",
            height: 500,
            scrollrows: true,
            gridComplete: function () {
                captionCenter('#tbSinfo');
            },
            ondblClickRow: function (rowid, iRow, iCol, e) {
                $('#tbSinfo').editGridRow(rowid, editproperty);
            },
            toppager: true,
            editurl: "/InfoMaintenance/StoreInfoJSON.aspx",
        });

        $('#tbSinfo').jqGrid('navGrid', "#pager", { edit: true, add: false, del: false, search: true, refresh: false, cloneToTop: true },
                              {
                                  closeAfterEdit: true,                                  
                                  height: 380,
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
                                 
                              }, // add parameters
                              {
                                  
                              },
                              {
                                  sopt: ['eq'],
                                  closeAfterSearch: true,
                                  afterShowSearch: function () {
                                      $("#jqg1").focus();
                                  }
                              },
                              {});        
  </script>
</asp:Content>

