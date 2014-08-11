<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="DeviceOnlineReport.aspx.cs" Inherits="Store_DeviceOnlineReport" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <title>设备在线时间报表</title>
    <style>
        div.ui-datepicker{
         font-size:10px;
        }
    </style>
    <script src="/contents/js/bootstrap-select.js"></script>
    <script src="/contents/js/jquery-ui.min.js"></script>
    <script>
        $(function () {
            var option = "";
            for (var i = 0; i < 53; i++) {
                option += "<option value='" + i + "'>" + i + "</option>";
            }
            $('#week').append(option); // 填充周数

            option = "";
            for (var i = 0; i < 13; i++) {
                option += "<option value='" + i + "'>" + i + "</option>";
            }
            $('#month').append(option); // 填充月份

            // select初始化
            $('.selectpicker').selectpicker({
                style: 'btn-primary',
                size: 5
            });
            $('#week').next().hide();
            $('#month').next().hide();
            $('#year').val('0');


            // 请求监控设备
            $.ajax({
                type: "get",
                url: "/Store/DeviceOnlineReportJSON.aspx?status=device",
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
        <div class="col-md-2">
            <select id="condition" class="selectpicker">
                <option value="day">当天</option>
                <option value="week">周</option>
                <option value="month">月</option>
                <option value="year">年</option>
                <option value="time">时间段</option>
            </select>         
        </div>
    <div class="col-md-2">        
        <select id="week" class="selectpicker"></select>
        <select id="month" class="selectpicker"></select>
        <input type="text" class="form-control" style="width: 200px; display:none;" placeholder="当天" value="1" id="day" />
        <input type="text" class="form-control" style="width: 200px; display:none;" placeholder="年" id="year" />        
        <input type="text" class="form-control" style="width: 200px; display:none;" placeholder="起始日期" id="sdate" />        
    </div> 
    <div class="col-md-2">
        <input type="text" class="form-control" style="width: 200px; display:none;" placeholder="结束日期" id="edate" />
    </div>
    <div class="col-md-1">
      <a href="#fakelink" onclick="getReport();" class="btn btn-primary" style="width: 100px;" id="query">查询</a>
      <img src="/contents/images/load_green.GIF" style="display: none;" id="loadimg" />
    </div>
  </div>

  <div class="row">
    <div class="col-md-1"></div>
    <div class="col-md-10">
      <table id="tbReport"></table>
      <table id="tbiPad"></table>
    </div>
    <div class="col-md-1"></div>
  </div>

    <script>
        $('#mreport').addClass('active');
        $('#mdevicerepo').addClass('active');        
        
        

        $('#condition').change(function () {            
            $("select option:selected").each(function () {
                var v = $(this).val();
                switch (v) {
                    case "day":
                        $('#day').val("1");
                        $('#week').val('0').next().hide();
                        $('#month').val('0').next().hide();
                        $('#year').val('0').hide();
                        $('#sdate').val('').hide();
                        $('#edate').val('').hide();
                        break;
                    case "week":
                        $('#day').val("0");
                        $('#week').selectpicker('val', theWeek()).next().show();
                        $('#month').val('0').next().hide();
                        $('#year').val('0').hide();
                        $('#sdate').val('').hide();
                        $('#edate').val('').hide();                        
                        break;
                    case "month":
                        $('#day').val("0");
                        $('#week').val('0').next().hide();
                        $('#month').selectpicker('val', new Date().getMonth()+1).next().show();
                        $('#year').val('0').hide();
                        $('#sdate').val('').hide();
                        $('#edate').val('').hide();                        
                        break;
                    case "year":
                        $('#day').val("0");
                        $('#week').val('0').next().hide();
                        $('#month').val('0').next().hide();
                        $('#year').val(new Date().getFullYear()).show();
                        $('#sdate').val('').hide();
                        $('#edate').val('').hide();
                        break;
                    case "time":
                        $('#day').val("0");
                        $('#week').val('0').next().hide();
                        $('#month').val('0').next().hide();
                        $('#year').val('0').hide();
                        $('#sdate').datepicker({ dateFormat: 'yy-mm-dd' }).show();
                        $('#edate').datepicker({ dateFormat: 'yy-mm-dd' }).show();
                        break;
                }
            });            
        });

        function theWeek() { // weekStart：每周开始于周几：周日：0，周一：1，周二：2 ...，默认为周日  
            var weekStart = 1;
            if (isNaN(weekStart) || weekStart > 6)
                weekStart = 0;
            var now = new Date();
            var year = now.getFullYear();
            var firstDay = new Date(year, 0, 1);
            var firstWeekDays = 7 - firstDay.getDay() + weekStart;
            var dayOfYear = (((new Date(year, now.getMonth(), now.getDate())) - firstDay) / (24 * 3600 * 1000)) + 1;
            return Math.ceil((dayOfYear - firstWeekDays) / 7) + 1;
        };

        function captionCenter(gridName) {
            $(gridName).closest("div.ui-jqgrid-view")
                       .children("div.ui-jqgrid-titlebar")
                       .css("text-align", "center")
                       .children("span.ui-jqgrid-title")
                       .css("float", "none");
        };

        function captionText()
        {            
            return "Store Monitoring Report - " + $('#device').find("option:selected").text();
        }

        function formatDate(date) {
            var myyear = date.getFullYear();
            var mymonth = date.getMonth() + 1;
            var myweekday = date.getDate();

            if (mymonth < 10) {
                mymonth = "0" + mymonth;
            }
            if (myweekday < 10) {
                myweekday = "0" + myweekday;
            }
            return (myyear + "-" + mymonth + "-" + myweekday);
        }

        function getXDate(year, weeks, weekDay) {
            weekDay %= 7;
            var date = new Date(year, "0", "1");
            var time = date.getTime();
            weekDay == 0 ? time += weeks * 7 * 24 * 3600000 : time += (weeks - 1) * 7 * 24 * 3600000;//这里需要注意，现在这种模式是以周日为一周的结束，如果设定周日为一周的开始，去掉这个判断，选择后者。
            date.setTime(time);
            return getNextDate(date, weekDay);
        }

        function getNextDate(nowDate, weekDay) {
            var day = nowDate.getDay();
            var time = nowDate.getTime();
            var sub = weekDay - day;
            time += sub * 24 * 3600000;
            nowDate.setTime(time);
            return nowDate;
        }

        function addDate(date, days) {
            var d = new Date(date);
            d.setDate(d.getDate() + days);
            var m = d.getMonth() + 1;
            return d.getFullYear() + '-' + m + '-' + d.getDate();
        }

        function headerText() {
            var d = $('#day').val();
            var w = $('#week').val();
            var m = $('#month').val();
            var y = $('#year').val();
            var s = $('#sdate').val();
            var e = $('#edate').val();

            if (d == "1") {
                var now = new Date().toISOString().split('T')[0].replace(/-/g,'.');
                return now + " - " + now;
            }

            if (w != "0") {
                var now = new Date();
                var s = formatDate(getXDate(now.getFullYear(), parseInt(w), 1))
                var e = addDate(s, 6);
                return s.replace(/-/g, '.') + " - " + e.replace(/-/g, '.');
            }

            if (y != "0") {
                var s = y + ".1.1";
                var e = y + ".12.31";
                return s + " - " + e;
            }

            if (m != "0") {
                var now = new Date();
                var s = new Date(now.getFullYear(), parseInt(m) - 1, 2).toISOString().split('T')[0].replace(/-/g, '.');
                var e = new Date(now.getFullYear(), parseInt(m), 1).toISOString().split('T')[0].replace(/-/g, '.');
                return s + " - " + e;
            }

            if (s != "" && e != "") {
                s = s.split('T')[0].replace(/-/g, '.');
                e = e.split('T')[0].replace(/-/g, '.');               
                return s + " - " + e;
            }
        }


        function getReport() {
            var device = $('#device').val();
            var d = $('#day').val();
            var w = $('#week').val();
            var m = $('#month').val();
            var y = $('#year').val();            
            var s = $('#sdate').val();
            var e = $('#edate').val();

            var url = "/Store/DeviceOnlineReportJSON.aspx?status=query&device=" + device + "&day=" + d + "&week=" + w + "&month=" + m + "&year=" + y + "&sdate=" + s + "&edate=" + e;
            
            if (device == "D05") {
                $('#gbox_tbReport').hide()
                $('#gbox_tbiPad').show()
                $('#tbiPad').jqGrid({
                    url: url,
                    datatype: "json",
                    colNames: ["店号", "区域", "店铺类型", "设备ID", "设备名称", "最后在线时间"],
                    colModel: [
                        { name: "StoreNo", index: "StoreNo", width: 80, align: "center" },
                        { name: "StoreRegion", index: "StoreRegion", width: 80, align: "center" },
                        { name: "StoreType", index: "StoreType", width: 80, align: "center" },
                        { name: "DeviceID", index: "DeviceID", width: 80, align: "center" },
                        { name: "DeviceName", index: "DeviceName", width: 150, align: "center" },
                        { name: "RecordTime", index: "RecordTime", width: 200, align: "center" },
                    ],
                    rowNum: 500,
                    sortname: 'StoreNo',
                    viewrecords: true,
                    sortorder: "desc",
                    caption: "",
                    height: 395,
                    scrollrows: true,
                    gridComplete: function () {
                        captionCenter('#tbiPad');
                        $('#loadimg').hide();
                        $('#query').show();
                    },
                    beforeRequest: function () {
                        $('#query').hide();
                        $('#loadimg').show();
                    }
                });
                $("#tbiPad").jqGrid('destroyGroupHeader');
                $("#tbiPad").jqGrid('setGroupHeaders', {
                    useColSpanStyle: false,
                    groupHeaders: [
                      { startColumnName: 'StoreNo', numberOfColumns: 9, titleText: headerText },
                    ]
                });
                $('.jqg-first-row-header').next().children().css("text-align", "center");
                $("#tbiPad").jqGrid('setCaption', "iPad 最后在线时间表");
                $("#tbiPad").jqGrid('setGridParam', { url: url }).trigger("reloadGrid");
            }
            else {
                $('#gbox_tbReport').show()
                $('#gbox_tbiPad').hide()
                $('#tbReport').jqGrid({
                    url: url,
                    datatype: "json",
                    colNames: ["店号", "区域", "店铺类型", "设备ID", "设备名称", "在线时间(分)", "离线时间(分)", "在线比率", "Ping值"],
                    colModel: [
                        { name: "StoreNo", index: "StoreNo", width: 80, align: "center" },
                        { name: "StoreRegion", index: "StoreRegion", width: 80, align: "center" },
                        { name: "StoreType", index: "StoreType", width: 80, align: "center" },
                        { name: "DeviceID", index: "DeviceID", width: 80, align: "center" },
                        { name: "DeviceName", index: "DeviceName", width: 150, align: "center" },
                        { name: "Uptime", index: "Uptime", width: 80, align: "center" },
                        { name: "Downtime", index: "Downtime", width: 80, align: "center" },
                        { name: "Ratio", index: "Ratio", width: 80, align: "center" },
                        { name: "Ping", index: "Ping", width: 80, align: "center" },
                    ],
                    rowNum: 500,
                    sortname: 'StoreNo',
                    viewrecords: true,
                    sortorder: "desc",
                    caption: "",
                    height: 395,
                    scrollrows: true,
                    gridComplete: function () {
                        captionCenter('#tbReport');
                        $('#loadimg').hide();
                        $('#query').show();
                    },
                    beforeRequest: function () {
                        $('#query').hide();
                        $('#loadimg').show();
                    }
                });
                $("#tbReport").jqGrid('destroyGroupHeader');
                $("#tbReport").jqGrid('setGroupHeaders', {
                    useColSpanStyle: false,
                    groupHeaders: [
                      { startColumnName: 'StoreNo', numberOfColumns: 9, titleText: headerText },
                    ]
                });
                $('.jqg-first-row-header').next().children().css("text-align", "center");
                $("#tbReport").jqGrid('setCaption', captionText());
                $("#tbReport").jqGrid('setGridParam', { url: url }).trigger("reloadGrid");
            }            
        }
    </script>
</asp:Content>

