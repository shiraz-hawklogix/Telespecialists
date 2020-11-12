var schedulerContentTopScroll = 0;

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
        $("#scheduler").kendoScheduler({
            date: currentDate,
            startTime: currentDate,
            mobile: isMobileDevice ? "phone" : false,
            height: height,
            toolbar: ["pdf"],
            pdf: {
                fileName: "Physician Schedule for nh alert.pdf",
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
                        url: '/Schedule/GetAllNH',
                        contentType: "application/json",
                        type: "POST",
                        data: function () {
                            var scheduler = $("#scheduler").data("kendoScheduler");
                            var filterModel = { startDate: null, endDate: null, physicians: null };
                            filterModel.startDate = formateDate(scheduler.view().startDate());
                            filterModel.endDate = formateDate(scheduler.view().endDate());
                            filterModel.physicians = checkMultiSelctVal($("#comboBox").data("kendoMultiSelect").value());
                            return filterModel;
                        },
                        complete: function (e) {
                            HideLoading();
                        }
                    },
                    update: {
                        url: '/Schedule/UpdateNH',
                        type: "POST",
                        complete: function (e) {
                            if (e != null) {
                                if (e.status == '500')
                                    $(".k-edit-form-container, .km-scroll-container").find("#validationSummary").empty().showBSDangerAlert("", "Error! Please try again.");
                                else if (e.status == '400')
                                    $(".k-edit-form-container, .km-scroll-container").find("#validationSummary").empty().showBSDangerAlert("", "Error! The start and end time is overlapped with another existing schedule of this owner.");
                                else {
                                    schedulerContentTopScroll = $('.k-scheduler-content').scrollTop();
                                    console.log("Scroll update: " + schedulerContentTopScroll);
                                    refreshScheduler();
                                }
                            }
                            HideLoading();
                        }
                    },
                    create: {
                        url: '/Schedule/CreateNH',
                        type: "POST",
                        complete: function (e) {
                            if (e != null) {
                                if (e.status == '500')
                                    $(".k-edit-form-container, .km-scroll-container").find("#validationSummary").empty().showBSDangerAlert("", "Error! Please try again.");
                                else if (e.status == '400')
                                    $(".k-edit-form-container, .km-scroll-container").find("#validationSummary").empty().showBSDangerAlert("", "Error! The start and end time is overlapped with an existing schedule of this owner.");
                                else {
                                    schedulerContentTopScroll = $('.k-scheduler-content').scrollTop();
                                    console.log("Scroll create: " + schedulerContentTopScroll);
                                    refreshScheduler();
                                }
                            }
                            HideLoading();
                        }
                    },
                    destroy: {
                        url: '/Schedule/DestroyNH',
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
                $(".km-pane-wrapper").css("position", "relative");
                var events = $('.k-event-template');
                var isDayView = $.trim($(".k-state-selected").data("name")).toLowerCase() == "day";
                // console.log(isDayView);
                var total = 0;
                for (var i = 0; i < events.length; i += 1) {
                    var currentEvent = $(events[i]);
                    var initialText = currentEvent.text();
                    var modifiedHtml = initialText.replaceAll("#div#", "<div>").replaceAll("#/div#", "</div>");
                    if (!isDayView) {
                        modifiedHtml = modifiedHtml.replaceAll("<div>", "<div style='display:none'>");
                    }
                    // added by husnain -- code start
                    /*
                    if (i % 2 !== 0 && isDayView) {
                        var s = modifiedHtml + '';
                        console.log(s);
                        if (s) {
                            var foundIndex = SplitString(s);
                            total += foundIndex;
                            console.log('index rate is : ' + foundIndex);
                            console.log('total index is :' + total.toFixed(2));
                        }
                    }
                    */
                    // husnain -- code end
                    currentEvent.html(modifiedHtml);
                }
                NHscheduler_view_range(e);
            },
            resources: [
                {
                    field: "ownerId",
                    title: "Owner",
                    dataSource: getPhysicians(false),
                }
            ]
        });
    }
    catch (e) {
        console.log(e.messages);
        HideLoading();
    }
}
function NHscheduler_view_range(e) {
    var view = e.sender.view();
    var startDatePST = view.startDate();
    var endDatePST = view.endDate();

    var sch = $("#scheduler").data("kendoScheduler");

    var startDate = formateDate(startDatePST);
    var endDate = formateDate(endDatePST);
    var physicians = checkMultiSelctVal($("#comboBox").data("kendoMultiSelect").value());
    $(sch.toolbar).children().remove("#ScheduleExportToExcel");
    $(sch.toolbar).children().remove("#ScheduleExportToiCal");

    var ExportToExcel = $("<a role='button' id='ScheduleExportToExcel' href='/Schedule/ExportNHSchedule?startDate=" + startDate + " &endDate=" + endDate + "&Physicians=" + physicians + "' class='k-button k-excel'><span class='k-icon k-i-file-excel'></span> Export to Excel</a>");
    $(sch.toolbar).prepend(ExportToExcel);

    if ($('#IsPhysician').val() == "1") {
        var ExportToiCal = $("<button id='ScheduleExportToiCal' onclick='ExportToiCal();' class='k-button k-save'><span class='k-icon k-i-calendar'></span>Export to iCal</button>");
        $(sch.toolbar).prepend(ExportToiCal);
    }

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
        event.startDate = ICAL.Time.fromDateTimeString(getISOString(schedulerEvent.start, true));
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

    console.log(component.toString());

    kendo.saveAs({
        dataURI: "data:text/calendar," + component.toString(),
        fileName: "ScheduleiCal_" + getISOString(new Date(), true).replace(/[_\W]+/g, "") + ".ics"
    });
}

function getISOString(date, toUTC) {
    date = toUTC ? kendo.timezone.convert(date, date.getTimezoneOffset(), "Etc/UTC") : date;
    return kendo.toString(date, "yyyy-MM-ddTHH:mm:ssZ");
}

function getTimeZone(timezone) {
    return timezone.toLowerCase() === "z" ? null : timezone;
}