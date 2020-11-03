var schedulerContentTopScroll = 0;
var selectedEvents = [];

function initScheduler(isAdmin, currentDate, isMobileDevice, height) {

    try {
        var editableSettings = {
            confirmation: isMobileDevice ? "Delete schedule" : "Are you sure you want to remove this schedule?",
            template: isMobileDevice ? $("#customEditorMobileTemplate").html() : $("#customEditorTemplate").html(),
            window: {
                title: "Schedule",
            }
        }

        currentDate = new Date(new Date(currentDate).setHours(0, 0, 0, 0));

        var schedular = $("#scheduler").kendoScheduler({
            date: currentDate,
            startTime: currentDate,
            mobile: isMobileDevice ? "phone" : false,
            height: height,
            toolbar: ["pdf", "excel"/*,"iCal"*/],
            pdf: {
                fileName: "Physician Schedule.pdf",
                proxyURL: "https://demos.telerik.com/kendo-ui/service/export"
            },


            views: [
                "day",
                { type: "workWeek", selected: false },
                "week",
                { type: "month", selected: true },
            ],
            editable: isAdmin == "True" ? editableSettings : false,

            dataSource: {
                serverFiltering: true,
                transport: {
                    read: {
                        url: '/Schedule/GetAll',
                        contentType: "application/json",
                        type: "POST",
                        data: function () {
                            
                            var scheduler = $("#scheduler").data("kendoScheduler");
                            var filterModel = { startDate: null, endDate: null, physicians: null, SchType : null};                           
                            filterModel.startDate = formateDate(scheduler.view().startDate());
                            filterModel.endDate = formateDate(scheduler.view().endDate());
                            filterModel.physicians = checkMultiSelctVal($("#comboBox").data("kendoMultiSelect").value());
                            filterModel.SchType = $('#rdValueId').val();
                            return filterModel;
                        },
                        complete: function (e) {
                            //HideLoading();
                        }
                    },
                    update: {
                        url: '/Schedule/Update',
                        type: "POST",
                        data: function () {
                            var filterModel = {SchType: null };
                            filterModel.SchType = $('#rdValueId').val();
                            return filterModel;
                        },
                        complete: function (e) {
                            if (e != null) {
                                if (e.status == '500')
                                    $(".k-edit-form-container, .km-scroll-container").find("#validationSummary").empty().showBSDangerAlert("", "Error! Please try again.");
                                else if (e.status == '400')
                                    $(".k-edit-form-container, .km-scroll-container").find("#validationSummary").empty().showBSDangerAlert("", "Error! The start and end time is overlapped with another existing schedule of this owner.");
                                else {
                                    schedulerContentTopScroll = $('.k-scheduler-content').scrollTop();
                                    //console.log("Scroll update: " + schedulerContentTopScroll);
                                    refreshScheduler();                                   
                                }
                            }
                            HideLoading();
                        }
                    },
                    create: {
                        url: '/Schedule/Create',
                        type: "POST",
                        data: function () {
                            var filterModel = { SchType: null };
                            filterModel.SchType = $('#rdValueId').val();
                            return filterModel;
                        },
                        complete: function (e) {                            
                            if (e != null) {
                                if (e.status == '500')
                                    $(".k-edit-form-container, .km-scroll-container").find("#validationSummary").empty().showBSDangerAlert("", "Error! Please try again.");
                                else if (e.status == '400')
                                    $(".k-edit-form-container, .km-scroll-container").find("#validationSummary").empty().showBSDangerAlert("", "Error! The start and end time is overlapped with an existing schedule of this owner.");
                                else {
                                    schedulerContentTopScroll = $('.k-scheduler-content').scrollTop();
                                    //console.log("Scroll create: " + schedulerContentTopScroll);
                                    refreshScheduler();                                                                       
                                }
                            }
                            HideLoading();
                        }
                    },
                    destroy: {
                        url: '/Schedule/Destroy',
                        type: "POST"
                    },
                    parameterMap: function (data, operation) {
                        if (operation !== "read") {
                            data.Start = formateDate(data.Start);
                            data.End = formateDate(data.End);
                            data.ScheduleDate = formateDate(data.ScheduleDate);
                            return data
                        }
                        else {
                            return kendo.stringify(data);
                        }
                    }
                },
                error: function (e) {
                    e.preventDefault = true;
                    if (e.status == '500') {
                        $(".k-edit-form-container, .km-scroll-container").find("#validationSummary").empty().showBSDangerAlert("", "Error! Please try again.");
                    }
                    HideLoading();
                },
                schema: {
                    model: {
                        id: "Id",
                        fields: {
                            title: { from: "TitleBig" },
                            start: {
                                type: "date", from: "Start", parse: function (e) { return new Date(e); }, validation: { required: true }
                            },
                            end: {
                                type: "date", from: "End", parse: function (e) { return new Date(e); }, validation: { required: true }
                            },
                            scheduleDate: {
                                type: "date", from: "ScheduleDate", parse: function (e) { return new Date(e); }, validation: { required: true }
                            },
                            description: { from: "Description" },
                            fullName: { from: "FullName" },
                            ownerId: { from: "UserId", validation: { required: true } },
                            rate: { from: "Rate" },
                            ShiftId: { from: "ShiftId" }
                        }
                    }
                }
            },
            edit: function (e) {
                try {
                    $(".k-edit-form-container").find("#validationSummary").empty();
                    $('.k-window-title').addClass("bold");
                    $(".k-scheduler-update").addClass("k-primary");
                    $("#schedulerValidator .k-widget").attr("style", "width: 100%;");
                    e.container.find("[for=recurrenceRule]").parent().hide();
                    e.container.find("[data-container-for=recurrenceRule]").hide();
                    e.container.find("[for=isAllDay]").parent().hide();
                    e.container.find("[data-container-for=isAllDay]").hide();
                    e.container.find("#ownerId").kendoDropDownList({
                        dataTextField: "Item2",
                        dataValueField: "Item1",
                        template: '<span class="k-scheduler-mark" style="background-color: #: color # !important"></span>#: text # ',
                        dataSource: getPhysicians(false),
                        optionLabel: "Select",
                    });
                    var ownerDropDown = e.container.find("#ownerId");
                    var scheduleDateControl = e.container.find("#scheduleDate");
                    if (isMobileDevice) {
                        e.container.find("#description").attr("style", "margin: 3px 10px 3px 3px;width: 92%;border: 1px solid rgb(221, 221, 221); border-radius: 3px 0 0 3px; padding: 0px;");
                        e.container.find("input").css("padding", "5px");
                        $(".k-edit-field").css("border", "0px");
                        $("#schedulerValidator .k-widget").attr("style", "margin-right:10px;border: 1px solid rgb(221, 221, 221);border-radius: 3px 0 0 3px;padding: 0px;width: 70%;");
                        $("#schedulerValidator .k-state-active ").css("box-shadow", "none !important");
                    }
                    if (e.event.Id > 0) {
                        ownerDropDown.data("kendoDropDownList").readonly();
                        $("[data-container-for=ownerId] span").css("color", "#ccc");

                        scheduleDateControl.data("kendoDatePicker").readonly();
                        scheduleDateControl.css("color", "#ccc");
                        $(".k-i-calendar").css("color", "#ccc");
                        //var id = e.event.Id;
                        //GetCustomRate(id);
                    }
                    var editorValidator = $('#schedulerValidator').kendoValidator({
                        rules: {
                            requiredFieldRules: function (input) {
                                if (input.is('[id=ownerId]')) {
                                    if ($(input).data("kendoDropDownList").value() == '' || $(input).data("kendoDropDownList").value() == null)
                                        return false;
                                }
                                else if (input.is('[id=scheduleDate]') || input.is('[name=End]')) {
                                    if ($(input).val() == '' || $(input).val() == null)
                                        return false;
                                }
                                else if (input.is('[name=Start]')) {
                                    if ($(input).val() == '')
                                        return false;
                                }
                                else if (input.is('[name=End]')) {
                                    if ($(input).val() == '')
                                        return false;
                                }
                                return true;
                            },
                            startEndNOtEqual: function (input) {
                                if (input.is('[name=End]') && $(input).val() != '' && $("[name=Start]").val() != "") {
                                    if ($("[name=Start]").val() == $("[name=End]").val())
                                        return false;
                                }
                                return true;
                            }
                        },
                        messages: {
                            requiredFieldRules: 'This is required field.',
                            startEndNOtEqual: 'End time should be greater than Start.'
                        }
                    }).data('kendoValidator');
                    // Adjust dropdown horizental allignment in case of five-9 sidebae expanded.
                    $('[data-role="dropdownlist"]').each(function () {
                        var kendoDropDown = $(this).data("kendoDropDownList");
                        if (kendoDropDown)
                            kendoDropDown.bind("open", onDropdDownOpen);
                    });
                    // Adjust date picker horizental allignment in case of five-9 sidebae expanded.
                    $('[data-role="datepicker"]').each(function () {
                        var datePicker = $(this).data("kendoDatePicker");
                        if (datePicker)
                            datePicker.bind("open", onDatePickerOpen);
                    });
                    // Adjust date picker horizental allignment in case of five-9 sidebae expanded.
                    $('[data-role="timepicker"]').each(function () {
                        var timePicker = $(this).data("kendoTimePicker");
                        if (timePicker)
                            timePicker.bind("open", onTimePickerOpen);
                    });
                }
                catch (e) {
                    console.log(e);
                    HideLoading();
                }
            },
            save: function (e) {
                var editorValidator = $('#schedulerValidator').data('kendoValidator');
                if (!editorValidator.validate()) {
                    e.preventDefault();
                }
            },
            dataBound: function (e) {
                debugger;
               
                $(".km-pane-wrapper").css("position", "relative");
                var events = $('.k-event-template');               
                var isDayView = $.trim($(".k-state-selected").data("name")).toLowerCase() == "day";
                var isMonthView = $.trim($(".k-state-selected").data("name")).toLowerCase() == "month";
                var isWorkWeekView = $.trim($(".k-state-selected").data("name")).toLowerCase() == "workweek";
                var isWeekView = $.trim($(".k-state-selected").data("name")).toLowerCase() == "week";
                selectedEvents = e.sender._data;
                var total = 0;
                for (var i = 0; i < events.length; i += 1) {
                    var currentEvent = $(events[i]);
                    var initialText = currentEvent.text();
                    var modifiedHtml = initialText.replaceAll("#div#", "<div>").replaceAll("#/div#", "</div>");
                    if (!isDayView) {
                        modifiedHtml = modifiedHtml.replaceAll("<div>", "<div style='display:none'>");
                    }                   
                    currentEvent.html(modifiedHtml);
                }
                //added by Bilal
                if (e.sender._data.length > 0 && $('#rdSuperAdminValueId').val() == "SuperAdmin" && $('#rdValueId').val() == "Physician" && !isDayView) {
                    for (var i = 0; i < e.sender._data.length; i += 1) {

                        var schedualMonth = e.sender._data[i].isFlagMonth;
                        var schedualDay = e.sender._data[i].isFlagDay;
                        debugger;
                        if (e.sender._data[i].isFlag != null && e.sender._data[i].isFlag == true && isMonthView) {
                            var currentMonth = parseInt(monthNameToNum($('.k-lg-date-format')[0].innerText.split(',')[0]));
                            if (currentMonth != schedualMonth) {
                                var otherMonthEvents = $('.k-other-month .k-nav-day');
                                for (var om = 0; om < otherMonthEvents.length; om += 1) {
                                    var CalDay = parseInt($(otherMonthEvents)[om].innerText);
                                    if (schedualDay == CalDay) {
                                        var curEvent = $(otherMonthEvents[om]);
                                        var curText = curEvent.text();
                                        var changeHtml = curText.replaceAll(curText, "<span class='k-more-events k-button' style='position: relative; background: border-box; border: none; float:left; width:5px;margin-top:-2px'><i class='fa fa-flag' style='color:red'></i></span> " + curText);
                                        curEvent.html(changeHtml);
                                        break;
                                    }
                                }
                            }
                            else {
                                var otherMonthDays = parseInt($('.k-scheduler-table tr:eq(1) .k-other-month').length) - 1;
                                var calMonthEvents = $('.k-nav-day');
                                var curEvent = $(calMonthEvents[schedualDay + otherMonthDays]);
                                var curText = curEvent.text();
                                var changeHtml = curText.replaceAll(curText, "<span class='k-more-events k-button' style='position: relative; top: -2px; background: border-box; border: none; float:left; width:5px'><i class='fa fa-flag' style='color:red'></i></span> " + curText);
                                curEvent.html(changeHtml);
                            }
                        }
                        if (e.sender._data[i].isFlag != null && e.sender._data[i].isFlag == true && (isWorkWeekView || isWeekView)) {
                            var weekEvents = $('.k-nav-day');
                            for (var c = 0; c < weekEvents.length; c += 1) {
                                var CalMonth = parseInt($(weekEvents)[c].innerText.split(' ')[1].split('/')[0]);
                                var CalDay = parseInt($(weekEvents)[c].innerText.split(' ')[1].split('/')[1]);
                                if ((schedualMonth == CalMonth) && (schedualDay == CalDay)) {
                                    var curEvent = $(weekEvents[c]);
                                    var curText = curEvent.text();
                                    var changeHtml = curText.replaceAll(curText, "<span class='k-more-events k-button' style='position: relative; background: border-box; border: none; float:left; width:5px'><i class='fa fa-flag' style='color:red'></i></span> " + curText);
                                    curEvent.html(changeHtml);
                                    break;
                                }
                            }
                        }
                    }
                }
               
                scheduler_view_range(e);
            },
            resources: [
                {
                    field: "ownerId",
                    title: "Owner",
                    dataSource: getPhysicians(false)
                }
            ]
        }).getKendoScheduler();
    }
    catch (e) {
        console.log(e.messages);
        HideLoading();
    }
}


function scheduler_view_range(e) {
    var view = e.sender.view();
    var startDatePST = view.startDate();
    var endDatePST = view.endDate();

    var sch = $("#scheduler").data("kendoScheduler");

    var startDate = formateDate(startDatePST);
    var endDate = formateDate(endDatePST);
    var physicians = checkMultiSelctVal($("#comboBox").data("kendoMultiSelect").value());

    $(sch.toolbar).children().remove("#ScheduleExportToExcel");
    $(sch.toolbar).children().remove("#ScheduleExportToiCal");
    $(sch.toolbar).children().remove("#SchedulePublish");
   
    var isGetAll = false;
    if (e.sender._data.length > 0 && $('#rdValueId').val() == "Physician" && $('#rdSuperAdminValueId').val() == "SuperAdmin" && $.trim($(".k-state-selected").data("name")).toLowerCase() == "month") {
        if (e.sender._data.find(x => x.scheduleGetAll == false) == undefined) {
            isGetAll = true;
        } else {
            var PublishSchedual = $("<a role='button' id='SchedulePublish' disabled  class='k-button btn-un-published'><span class='fa fa-upload'></span>  Publish Schedule</a>");
            $(sch.toolbar).prepend(PublishSchedual);
        }
    }

    if ($('#rdValueId').val() == "Physician" && $('#rdSuperAdminValueId').val() == "SuperAdmin" && $.trim($(".k-state-selected").data("name")).toLowerCase() == "month" && isGetAll) {
        $(sch.toolbar).children().remove("#SchedulePublish");
        if (e.sender._data.length > 0 && !e.sender._data[0].PublishStatus) {
            var PublishSchedual = $("<a role='button' id='SchedulePublish' onclick='PublishSchedule();' class='k-button btn-un-published'><span class='fa fa-upload'></span>  Publish Schedule</a>");
            $(sch.toolbar).prepend(PublishSchedual);
        }
        else {
            var PublishSchedual = $("<a role='button' id='SchedulePublish' disabled  class='k-button btn-published'><span class='fa fa-check-square-o'></span> Published</a>");
            $(sch.toolbar).prepend(PublishSchedual);
        }
    } 
   
  
    var ExportToExcel = $("<a role='button' id='ScheduleExportToExcel' href='/Schedule/ExportSchedule?startDate=" + startDate + " &endDate=" + endDate + "&Physicians=" + physicians + "' class='k-button k-excel'><span class='k-icon k-i-file-excel'></span>Export to Excel</a>");
    $(sch.toolbar).prepend(ExportToExcel);


    ScheduleCheckLoad();
    var ExportToiCal = $("<button id='ScheduleExportToiCal' onclick='ExportToiCal();' class='k-button k-save'><span class='k-icon k-i-calendar'></span>Export to iCal</button>");
    $(sch.toolbar).prepend(ExportToiCal);

}
//$("#ScheduleExportToExcel").on('click', function () {
//    var physicians = checkMultiSelctVal($("#comboBox").data("kendoMultiSelect").value());

//});

function updateHref() {
    debugger;
    var currentAttribute = $("#ScheduleExportToExcel").attr('href');
    //var found = currentAttribute.find('Physicians=');
    var found = currentAttribute.split('Physicians=');
    var physicians = checkMultiSelctVal($("#comboBox").data("kendoMultiSelect").value());
    found[1] = 'Physicians=' + physicians;
    $("#ScheduleExportToExcel").attr('href', found);
}

function getPhysicians(onlyScheduledPhy) {
    return new kendo.data.DataSource({
        transport: {
            read: {
                url: "/Schedule/GetScheduledPhysicians?onlySchedulePhys=" + onlyScheduledPhy,
                contentType: "application/json",
                type: "POST"
            }
        },
        schema: {
            model: {
                id: "Item1",
                fields: {
                    value: { from: "Item1" },
                    text: { from: "Item2" },
                    color: { from: "Item3" },
                    fullName: { from: "Item4" },
                    NPINumber: { from: "Item5" },
                    gender: { from: "Item6" },
                }
            }
        }
    });
}

function formateDate(date) {
    if (typeof date == "string")
        date = new Date(date);

    var day = (date.getDate() <= 9 ? "0" + date.getDate() : date.getDate());
    var month = (date.getMonth() + 1 <= 9 ? "0" + (date.getMonth() + 1) : (date.getMonth() + 1));
    var dateString = month + "/" + day + "/" + date.getFullYear() + " " + ("0" + date.getHours()).slice(-2) + ":" + ("0" + date.getMinutes()).slice(-2) + ":" + ("0" + date.getSeconds()).slice(-2);
    return dateString;
}

function checkMultiSelctVal(value) {
    debugger;
    if (value.length == 0) {
        return null;
    }
    if (value.length == 1) {
        if (value[0].trim() == "")
            return null;
    }
    return value;
}

function refreshScheduler() {
    var scheduler = $('#scheduler').data('kendoScheduler');
    scheduler.view(scheduler.view().name);
    $('.k-scheduler-content').scrollTop(schedulerContentTopScroll);
}

function GetCustomRate(id) {
    var phy_key = $('#ownerId').data("kendoDropDownList").value();
    var schedule_date = $('#scheduleDate').val();
    // var id = $('.get_by_cls').html();
    if (phy_key && schedule_date) {
        $.ajax({
            cache: false,
            async: true,
            type: "POST",
            url: "/rate/getCustomRate",
            data: { uss_key: id },
            success: function (e) {
                $('.rate').val(e.phr_rate);
                $('#ShiftId').val(e.phr_shift_key);
                console.log(e.phr_rate);
            },
            error: function (data) {
            }
        });
    }

}

function SplitString(s) {
    var arr = s.toString().split('(');
    var getindex = arr[1];
    var _index = getindex.split(')');
    console.log(arr);
    return parseFloat(_index[0]);
}

function getfacilities(start_time, end_time) {
    $.ajax({
        cache: false,
        async: true,
        type: "POST",
        url: "/Schedule/GetFewerPhyfacilities",
        data: { startDate: start_time, endDate: end_time },
        success: function (e) {
            $(".modal-body").animate({ scrollTop: 0 }, "slow");
            var _list = "<ul style='font-size:14px; list-style: none; padding-left:15px;'>";
            if (e.length > 0) {
                for (var i = 0; i < e.length; i++) {
                    _list = _list + "<li><i class='fa fa-flag'></i><span style='padding-left:10px;'>" + e[i].facility_name + "</span> - <span class=''><strong> (" + e[i].physcian_count + ") </strong> physician(s) credentialed.</span></li>";
                }
            } else {
                _list = _list + "<li>More than 2 physcians are covering in all facilities.</li>";
            }

            _list = _list + "</ul>";
            $('#fewPhylblId').text('Facility List:');
            $('#facCountlblId').text('Facility Count - ' + e.length);
            $('#fewerphyHeadId').text('Facilities with < 3 physicians credentialed');
            $("#fewerphyIdList").html(_list);
            $('#fewerphyId').modal("show");
        },
        error: function (data) {
        }
    });
}

function showTooltip() {
    $(".fewer-facility-list").css("cursor", "pointer");
}

function monthNameToNum(monthname) {
    var months = [
        'January', 'February', 'March', 'April', 'May',
        'June', 'July', 'August', 'September',
        'October', 'November', 'December'
    ];

    var month = months.indexOf(monthname);
    //return month ? month + 1 : 0;
    return month + 1;
}

function PublishSchedule() {
    var scheduleMonthDetails = $('.k-lg-date-format').text();
    var Month = parseInt(monthNameToNum(scheduleMonthDetails.split(',')[0].trim()));
    var Year = parseInt(scheduleMonthDetails.split(',')[1].trim());
    $.ajax({
        cache: false,
        async: true,
        type: "POST",
        url: "/Schedule/PublishSchedule",
        data: { month: Month, year: Year },
        success: function (e) {
            if (e.Status) {
                ScheduleCheckLoad();
                $('#bAlertId').removeClass('alert-warning');
                $('#bAlertId').removeClass('alert-success');
                $('#bAlertId').addClass('alert-success');
                $('#alertMessage').html('Schedule of <b>' + scheduleMonthDetails + '</b> published.');
                $('#bAlertId').show();
                $('#SchedulePublish').attr("disabled", true);
                $('#SchedulePublish').removeAttr("onclick");            
                $('#SchedulePublish').removeClass('btn-un-published');
                $('#SchedulePublish').addClass('btn-published');
                $('#SchedulePublish').html('<span class="fa fa-check-square-o"></span> Published');
                refreshScheduler();
            } else {
                $('#bAlertId').removeClass('alert-warning');
                $('#bAlertId').removeClass('alert-success');
                $('#bAlertId').addClass('alert-warning');
                $('#alertMessage').text('Something bad happened during publishing month schedule.');
                $('#bAlertId').show();
                $('#SchedulePublish').attr("disabled", false)
            }
            setTimeout(function () {
                $("#bAlertId").delay(5000).slideUp(200, function () {
                    $(this).hide();
                });
            }, 5000); 
        },
        error: function (data) {
        }
    });
}

function CloseAlert() {
    $('#bAlertId').hide();
}

function getRandomString(length) {
    var randomChars = 'abcdefghijklmnopqrstuvwxyz0123456789';
    var result = '';
    for (var i = 0; i < length; i++) {
        result += randomChars.charAt(Math.floor(Math.random() * randomChars.length));
    }
    return result;
}

function ExportToiCal() {
    var scheduler = $("#scheduler").data("kendoScheduler");
    var component = new ICAL.Component(['VCALENDAR', [], []]);
    component.updatePropertyWithValue('PRODID', '-//Google Inc//Google Calendar 70.9054//EN');
    component.updatePropertyWithValue('VERSION', '2.0');

    var schedulerEvents = scheduler.dataSource.view().toJSON();

    for (var i = 0; i < schedulerEvents.length; i++) {
        var schedulerEvent = schedulerEvents[i];
        var vevent = new ICAL.Component('VEVENT');
        var event = new ICAL.Event(vevent);

        event.uid = schedulerEvent.recurrenceId ? schedulerEvent.recurrenceId : getRandomString(26);
        if (schedulerEvent.Title.length == 3) {
            event.summary = schedulerEvent.title.substring(0, 20);
        } else {
            event.summary = schedulerEvent.title.substring(0, 19);
        }
        
        event.description = schedulerEvent.fullName;
        event.startDate  = ICAL.Time.fromDateTimeString(getISOString(schedulerEvent.start, true));
        event.endDate = ICAL.Time.fromDateTimeString(getISOString(schedulerEvent.end, true));

        if (schedulerEvent.recurrenceRule) {
            event.component.addProperty(
                new ICAL.Property(ICAL.parse.property("RRULE:" + schedulerEvent.recurrenceRule)));
        }

        if (schedulerEvent.recurrenceException) {
            event.component.addProperty(
                new ICAL.Property(ICAL.parse.property("EXDATE:" + schedulerEvent.recurrenceException)));
        }

        if (schedulerEvent.Id) {
            event.recurrenceId = ICAL.Time.fromDateTimeString(getISOString(schedulerEvent.start, true));
        }
   
        event.component.addPropertyWithValue("DTSTAMP",
            ICAL.Time.fromDateTimeString(getISOString(new Date(), true)));

        component.addSubcomponent(vevent);
    }

    //console.log(component.toString());

    kendo.saveAs({
        dataURI: "data:text/calendar," + component.toString(),
        fileName: "SchedulerCal_" + getISOString(new Date(), true)  + ".ics"
    });
}

function getISOString(date, toUTC) {
    date = toUTC ? kendo.timezone.convert(date, date.getTimezoneOffset(), "Etc/UTC") : date;
    return kendo.toString(date, "yyyy-MM-ddTHH:mm:ssZ");
}

function getTimeZone(timezone) {
    return timezone.toLowerCase() === "z" ? null : timezone;
}

$(document).ready(function () {
    ScheduleCheckLoad();
    $('#NavigateYes').click(function () {
        $('#AllowToNavigateId').modal('hide');
        loadPageAsync($('#navigateUrl').val(), $('#navigateTarget').val())
    });
})