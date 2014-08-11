<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="DeviceCurrentMonitor.aspx.cs" Inherits="Store_DeviceCurrentMonitor" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <title>设备当前监控信息</title>    
    <script src="/contents/js/bootstrap-select.js"></script>
    <script>
        $(function () {

            $('.selectpicker').selectpicker({
                style: 'btn-primary',
                size: 5
            });

            // 请求监控设备
            $.ajax({
                type: "get",
                url: "/Store/DeviceCurrentMonitorJSON.aspx?status=device",
                beforeSend: function (XMLHttpRequest) {

                },
                success: function (data, textStatus) {
                    var obj = eval(data);
                    var options = "";
                    for (var i = 0; i < obj.length; i++) {
                        options += "<option value='" + obj[i].DeviceID + "'>" + obj[i].DeviceName + "</option>";
                    }
                    $('#device').append(options);
                    $('#device').val("D01");
                },
                complete: function (XMLHttpRequest, textStatus) {
                    //HideLoading();
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    //请求出错处理 

                }
            });
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div class="row">
        <div class="col-md-1"></div>
        <div class="col-md-2">
            <select id="device" class="selectpicker">        
            </select>    
        </div>        
        <div class="col-md-1">
          <a href="#fakelink" onclick="getReport();" class="btn btn-primary" style="width: 100px;" id="query">查询</a>
          <img src="/contents/images/load_green.GIF" style="display: none;" id="loadimg" />
        </div>
  </div>

  <div class="row">
    <div class="col-md-1"></div>
    <div class="col-md-10">
      <table id="tbDevice"></table>      
    </div>
    <div class="col-md-1"></div>
  </div>

    <script>
        $('#mfacility').addClass('active');
        $('#mdevice').addClass('active');
               

        function captionCenter(gridName) {
            $(gridName).closest("div.ui-jqgrid-view")
                       .children("div.ui-jqgrid-titlebar")
                       .css("text-align", "center")
                       .children("span.ui-jqgrid-title")
                       .css("float", "none");
        };

        function captionText() {
            return $('#device').find("option:selected").text() + "当前监控信息";
        }

        function getReport() {
            var device = $('#device').val();           

            var url = "/Store/DeviceCurrentMonitorJSON.aspx?status=query&device=" + device + "";

            $('#tbDevice').jqGrid({
                url: url,
                datatype: "json",
                colNames: ["ID", "店号", "区域", "店铺类型", "设备ID", "设备名称", "IP", "时间", "Ping"],
                colModel: [
                    { name: "N", index: "N", width: 50, align: "center" },
                    {
                        name: "StoreNo", index: "StoreNo", width: 100, align: "center", formatter: function (cellvalue, options, rowObject) {
                            if (rowObject["DeviceNetwork"] == 'DOWN') {
                                return "<span class='btn btn-danger'></span> " + cellvalue;
                            } else {
                                return "<span class='btn btn-success'></span> " + cellvalue;
                            }
                        }
                    },
                    { name: "StoreRegion", index: "StoreRegion", width: 100, align: "center" },
                    { name: "StoreType", index: "StoreType", width: 100, align: "center" },
                    { name: "DeviceID", index: "DeviceID", width: 100, align: "center" },
                    { name: "DeviceName", index: "DeviceName", width: 120, align: "center" },
                    { name: "IP", index: "IP", width: 200, align: "center" },                    
                    { name: "RecordTime", index: "RecordTime", width: 150, align: "center" },
                    { name: "Ping", index: "Ping", width: 100, align: "center" },
                ],
                rowNum: 500,
                sortname: 'StoreNo',
                viewrecords: true,
                sortorder: "desc",
                caption: captionText(),
                height: 395,
                scrollrows: true,
                gridComplete: function () {
                    captionCenter('#tbDevice');
                    $('#loadimg').hide();
                    $('#query').show();
                },
                beforeRequest: function () {
                    $('#query').hide();
                    $('#loadimg').show();
                }
            }); 
            $('.jqg-first-row-header').next().children().css("text-align", "center");
            $("#tbDevice").jqGrid('setCaption', captionText());
            $("#tbDevice").jqGrid('setGridParam', { url: url }).trigger("reloadGrid");
        }
    </script>
</asp:Content>

