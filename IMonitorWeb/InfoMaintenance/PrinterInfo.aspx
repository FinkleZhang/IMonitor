<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="PrinterInfo.aspx.cs" Inherits="InfoMaintenance_PrinterInfo" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <title>打印机信息维护</title>
    <style type="text/css">
    .ui-jqgrid .ui-pg-input { height:20px;font-size:.8em; margin: 0;} 
  </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div class="row">  
        <div class="col-md-1"></div>      
        <div class="col-md-11">
          <div>                
            <table id="tbPinfo"></table>  
            <div id="pager"></div>                               
          </div>                      
        </div>        
    </div>

    <script type="text/javascript">
        $('#mstores').addClass('active');
        $('#pinfo').addClass('active');


        var lastsel;
        var alertbar = $('#alertbar');

        function captionCenter(gridName) {
            $(gridName).closest("div.ui-jqgrid-view")
                      .children("div.ui-jqgrid-titlebar")
                      .css("text-align", "center")
                      .children("span.ui-jqgrid-title")
                      .css("float", "none");
        };

        function printerType() {
            var sel_id = $('#tbPinfo').jqGrid('getGridParam', 'selrow');
            var value = $('#tbPinfo').jqGrid('getCell', sel_id, 'PrinterType');
            return value;
        };        

        $('#tbPinfo').jqGrid({
            url: '/InfoMaintenance/PrinterInfoJSON.aspx?status=all',
            datatype: "json",
            colNames: ["打印机型号", "墨盒型号"],
            colModel: [
                { name: "PrinterType", index: "PrinterType", width: 400, align: "center", editable: true },
                { name: "TonerType", index: "TonerType", width: 300, align: "center", editable: true },
            ],
            rowNum: 500,
            sortname: 'PrinterType',
            viewrecords: true,
            sortorder: "desc",
            caption: "打印机信息维护",
            height: 500,
            scrollrows: true,
            gridComplete: function () {
                captionCenter('#tbPinfo');
            },
            ondblClickRow: function (rowid, iRow, iCol, e) {
                $('#tbPinfo').editGridRow(rowid, true);
            },
            toppager: true,
            editurl: "/InfoMaintenance/PrinterInfoJSON.aspx",
        });

        $('#tbPinfo').jqGrid('navGrid', "#pager", { edit: true, add: true, del: true, search: false, refresh: false, cloneToTop: true },
                              {
                                  closeAfterEdit: true
                              }, // edit parameters
                              {

                              }, // add parameters
                              {
                                  delData: {
                                      name: printerType
                                  }
                              },
                              {},
                              {});
  </script>
</asp:Content>

