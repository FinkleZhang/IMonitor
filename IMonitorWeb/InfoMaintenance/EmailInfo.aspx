<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="EmailInfo.aspx.cs" Inherits="InfoMaintenance_EmailInfo" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <title>邮件信息维护</title>
    <style type="text/css">
        .ui-jqgrid .ui-pg-input { height:20px;font-size:.8em; margin: 0;} 
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div class="row">  
        <div class="col-md-1"></div>      
        <div class="col-md-11">
          <div>                
            <table id="tbEinfo"></table>  
            <div id="pager"></div>                               
          </div>                      
        </div>        
    </div>

    <script type="text/javascript">
        $('#mstores').addClass('active');
        $('#einfo').addClass('active');


        var lastsel;
        var alertbar = $('#alertbar');

        function captionCenter(gridName) {
            $(gridName).closest("div.ui-jqgrid-view")
                      .children("div.ui-jqgrid-titlebar")
                      .css("text-align", "center")
                      .children("span.ui-jqgrid-title")
                      .css("float", "none");
        };

        function level() {
            var sel_id = $('#tbEinfo').jqGrid('getGridParam', 'selrow');
            var value = $('#tbEinfo').jqGrid('getCell', sel_id, 'Level');
            return value;
        };

        $('#tbEinfo').jqGrid({
            url: '/InfoMaintenance/EmailInfoJSON.aspx?status=all',
            datatype: "json",
            colNames: ["Level", "邮件地址"],
            colModel: [
                { name: "Level", index: "Level", width: 300, align: "center", editable: true },
                { name: "EmailAddress", index: "EmailAddress", width: 400, align: "center", editable: true },
            ],
            rowNum: 500,
            sortname: 'Level',
            viewrecords: true,
            sortorder: "desc",
            caption: "邮件信息维护",
            height: 500,
            scrollrows: true,
            gridComplete: function () {
                captionCenter('#tbEinfo');
            },
            ondblClickRow: function (rowid, iRow, iCol, e) {
                $('#tbEinfo').editGridRow(rowid, true);
            },
            toppager: true,
            editurl: "/InfoMaintenance/EmailInfoJSON.aspx",
        });

        $('#tbEinfo').jqGrid('navGrid', "#pager", { edit: true, add: true, del: true, search: false, refresh: false, cloneToTop: true },
                              {
                                  closeAfterEdit: true
                              }, // edit parameters
                              {

                              }, // add parameters
                              {
                                  delData: {
                                      name: level
                                  }
                              },
                              {},
                              {});
  </script>
</asp:Content>

