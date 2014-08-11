<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="StoreBaseInformation.aspx.cs" Inherits="InfoMaintenance_StoreBaseInformation" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <title>门店基础信息维护</title>
    <style type="text/css">
        .ui-jqgrid .ui-pg-input { height:20px;font-size:.8em; margin: 0;} 
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div class="row">    
        <div class="col-md-1"></div>       
        <div class="col-md-11">
          <div>                
            <table id="tbSBinfo"></table>  
            <div id="pager"></div>                               
          </div>                      
        </div>        
    </div>

    <script type="text/javascript">
        $('#mstores').addClass('active');
        $('#sbinfo').addClass('active');


        var lastsel;
        var alertbar = $('#alertbar');
        window.pt = "";

        function captionCenter(gridName) {
            $(gridName).closest("div.ui-jqgrid-view")
                      .children("div.ui-jqgrid-titlebar")
                      .css("text-align", "center")
                      .children("span.ui-jqgrid-title")
                      .css("float", "none");
        };

        function storeNo() {
            var sel_id = $('#tbSBinfo').jqGrid('getGridParam', 'selrow');
            var value = $('#tbSBinfo').jqGrid('getCell', sel_id, 'StoreNo');
            return value;
        };

        function sync() {
            $.ajax({
                type: "get",
                url: "/InfoMaintenance/StoreBaseInformationJSON.aspx?status=sync",
                beforeSend: function (XMLHttpRequest) {
                    alertbar.removeClass("alert-danger").addClass("alert-success");
                    alertbar.text("正在同步...");
                    alertbar.fadeIn(1000);
                },
                success: function (data, textStatus) {
                    alertbar.text(data);
                    alertbar.fadeOut(1500);
                    $('#tbSBinfo').trigger('reloadGrid');
                },
                complete: function (XMLHttpRequest, textStatus) {
                    //HideLoading();
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    //请求出错处理 
                    alertbar.removeClass("alert-success").addClass("alert-danger");
                    alertbar.text("无法同步");
                }
            });
        };

        function getPrinterBaseInfo() {            
            $.ajax({
                type: "get",
                url: "/InfoMaintenance/StoreBaseInformationJSON.aspx?status=p",
                beforeSend: function (XMLHttpRequest) {
                    
                },
                success: function (data, textStatus) {                    
                    window.pt = data;
                },
                complete: function (XMLHttpRequest, textStatus) {
                    //HideLoading();
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    //请求出错处理                     
                }
            });            
        };

        function ptype() {
            return window.pt.split("|")[0];
        }

        function ttype() {
            return window.pt.split("|")[1];
        }

        function pat() {
            var p = window.pt.split("|")[0].split(";");
            var t = window.pt.split("|")[1].split(";");
            var obj = {};
            for (var i = 0; i < p.length; i++)
            {
                obj[p[i].split(":")[0]] = t[i].split(":")[0];
            }
            return obj;
        }
        
        $('#tbSBinfo').jqGrid({
            url: '/InfoMaintenance/StoreBaseInformationJSON.aspx?status=all',
            datatype: "json",
            colNames: ["店号", "区域", "类型", "邮件地址", "打印机型号", "墨盒型号", "笔记本数量"],
            colModel: [
                { name: "StoreNo", index: "StoreNo", width: 50, align: "center", editable: true },
                { name: "StoreRegion", index: "StoreRegion", width: 50, align: "center", editable: true, editoptions: { dataEvents: [{ type: "change", fn: function (e) { var v = this.value == "HK" ? "Store@luxottica.com.hk" : "Store@luxottica.com.cn"; $("#EmailAddress").val($("#StoreNo").val() + v); } }] } },
                { name: "StoreType", index: "StoreType", width: 60, align: "center", editable: true, edittype: "select", editoptions: { value: "iFocus:iFocus;Focus:Focus;Accufit:Accufit" } },
                { name: "EmailAddress", index: "EmailAddress", width: 190, align: "center", search: false, editable: true, editoptions: { size: 40 } },
                { name: "PrinterType", index: "PrinterType", width: 250, align: "center", editable: true, edittype: "select", editoptions: { value: ptype, dataEvents: [{ type: "change", fn: function (e) { var obj = pat(); $("#TonerType").val(obj[this.value]); } }] } },
                { name: "TonerType", index: "TonerType", width: 80, align: "center", editable: true, edittype: "select", editoptions: { value: ttype } },
                { name: "LaptopCount", index: "LaptopCount", width: 89, align: "center", editable: true, edittype: "select", editoptions: { value: "1:1;2:2" } },                
            ],
            rowNum: 500,
            sortname: 'StoreNo',
            viewrecords: true,
            sortorder: "desc",
            caption: "门店基础信息维护",
            height: 500,
            scrollrows: true,
            gridComplete: function () {
                captionCenter('#tbSBinfo');                            
            },
            beforeRequest: function () {
                getPrinterBaseInfo();
            },
            ondblClickRow: function (rowid, iRow, iCol, e) {                
                $('#tbSBinfo').editGridRow(rowid, true);
            },
            toppager: true,
            editurl: "/InfoMaintenance/StoreBaseInformationJSON.aspx",
        });

        $('#tbSBinfo').jqGrid('navGrid', "#pager", { edit: true, add: true, del: true, search: true, refresh: false, cloneToTop: true },
                              {
                                  closeAfterEdit: true,
                                  height: 325,
                                  width: 360
                              }, // edit parameters
                              {
                                  closeAfterAdd: true,
                                  height: 325,
                                  width: 360
                              }, // add parameters
                              {
                                  delData: {
                                      name: storeNo
                                  }
                              },
                              {
                                  sopt: ['eq'],
                                  closeAfterSearch: true,
                                  afterShowSearch: function () {
                                      $("#jqg1").focus();
                                  }
                              },
                              {});

        $("#tbSBinfo").navButtonAdd('#tbSBinfo_toppager', {
            caption: "",
            title: "同步店铺信息",
            buttonicon: "ui-icon-transferthick-e-w",
            onClickButton: sync,
            position: "first"
        });
  </script>
</asp:Content>

