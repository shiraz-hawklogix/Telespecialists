var clipboardDiv = document.createElement('div');


function copyHtmlToClipboard(html) {
    clipboardDiv.innerHTML = html;

    var focused = document.activeElement;
    clipboardDiv.focus();

    window.getSelection().removeAllRanges();
    var range = document.createRange();
    range.setStartBefore(clipboardDiv.firstChild);
    range.setEndAfter(clipboardDiv.lastChild);
    window.getSelection().addRange(range);

    var ok = false;
    try {
        if (document.execCommand('copy')) ok = true; else utils.log('execCommand returned false !');
    } catch (err) {
        console.log('execCommand failed ! exception ' + err);
    }

    focused.focus();
}

function initClipboardDiv() {
    clipboardDiv.style.fontSize = '12pt'; // Prevent zooming on iOS
    // Reset box model
    clipboardDiv.style.border = '0';
    clipboardDiv.style.padding = '0';
    clipboardDiv.style.margin = '0';
    // Move element out of screen
    clipboardDiv.style.position = 'fixed';
    clipboardDiv.style['right'] = '-9999px';
    clipboardDiv.style.top = (window.pageYOffset || document.documentElement.scrollTop) + 'px';
    // more hiding
    clipboardDiv.setAttribute('readonly', '');
    clipboardDiv.style.opacity = 0;
    clipboardDiv.style.pointerEvents = 'none';
    clipboardDiv.style.zIndex = -1;
    clipboardDiv.setAttribute('tabindex', '0'); // so it can be focused
    clipboardDiv.innerHTML = '';
    document.body.appendChild(clipboardDiv);
}

function getUclDataByType(data, typeKey) {
    var result = [];
    if (data) {
        result = data.filter(function (el) {
            return el.ucd_ucl_key == typeKey
        });
    }
    return result;
}

function checkPhysicianForCase(phy_key) {
    var url = "/Case/CheckPhysicianForCase";
    var result = null;
    $.ajax({
        async: false,
        type: 'Get',
        url: url,
        data: { Id: phy_key },
        success: function (response) {
            result = response;
        }
    });
    return result;
}

$(document).ready(function () {


    initClipboardDiv();
    // Its used to remove the status message in pages
    maskPhoneNumber();

    if ($(".PhyscianStatusUpdateTime").length > 0)
        upTime($(".PhyscianStatusUpdateTime").val(), $(".physicianStatusTimer"));
    onReadyCallBack();

    $(document).on("change", "#drp_physican_status", function () {
        var selectedValue = $(this).val();
        var url = "/physician/SetStatus";
        var selectedText = $("#drp_physican_status option:selected").text();

        if (selectedValue != "" && selectedValue != undefined) {
            $("#divModelPopUp").empty().showConfirmPopUp("Confirm",
                "<span>Are you sure you want to change the status to " + selectedText.trim() + "?</span>",
                function () {
                    changeStatus();

                });
        }
    });




    window.setTimeout(function () {
        $(".addValidation>input, .addValidation>select").show().css("visibility", "hidden"); // code added for kendo ui dropdowns, because they hides the actual control due to which validations are not working
    }, 1000);

    $("input").attr("autocomplete", "off");
    if ($("#new_case_notification").length > 0) {
        window.setTimeout(function () {
            document.getElementById('new_case_notification').muted = true;
            document.getElementById('new_case_notification').play();
        }, 5000);
    }
});

function changeStatus() {
    var selectedValue = $("#drp_physican_status").val();
    var url = "/physician/SetStatus";

    $.post(url, { id: selectedValue }, function (response) {
        if (response.success) {
            $("#divModelPopUp").modal("hide");

            $(".current-status-button, .current-status-button-layout").css("background-color", response.data.phs_color_code.toString());
            $(".current-status-button, .current-status-button-layout").removeClass("d-none");
            $(".currentStatus").html(response.data.phs_name);
            if (response.data.isResetTimer)
                window.setTimeout(function () {
                    upTime(new Date().toUTCString().replace(" GMT", ""), $(".physicianStatusTimer"));
                }, 100);

            if ($("#currentUrl").val() != undefined) {
                loadPageAsync($("#currentUrl").val());
            }

            $.ajax({
                url: "/Physician/_ChangeStatus?rand=" + Math.floor(Math.random() * Math.floor(3)),
                cache: false,
                success: function (response) {
                    $("#layout-status").empty().html(response);
                },
            });
        }
        else {
            $("#divModelPopUp").modal("hide");
        }
    });
}

function refreshCurrentPhyStatus(resetTime) {
    var isResetTime = true;
    if (typeof (resetTime) != undefined) {

        isResetTime = resetTime == "True";
    }
    var url = '/Physician/GetCurrentStatus?rnd' + Math.floor(Math.random() * 10000) + 1;
    $.get(url, null, function (response) {
        $(".physicianCurrentStatus").html(response.data);
        $("#drp_physican_status").val(response.status_key);
        if ($(".PhyscianStatusUpdateTime").length > 0 && isResetTime) {
            $(".PhyscianStatusUpdateTime").val(getCurrentUTCTime());
            upTime($(".PhyscianStatusUpdateTime").val(), $(".physicianStatusTimer"));
        }
        else {
            upTime($(".PhyscianStatusUpdateTime").val(), $(".physicianStatusTimer"));
        }
        if (closeNavigatorCasePopupWithNoQueue_def != undefined)
            closeNavigatorCasePopupWithNoQueue_def(); // close the navigator popup if it is there on page.

    });
}


function onReadyCallBack() {
    autoCloseAlert();


    //if ($("#PhyscianStatusUpdateTime").length > 0)
    //    upTime($("#PhyscianStatusUpdateTime").val(), $("#physicianStatusTimer"));



    AddAntiForgeryToken = function (data) {
        data.__RequestVerificationToken = $('form').eq(0).find('input[name=__RequestVerificationToken]').val();
        return data;
    };

    $(".allownumericwithdecimal").off("keypress keyup blur").on("keypress keyup blur", function (event) {
        $(this).val($(this).val().replace(/[^0-9\.]/g, ''));
        if ((event.which != 46 || $(this).val().indexOf('.') != -1) && (event.which < 48 || event.which > 57)) {
            event.preventDefault();
        }
    });

    $(".allownumericwithoutdecimal").off("keypress keyup blur").on("keypress keyup blur", function (event) {
        $(this).val($(this).val().replace(/[^\d].+/, ""));
        if ((event.which < 48 || event.which > 57)) {
            event.preventDefault();
        }
    });

    $(".allownumericwithoutdecimalwithEnter").off("keypress keyup blur").on("keypress keyup blur", function (event) {
        $(this).val($(this).val().replace(/[^\d].+/, ""));
        if ((event.which != 13 && event.which < 48 || event.which > 57)) {
            event.preventDefault();
        }
    });
}

$(document).on("click", ".loadModelPopup", function (e) {
    e.preventDefault();
    var url = $(this).attr("href");
    $.get(url, function (response) {
        $("#divModelPopUp").empty().html(response);
        $("#divModelPopUp").modal("show");
    });
});

function autoCloseAlert() {
    window.setTimeout(function () {
        $("#autoclose").slideUp(500, function () {
            $(this).remove();
        });
    }, 2000);
}

$(document).on("click", ".loadLinkAsync", function (e) {
    e.preventDefault();
    var url = $(this).attr("href");
    var target = $(this).data("target");
 if ($('#Physician').length == 1 && $('#is_super_admin').val() == "SuperAdmin" && url != "/Schedule/main") {
        if ($('#ScheduleMId').hasClass('Un-publish-sch')) {
            e.preventDefault();
            e.stopPropagation();
            $('#navigateUrl').val(url);
            $('#navigateTarget').val(target);
            $('#AllowToNavigateId').modal('show');
            return false;
        }
    }
    if (loadPageAsync.xhrRequest != undefined) {
        if (loadPageAsync.xhrRequest.readyState == 1)
            loadPageAsync.xhrRequest.abort();
    }

    loadPageAsync(url, target)
});
$(document).on("click", ".signOutCurrentUser", function (e) {
    signOutAndLogout();
});

$(document).on("click", ".loadLinkAsync-cancel", function (e) {
    e.preventDefault();
    var url = $(this).attr("href");
    var target = $(this).data("target");

    $("#divModelPopUp").empty().showCancelConfirmPopUp("Confirm",
        "Are you sure you want to close out of this page?",
        function () {
            $("#divModelPopUp").modal("hide");
            if (loadPageAsync.xhrRequest != undefined) {
                if (loadPageAsync.xhrRequest.readyState == 1)
                    loadPageAsync.xhrRequest.abort();
            }

            loadPageAsync(url, target);
        }
    );
});

$(document).on("click", ".clearLS", function (e) {
    localStorage.clear();
    clearAutoSaveInterval();
    $("#caseListActiveTab").val("emergentTab");
});

$(document).on("submit", ".ajaxForm>Form", function (e) {

    e.preventDefault();
    var url = $(this).attr("action");
    var data = $(this).serialize();
    $this = $(this);
    try {


        if ($this.isValid() == false) {
            return;
        }
    }
    catch (error) {

    }

    $("#divContentArea").find("input:button, input:submit").disable();
    $.ajax({
        type: "POST",
        url: url,
        dataType: 'json',
        data: data,
        error: function (xhr, error) {
            $("#divContentArea").find("input:button, input:submit").enable();
            $("#mainContentArea").find("#validationSummary").empty().showBSDangerAlert("", "Oops, Something went wrong.<br/>Please retry or contact us if the problem still persist.");
            //console.debug(xhr); console.debug(error);
        },
        success: function (response) {

            if (response.success) {
                // 
                var target = $this.data("target");

                if (response.refershPage) {
                    window.location.href = response.redirectUrl;
                }
                else {
                    $("#divModelPopUp").modal("hide");
                    if (response.message !== "" && response.message !== null) {
                        $("#mainContentArea").find("#validationSummary").empty().showBSSuccessAlert("", response.message);
                        $("#divContentArea").find("input:button, input:submit").enable();
                    }

                    if (response.redirectUrl != "" && response.redirectUrl != null)
                        loadPageAsync(response.redirectUrl, target);
                }
            }
            else {
                $("#divContentArea").find("input:button, input:submit").enable();
                $("#mainContentArea").find("#validationSummary").empty().showBSDangerAlert("", response.message);
                $("#divModelPopUp").modal("hide");
            }
        }
    });
});


$(document).on("keypress", ".type_integer", function (e) {
    var val = String.fromCharCode(e.keyCode);
    if (isNaN(val) || val == ' ') {
        e.preventDefault();
    }
});

$(document).on("paste, drop", ".type_integer", function (e) {
    e.preventDefault();
});


function formatPhoneNumber(phoneStr) {
    if ($.trim(phoneStr) != "") {
        s2 = ("" + phoneStr).replace(/\D/g, ''),
            m = s2.match(/^(\d{3})?[- ]??[\s]?(\d{3})?[\s]?(\d{4})?(.*)?$/);
        return (!m) ? null : "(" + m[1] + ") " + m[2] + "-" + m[3];
    }
    else {
        return "";
    }
}

function formatName(firstName, lastName) {
    var result = firstName !== null ? firstName : "";
    result += lastName !== null ? " " + lastName.charAt(0).toUpperCase() : "";
    return result;
}

function maskPhoneNumber() {
    if ($(".phone_number").kendoMaskedTextBox != undefined) {
        $(".phone_number").kendoMaskedTextBox({
            mask: "(999) 000-0000"
        });
    }
}
/* 
 * Added simply javascript method instead of extension method because setTimeout was creating issues in case of multiple 
 * instances in case of extension method.
 */
function upTime(countTo, element, callBackFunction) {
    if (element.length == 0)
        return;
    try {
        now = new Date(new Date().toUTCString().replace(" GMT", ""));
        countTo = new Date(countTo);
        difference = (now - countTo);
        days = Math.floor(difference / (60 * 60 * 1000 * 24) * 1);
        years = Math.floor(days / 365);
        if (years > 1) { days = days - (years * 365) }
        hours = Math.floor((difference % (60 * 60 * 1000 * 24)) / (60 * 60 * 1000) * 1);
        mins = Math.floor(((difference % (60 * 60 * 1000 * 24)) % (60 * 60 * 1000)) / (60 * 1000) * 1);
        secs = Math.floor((((difference % (60 * 60 * 1000 * 24)) % (60 * 60 * 1000)) % (60 * 1000)) / 1000 * 1);
        //document.getElementById('years').firstChild.nodeValue = years;
        // document.getElementById('days').firstChild.nodeValue = days;

        if (secs.formatNumber() >= 0) {

            var dd = days * 24;
            var hrs = (hours + (days >= 1 ? dd : 0));

            element.find("#hours").html(hrs);
            element.find('#minutes').html(mins.formatNumber(2));
            element.find('#seconds').html(secs.formatNumber(2));

            var caseStatus = $("#cas_cst_key").val();
            if (hrs > 0 && caseStatus != caseStatusEnum.Accepted)
                element.find("#custom").addClass("elapsed-time");
            else if (mins.formatNumber(2) > 7 && caseStatus != caseStatusEnum.Accepted)
                element.find("#custom").addClass("elapsed-time");
            else
                element.find("#custom").removeClass("elapsed-time");

            if (typeof (callBackFunction) == "function") {
                callBackFunction();
            }
        }
        else {
            element.find("#hours").html("00");
            element.find('#minutes').html("00");
            element.find('#seconds').html("00");

        }

        var elementId = element.attr("id");

        clearTimeout(upTime[elementId]);

        element.show();
        if (callBackFunction != undefined)
            upTime[elementId] = setTimeout(function () { upTime(countTo, element, callBackFunction); }, 1000);
        else
            upTime[elementId] = setTimeout(function () { upTime(countTo, element); }, 1000);
    } catch (e) { console.log(e); }
}

function startTimerFromDate(countTo, element, doRest) {
    if (typeof (doRest) == 'undefined') {
        doRest = false;
    }

    var elementId = element.attr("id");
    if (doRest) {
        clearTimeout(startTimerFromDate[elementId]);
    }
    else {
        now = new Date(new Date().toUTCString().replace(" GMT", ""));
        countTo = new Date(countTo);
        difference = (now - countTo);
        days = Math.floor(difference / (60 * 60 * 1000 * 24) * 1);
        hours = Math.floor((difference % (60 * 60 * 1000 * 24)) / (60 * 60 * 1000) * 1);
        mins = Math.floor(((difference % (60 * 60 * 1000 * 24)) % (60 * 60 * 1000)) / (60 * 1000) * 1);
        secs = Math.floor((((difference % (60 * 60 * 1000 * 24)) % (60 * 60 * 1000)) % (60 * 1000)) / 1000 * 1);

        var dd = days * 24;
        var hrs = (hours + (days >= 1 ? dd : 0));
        element.find("#hours").html(hrs);
        element.find('#minutes').html(mins.formatNumber(2));
        element.find('#seconds').html(secs.formatNumber(2));

        clearTimeout(startTimerFromDate[elementId]);
        element.show();
        startTimerFromDate[elementId] = setTimeout(function () { startTimerFromDate(countTo, element); }, 1000);
    }
}

function onDropdDownOpen(e) {
    setTimeout(function () {
        if ($("#five-9-nav-container").length) {
            var elementLeft = $(e.sender.wrapper).offset().left;
            var five9Left = parseFloat($("#five-9-nav-container").offset().left);
            var five9Width = parseFloat($("#five-9-nav-container").width());
            if (five9Left == 0) {
                $(e.sender.list).parents(".k-animation-container").css("left", "");
                var currentOffset = (elementLeft - five9Width) - 20;
                $(e.sender.list).parents(".k-animation-container").css("left", currentOffset.toFixed(2) + "px");
            }
        }

    });
}

function onDatePickerOpen(e) {
    setTimeout(function () {
        if ($("#five-9-nav-container").length) {
            var elementLeft = $(e.sender.wrapper).offset().left;
            var five9Left = parseFloat($("#five-9-nav-container").offset().left);
            var five9Width = parseFloat($("#five-9-nav-container").width());
            if (five9Left == 0) {
                $(e.sender.list).parents(".k-animation-container").css("left", "");
                var currentOffset = (elementLeft - five9Width) - 20;
                $(e.sender.dateView.div).parents(".k-animation-container").css("left", currentOffset.toFixed(2) + "px");
            }
        }
    });
}

function onTimePickerOpen(e) {
    setTimeout(function () {
        if ($("#five-9-nav-container").length) {
            var elementLeft = $(e.sender.wrapper).offset().left;
            var five9Left = parseFloat($("#five-9-nav-container").offset().left);
            var five9Width = parseFloat($("#five-9-nav-container").width());
            if (five9Left == 0) {
                $(e.sender.list).parents(".k-animation-container").css("left", "");
                var currentOffset = (elementLeft - five9Width) - 20;
                $(e.sender.timeView.list).parents(".k-animation-container").css("left", currentOffset.toFixed(2) + "px");
            }
        }
    });
}

function destryCkEditorField(fieldId) {
    try {
        CKEDITOR.instances[fieldId].destroy(true);
    } catch (e) { }
}

function getRandomInt(max) {
    return Math.floor(Math.random() * Math.floor(max));
}


function loadPageAsync(url, target) {

    checkUserLoggedInStatus();
    if (url != undefined) {
        url = url.addRandomParamToUrl();

        if (target == undefined || target == "" ) {
            target = "divContentArea"
        }

        var isPhysician = $("#IsPhysician").val() == "1" ? true : false;

        if (!isPhysician && url.indexOf("_CurrentStatus") == -1)
            $(".physicianCurrentStatus").html("");

        $("#hdnDisableLoader").val("0");

        loadPageAsync.xhrRequest = $.get(url, function (response) {
            loadPageAsync.xhrRequest = null;
            $(".error-message").slideUp(500, function () {
                $(this).remove();
            });

            $("#hdnCurrentUrl").val(url);

            $("#" + target).empty().html(response + $("#hdnJQueryValidations").val());
            onReadyCallBack();
        });
        ScheduleCheckLoad();
    }
}

function setClipboardText(text) {
    var id = "mycustom-clipboard-textarea-hidden-id";
    var existsTextarea = document.getElementById(id);

    if (!existsTextarea) {
        logToConsole("Creating textarea");
        var textarea = document.createElement("textarea");
        textarea.id = id;
        // Place in top-left corner of screen regardless of scroll position.
        textarea.style.position = 'fixed';
        textarea.style.top = 0;
        textarea.style.left = 0;

        // Ensure it has a small width and height. Setting to 1px / 1em
        // doesn't work as this gives a negative w/h on some browsers.
        textarea.style.width = '1px';
        textarea.style.height = '1px';

        // We don't need padding, reducing the size if it does flash render.
        textarea.style.padding = 0;

        // Clean up any borders.
        textarea.style.border = 'none';
        textarea.style.outline = 'none';
        textarea.style.boxShadow = 'none';

        // Avoid flash of white box if rendered for any reason.
        textarea.style.background = 'transparent';
        document.querySelector("body").appendChild(textarea);
        logToConsole("The textarea now exists :)");
        existsTextarea = document.getElementById(id);
    } else {
        logToConsole("The textarea already exists :3")
    }

    existsTextarea.value = text;
    existsTextarea.select();
    $('#txtAreaCopy').val('');
    $('#txtAreaCopy').val(text);
    try {
        
        var status = document.execCommand('copy');
        if (!status) {
           // copyCase();
            logToConsole("Cannot copy text");
        } else {
            logToConsole("The text is now on the clipboard");
            document.body.removeChild(existsTextarea);
        }
    } catch (err) {
        logToConsole('Unable to copy.');
    }
}

function CartlocationText(cartLocationText, div) {
    $(div).append("Cart Location: " + cartLocationText);
    $(div).append("##NewLine##");
}


function copyCase() {

    $("#divCaseCopy").html("");

    var startTime = $.trim($("#five9_start_time").val());

    if ($.trim(startTime) == "") {
        startTime = $.trim($("#cas_response_ts_notification").val());
    }

    if ($.trim(startTime) == "") {
        startTime = $.trim($("#cas_metric_stamp_time_est").val());
    }

    var hdnTimeZone = $("#hdnFacilityTimeZone").val();
    if (hdnTimeZone != "")
        hdnTimeZone = getFirstLetterOfEarchWord(hdnTimeZone);
    else
        hdnTimeZone = "EST";

    if ($.trim(startTime) != "") {
        if (hdnTimeZone != null && hdnTimeZone != "") {
            startTime = startTime + " " + hdnTimeZone + " - Local Time ";
            $("#divCaseCopy").append(startTime);
        }
        else
            $("#divCaseCopy").append(startTime);

        $("#divCaseCopy").append("##NewLine##");
    }

    //if (startTime != "" && startTime != undefined) {
    //    $("#divCaseCopy").append(startTime);
    //    $("#divCaseCopy").append("##NewLine##");
    //}

    var caseType = $.trim($("#cas_ctp_key option:selected").text()).replace("-- Select --", "");
    if ($.trim(caseType) != "") {
        $("#divCaseCopy").append(caseType);
        $("#divCaseCopy").append("##NewLine##");
    }

    var facility = $.trim($("#cas_fac_key option:selected").text()).replace("--Select--", "");
    if ($.trim(facility) != "") {
        $("#divCaseCopy").append(facility);
        $("#divCaseCopy").append("##NewLine##");
    }

    var cart = $.trim($("#cas_cart").val());
    if ($.trim(cart) != "") {
        $("#divCaseCopy").append("Cart: " + cart);
        $("#divCaseCopy").append("##NewLine##");
    }

    var cartLocation = $.trim($('#cas_cart_location_key option:selected').text()).replace("-- Select --", "");
    if (cartLocation == "Other") {
        var cartLocationText = $.trim($('#cas_cart_location_text').val());
        if (cartLocationText != "") {
            CartlocationText('<span style="word-wrap: break-word;">' + cartLocationText + '</span>', '#divCaseCopy')
        }
        else {
            CartlocationText(cartLocation, '#divCaseCopy')
        }
    }
    else if (cartLocation != "") {
        CartlocationText(cartLocation, '#divCaseCopy')
    }


    var callBack = $.trim($("#cas_callback:visible").val());
    if ($.trim(callBack) != "") {
        $("#divCaseCopy").append("Callback Phone: " + callBack);
        $("#divCaseCopy").append("##NewLine##");
    }

    var extension = $.trim($("#cas_callback_extension:visible").val());
    if (extension != "") {
        $("#divCaseCopy").append("Extension: " + extension);
        $("#divCaseCopy").append("##NewLine##");
    }

    var patient = $.trim($("#general_cas_patient_name:visible").val());
    if ($.trim(patient) != "") {
        $("#divCaseCopy").append("Patient Name: " + patient);
        $("#divCaseCopy").append("##NewLine##");
    }

    var triageNotes = $.trim($("#cas_triage_notes:visible").val());
    if ($.trim(triageNotes) != "") {
        $("#divCaseCopy").append("RRC Manager Notes: " + triageNotes);
        $("#divCaseCopy").append("##NewLine##");
    }

    var notes = $.trim($("#cas_notes:visible").val());
    if (notes != undefined && notes != "") {

        var notesMarkup = '';
        $(notes.split("\n")).each(function (i, n) {
            if (n != "") {
                notesMarkup += n + '##NewLine##';
            }
        });

        $("#divCaseCopy").append("Alert Notes: " + notesMarkup);
        //$("#divCaseCopy").append("##NewLine##");
    }

    //Added BY Axim 24-08-2020
    var navigatorNotes = $.trim($("#cas_navigator_stamp_notes:visible").val());
    if ($.trim(navigatorNotes) != "") {
        $("#divCaseCopy").append("Navigator Stamp Notes: " + navigatorNotes);
        $("#divCaseCopy").append("##NewLine##");
    }
    //ended by axim 24-08-2020
     // Comment below code to hide eta in dialog alert box by husnain
    /*
    if ($("#case-eta").prop("checked")) {
        var ETA = $.trim($("#cas_eta").val());
        if (ETA) {
            $("#divCaseCopy").append("ETA: " + ETA);
            $("#divCaseCopy").append("##NewLine##");
        }
    }
    */

    if ($("#cas_phy_has_technical_issue").prop("checked") && $.trim($("#cas_phy_technical_issue_date_est:visible").val()) != "") {
        var technical_issue_date = $.trim($("#cas_phy_technical_issue_date_est").val());
        if (technical_issue_date) {
            $("#divCaseCopy").append("Physician Having Technical Issues: " + technical_issue_date);
            $("#divCaseCopy").append("##NewLine##");
        }
    }

    //New for 387
    var callbackResponseTime = $.trim($("#cas_callback_response_time_est:visible").val());
    if (callbackResponseTime != "") {
        $("#divCaseCopy").append("Callback Response Time: " + callbackResponseTime);
        $("#divCaseCopy").append("##NewLine##");
    }

    // $("#btnCopyHidden").click();
    var textToCopy = $("#divCaseCopy").text();
    if (textToCopy != "" && textToCopy != undefined) {
        setClipboardText(textToCopy.replaceAll("##NewLine##", "\r\n"));
        var textToPreview = textToCopy.replaceAll("##NewLine##", "<br/>");
        $("#caseCopyPopUp").find(".modal-body").empty().html(textToPreview);
        var isRefreshed = $("#caseCopyPopUp").hasClass("show");
        $("#caseCopyPopUp").modal("show");
        logCopyCaseInfo(isRefreshed);
    }
}
function copyFirebase() {

    $("#divCaseCopy").html("");

    var startTime = $.trim($("#five9_start_time").val());



    if ($.trim(startTime) == "") {
        startTime = $.trim($("#cas_response_ts_notification").val());
    }

    if ($.trim(startTime) == "") {
        startTime = $.trim($("#cas_metric_stamp_time_est").val());
    }

    var hdnTimeZone = $("#hdnFacilityTimeZone").val();
    if (hdnTimeZone != "")
        hdnTimeZone = getFirstLetterOfEarchWord(hdnTimeZone);
    else
        hdnTimeZone = "EST";

    if ($.trim(startTime) != "") {
        if (hdnTimeZone != null && hdnTimeZone != "") {
            startTime = startTime + " " + hdnTimeZone + " - Local Time ";
            $("#divCaseCopy").append(startTime);
        }
        else
            $("#divCaseCopy").append(startTime);

        $("#divCaseCopy").append("##NewLine##");
    }

    //if (startTime != "" && startTime != undefined) {
    //    $("#divCaseCopy").append(startTime);
    //    $("#divCaseCopy").append("##NewLine##");
    //}

    var caseType = $.trim($("#cas_ctp_key option:selected").text()).replace("-- Select --", "");
    if ($.trim(caseType) != "") {
        $("#divCaseCopy").append(caseType);
        $("#divCaseCopy").append("##NewLine##");
    }



    var facility = $.trim($("#cas_fac_key option:selected").text()).replace("--Select--", "");
    if ($.trim(facility) != "") {
        $("#divCaseCopy").append(facility);
        $("#divCaseCopy").append("##NewLine##");
    }

    var cart = $.trim($("#cas_cart").val());
    if ($.trim(cart) != "") {
        $("#divCaseCopy").append("Cart: " + cart);
        $("#divCaseCopy").append("##NewLine##");
    }

    var cartLocation = $.trim($('#cas_cart_location_key option:selected').text()).replace("-- Select --", "");
    if (cartLocation == "Other") {
        var cartLocationText = $.trim($('#cas_cart_location_text').val());
        if (cartLocationText != "") {
            CartlocationText('<span style="word-wrap: break-word;">' + cartLocationText + '</span>', '#divCaseCopy')
        }
        else {
            CartlocationText(cartLocation, '#divCaseCopy')
        }
    }
    else if (cartLocation != "") {
        CartlocationText(cartLocation, '#divCaseCopy')
    }


    var callBack = $.trim($("#cas_callback:visible").val());
    if ($.trim(callBack) != "") {
        $("#divCaseCopy").append("Callback Phone: " + callBack);
        $("#divCaseCopy").append("##NewLine##");
    }

    var extension = $.trim($("#cas_callback_extension:visible").val());
    if (extension != "") {
        $("#divCaseCopy").append("Extension: " + extension);
        $("#divCaseCopy").append("##NewLine##");
    }

    var patient = $.trim($("#general_cas_patient_name:visible").val());
    if ($.trim(patient) != "") {
        $("#divCaseCopy").append("Patient Name: " + patient);
        $("#divCaseCopy").append("##NewLine##");
    }


    var triageNotes = $.trim($("#cas_triage_notes:visible").val());
    if ($.trim(triageNotes) != "") {
        $("#divCaseCopy").append("Triage Notes: " + triageNotes);
        $("#divCaseCopy").append("##NewLine##");
    }

    var notes = $.trim($("#cas_notes:visible").val());
    if (notes != undefined && notes != "") {

        var notesMarkup = '';
        $(notes.split("\n")).each(function (i, n) {
            if (n != "") {
                notesMarkup += n + '##NewLine##';
            }
        });

        $("#divCaseCopy").append("Notes: " + notesMarkup);
        $("#divCaseCopy").append("##NewLine##");
    }
    // Comment below code to hide eta in dialog alert box by husnain
    /*
    if ($("#case-eta").prop("checked")) {
        var ETA = $.trim($("#cas_eta").val());
        if (ETA) {
            $("#divCaseCopy").append("ETA: " + ETA);
            $("#divCaseCopy").append("##NewLine##");
        }
    }
    */

    if ($("#cas_phy_has_technical_issue").prop("checked") && $.trim($("#cas_phy_technical_issue_date_est:visible").val()) != "") {
        var technical_issue_date = $.trim($("#cas_phy_technical_issue_date_est").val());
        if (technical_issue_date) {
            $("#divCaseCopy").append("Physician Having Technical Issues: " + technical_issue_date);
            $("#divCaseCopy").append("##NewLine##");
        }
    }

    //New for 387
    var callbackResponseTime = $.trim($("#cas_callback_response_time_est:visible").val());
    if (callbackResponseTime != "") {
        $("#divCaseCopy").append("Callback Response Time: " + callbackResponseTime);
        $("#divCaseCopy").append("##NewLine##");
    }
}

function logCopyCaseInfo(isRefreshed) {

    var stampTime = $.trim($("#five9_start_time").val());

    if (stampTime == "" || stampTime == undefined) {
        stampTime = $.trim($("#cas_metric_stamp_time_est").val());
    }

    if (stampTime == "" || stampTime == undefined) {
        stampTime = $.trim($("#cas_response_ts_notification").val());
    }

    var sourceTime = $.trim($("#five9_intial_utc_time").val());
    if (sourceTime == "")
        sourceTime = $.trim($("#five9_intial_utc_time").val());

    var copiedText = $("#caseCopyPopUp").find(".modal-body").html();

    var data = {
        "cpy_source_timezone": sourceTime != "" ? "UTC" : "", // set source time zone only, if there is source time. 
        "cpy_source_time": sourceTime,
        "cpy_target_timezone_offset": $("#hdnFacilityTimeZoneOffSet").val(),
        "cpy_target_timezone": $("#hdnFacilityTimeZone").val(),
        "cpy_target_time": stampTime,
        "cpy_five9_original_stamp_time": $("#cas_five9_original_stamp_time").val(),
        "cpy_case_key": $("#cas_key").val() == "0" ? null : $("#cas_key").val(),
        "cpy_fac_key": $("#cas_fac_key").val(),
        "cpy_page_url": $("#CasePageUrl").val(),
        "cpy_copied_text": copiedText,
        "cpy_is_info_refreshed": isRefreshed,
        "cpy_call_id": $("#cas_call_id").val(),
        "cpy_fac_name": $.trim($("#cas_fac_key option:selected").text()).replace("--Select--", ""),
        "cpy_browser_name": getBrowserName()

    };


    var url = '/CaseCopyLog/Add';


    $.ajax({
        type: "POST",
        url: url,
        dataType: "json",
        data: JSON.stringify(data),
        contentType: "application/json; charset=utf-8",
        success: function (data) {
        },
        error: function (error) {
            logToConsole(error);
        }
    });
}
// Js Spinner
function ShowLoading() {
    $('div#processing').attr("style", "display:block");
    jsSpin();
}

function HideLoading() {
    $('div#processing').attr("style", "display:none");
    $('div#processing').find('.spinner').remove();
}

function KendoKeepState(prefix, options, saveFilters) {

    localStorage.setItem(prefix + '.RecPerPage', options.take);
    localStorage.setItem(prefix + '.page', options.page);
    if (options.sort) {
        localStorage.setItem(prefix + '.Sort', JSON.stringify(options.sort));
    } else {
        var prefixSort = JSON.parse(localStorage.getItem(prefix + ".Sort"));
        if (prefixSort != null && prefixSort.length > 0) {
            options.sort = prefixSort;
        }
    }

    if (saveFilters !== undefined) {
        if (options.filter) {
            localStorage.setItem(prefix + '.Filter', JSON.stringify(options.filter));
        } else {
            var filters = JSON.parse(localStorage.getItem(prefix + ".Filter"));
            if (filters != null && filters.filters.length > 0) {
                options.filter = filters;
            }
        }
    }

    return options;
}


function KendoGet(prefix, propertyName) {

    // RecPerPage
    if (propertyName == undefined)
        propertyName = "RecPerPage";

    var propertyVal = 10;
    var pp = localStorage.getItem(prefix + '.' + propertyName);

    if (pp == "undefined") {
        var resultedValue = GetKendoTotalCount(prefix);
        propertyVal = resultedValue;
    }

    else if (pp != null && pp != undefined) {
        propertyVal = pp;
    }
    else {
        if (propertyName == "page")
            propertyVal = 1;
    }

    return propertyVal;
}

function GetKendoPageSize(prefix, propertyName, isCheckCounter) {

    // RecPerPage
    if (propertyName == undefined)
        propertyName = "RecPerPage";

    var propertyVal = 10;
    var pp = localStorage.getItem(prefix + '.' + propertyName);

    if (isCheckCounter) {
        var pageCountValue = localStorage.getItem(prefix + '.' + "RecPerPage");
        if (pageCountValue == "undefined") {
            var resultedValue = GetKendoTotalCount(prefix);
            propertyVal = resultedValue;
        }
        else if (pageCountValue != null)
            propertyVal = pageCountValue;

    }

    else if (pp != null && pp != undefined) {
        propertyVal = pp;
    }
    else {
        if (propertyName == "page")
            propertyVal = 1;
    }

    return propertyVal;
}


function SetKendoTotalCount(prefix, totalCount) {

    localStorage.setItem(prefix + '.TotalCount', totalCount);
}

function GetKendoTotalCount(prefix) {

    var totalCount = JSON.parse(localStorage.getItem(prefix + ".TotalCount"));
    return totalCount;
}

function SetKendoRoleIds(prefix, roleIds) {
    localStorage.setItem(prefix + '.RoleIds', roleIds);
}
function GetKendoRoleIds(prefix) {
    var roleIds = localStorage.getItem(prefix + ".RoleIds");
    return roleIds;
}


function jsSpin() {
    var opts = {
        lines: 13, // The number of lines to draw
        length: 0, // The length of each line
        width: 25, // The line thickness
        radius: 60, // The radius of the inner circle
        corners: 1, // Corner roundness (0..1)
        rotate: 0, // The rotation offset
        direction: 1, // 1: clockwise, -1: counterclockwise
        color: '#000', // #rgb or #rrggbb or array of colors
        speed: 1.1, // Rounds per second
        trail: 52, // Afterglow percentage
        shadow: false, // Whether to render a shadow
        hwaccel: false, // Whether to use hardware acceleration
        className: 'spinner', // The CSS class to assign to the spinner
        zIndex: 2e9, // The z-index (defaults to 2000000000)
        top: 'auto', // Top position relative to parent in px
        left: 'auto' // Left position relative to parent in px
    };
    var target = document.getElementById('processing');
    var spinner = new Spinner(opts).spin(target);
}

$(document).ajaxStart(function () {
    //ajax request went so show the loading image
    var disableLoading = $("#hdnDisableLoader").val() == "1" ? true : false;
    if (!disableLoading) {
        ShowLoading();
    }
});



$(document).ajaxError(function () {
    HideLoading();
});

$(document).ajaxStop(function (e) {
    HideLoading();
});

$(document).ajaxComplete(function () {
    HideLoading();
});

function showFacilityTime(timeZoneOffset) {
    var currentDateTime = moment().utcOffset(timeZoneOffset);
    $("#FacilityTime").html(currentDateTime.format("HH:mm:ss"));
    if (showFacilityTime.To != undefined)
        window.clearTimeout(showFacilityTime.To);
    showFacilityTime.To = window.setTimeout(function () {
        showFacilityTime(timeZoneOffset);
    }, 1000);
    $("#FacilityTime").show();
}


function createCookie(cookieName, cookieValue, expiryDateTime) {
    var data = cookieName + "=" + btoa(cookieValue) + "; expires=" + expiryDateTime.toGMTString();
    document.cookie = data;
}

function getCookie(cookieName) {
    var name = cookieName + "=";
    var allCookieArray = document.cookie.split(';');
    for (var i = 0; i < allCookieArray.length; i++) {
        var temp = allCookieArray[i].trim();
        if (temp.indexOf(name) == 0) {
            var value = temp.substring(name.length, temp.length);
            return atob(value);
        }

    }
    return "";
}

var deleteCookie = function (name) {
    document.cookie = name + '=;expires=Thu, 01 Jan 1970 00:00:01 GMT;';
};

function getBrowserName() {
    var browserName = "";
    try {
        var isChrome = /Chrome/.test(navigator.userAgent) && /Google Inc/.test(navigator.vendor);
        var isIE = false || !!document.documentMode;

        if (isChrome && navigator.userAgent.indexOf("OPR") == -1) {

            browserName = "Chrome";
        }
        else {
            // Firefox 1.0+
            var isFirefox = typeof InstallTrigger !== 'undefined';
            if (isFirefox) {
                browserName = "FireFox";
            }
            else {
                var isEdge = !isIE && !!window.StyleMedia;
                if (isEdge) {
                    browserName = "Edge";
                }
                else {

                    if (isIE) {
                        browserName = "Internet Explorer";
                    }
                    else {
                        var isOpera = navigator.userAgent.indexOf(' OPR/') >= 0;
                        if (isOpera) {
                            browserName = "Opera";
                        }
                        else {
                            var isSafari = /constructor/i.test(window.HTMLElement) || (function (p) { return p.toString() === "[object SafariRemoteNotification]"; })(!window['safari'] || safari.pushNotification);
                            if (isSafari) {
                                browserName = "Safari";
                            }
                        }
                    }
                }
            }
        }


    }
    catch (e) {
        browserName = "";
    }
    return browserName;
}
function TestFive9PopupCode() {

    var ani = 9139630288;
    var dnis = 2392311456;
    var callId = 300000000000483;
    var campaignId = 1137587;
    var timeStamp = new Date(new Date().toUTCString()).getTime();
    var caseType = "Stroke Alert";

    var url = "/case/create?ani=" + ani + "&dnis=" + dnis + "&call_id=" + callId + "&campaign=" + campaignId + "&start_timestamp=" + timeStamp + "&case_type=" + caseType;

    var target = "divContentArea"

    if ($('#createForm').length > 0) {
        $("#IsAutoSave").val("1");
        $("#createForm").submit();
    }

    $.get(url, function (response) {

        $("#hdnCurrentUrl").val(url);
        $("#" + target).empty().html(response + $("#hdnJQueryValidations").val());
        onReadyCallBack();
    });
}

function inputNumericOnly(element, e) {
    // Allow: backspace, delete, tab, escape, enter and .
    if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 190]) !== -1 ||
        // Allow: Ctrl+A
        (e.keyCode == 65 && e.ctrlKey === true) ||
        // Allow: home, end, left, right
        (e.keyCode >= 35 && e.keyCode <= 39)) {
        // let it happen, don't do anything
        return;
    }
    // Ensure that it is a number and stop the keypress
    if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
        e.preventDefault();
    }
}

function getFirstLetterOfEarchWord(inputStrig) {
    var str = inputStrig.toString();
    if (inputStrig != null && inputStrig != "") {
        var matches = str.match(/\b(\w)/g);
        var result = matches.join('');
        return result;
    }
    else
        return "EST";
}

function logToConsole(obj) {
    if (enableJSLogging) {
        console.log(obj);
    }
}




function showCKEditor(elementId) {

    destryCkEditorField(elementId);
    window.setTimeout(function () {
        CKEDITOR.replace(elementId);
    }, 100);

    window.setTimeout(function () {
        $(".cke_toolbar").each(function () {
            $this = $(this);
            if ($(this).find("a:visible").length <= 0) {
                $this.hide()
            } else {
                $this.show();
            }
        });
    }, 300);
}

// tableId == container or grid which contain physician intials value (column)
// columnIndex == starts from 1
function formatPhysicianInitials(tableId, columnIndex) {
    $(tableId).find("tr").each(function (index, element) {
        var row = $(element);
        var result_array = [];
        var physician = row.find("td:nth-child(" + columnIndex + ")").text();
        if ($.trim(physician) != "") {
            var phy_array = physician.split('/');
            var i = 0;
            if (phy_array.length > 1) {
                while (i < phy_array.length) {
                    var initial = phy_array[i];
                    if (i > 0) {
                        if (phy_array[i - 1] != initial) // (phy_array[i - 1].indexOf(initial) == -1)
                            result_array.push(initial);
                    }
                    else {
                        result_array.push(initial);
                    }

                    i++;
                }
            }
            // if only one entry in history
            if (phy_array.length == 1) {
                result_array.push(phy_array[0]);
            }

            row.find("td:nth-child(" + columnIndex + ")").html(result_array.join("/"));
        }
    });
}

function getTotalPhysicianRotation(tableId, columnIndex) {
    $(tableId).find("tr").each(function (index, element) {
        var row = $(element);
        var result_array = [];
        var physician = row.find("td:nth-child(" + columnIndex + ")").text();
        if ($.trim(physician) != "") {
            var phy_array = physician.split('/');
            var i = 0;
            if (phy_array.length > 1) {
                while (i < phy_array.length) {
                    var initial = phy_array[i];
                    if (i > 0) {
                        if (phy_array[i - 1] != initial) // (phy_array[i - 1].indexOf(initial) == -1)
                            result_array.push(initial);
                    }
                    else {
                        result_array.push(initial);
                    }

                    i++;
                }
            }
            // if only one entry in history
            if (phy_array.length == 1) {
                result_array.push(phy_array[0]);
            }

            row.find("td:nth-child(" + columnIndex + ")").html(result_array.length);
        }
    });
}

function escapeHtml(string) {

    var entityMap = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#39;',
        '/': '&#x2F;',
        '`': '&#x60;',
        '=': '&#x3D;'
    };


    return String(string).replace(/[&<>"'`=\/]/g, function (s) {
        return entityMap[s];
    });
}

function checkMobileDevice() {
    if (/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|ipad|iris|kindle|Android|Silk|lge |maemo|midp|mmp|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino/i.test(navigator.userAgent)
        || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(navigator.userAgent.substr(0, 4))) {
        return true;
    }
    return false
}

var convertSeconds = function (sec) {
    var hrs = Math.floor(sec / 3600);
    var min = Math.floor((sec - (hrs * 3600)) / 60);
    var seconds = sec - (hrs * 3600) - (min * 60);
    seconds = Math.round(seconds * 100) / 100

    var result = (hrs < 10 ? "0" + hrs : hrs);
    result += ":" + (min < 10 ? "0" + min : min);
    result += ":" + (seconds < 10 ? "0" + seconds : seconds);
    return result;
}

function clearAutoSaveInterval() {
    if (typeof (autoSaveCase) != 'undefined') {
        if (autoSaveCase.timeout != undefined) {
            window.clearTimeout(autoSaveCase.timeout);
        }
    }
}

function initComboDate(selector) {
    $(selector).combodate({
        firstItem: 'name', //show 'hour' and 'minute' string at first item of dropdown
        minuteStep: 1
    });


    window.setTimeout(function () {
        $(".hour").addClass("form-control").css("display", "inline-block");
        $(".minute").addClass("form-control").css("display", "inline-block");
        $(".second").addClass("form-control").css("display", "inline-block");
    }, 100);
}

function timerControlChange(selector) {
    $(selector).each(function () {


        var hour = $(this).parent().find(".hour");
        var minute = $(this).parent().find(".minute");
        var second = $(this).parent().find(".second");


        if (hour.length > 0)
            if (($.trim(hour.val()) == "" && $.trim(minute.val()) != "") || ($.trim(hour.val()) == "" && $.trim(second.val()) != "")) {
                hour.val("0");
                hour.change();
            }

        if (minute.length > 0)
            if (($.trim(minute.val()) == "" && $.trim(hour.val()) != "") || ($.trim(minute.val()) == "" && $.trim(second.val()) != "")) {
                minute.val("0");
                minute.change();
            }

        if (second.length > 0)
            if (($.trim(second.val()) == "" && $.trim(hour.val()) != "") || ($.trim(second.val()) == "" && $.trim(minute.val()) != "")) {
                second.val("0");
                second.change();
            }


    });
}

function getCurrentUTCTime() {
    return moment.utc().format("MM/DD/YYYY HH:mm:ss");
}

function ValidateURL(url) {

    if (url == null || url.length == 0)
        return true;

    var expression = /https?\:\/\/[-a-zA-Z0-9@@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@@:%_\+.~#?&//=]*)/;

    var regex = new RegExp(expression);
    var result = regex.test(url);

    return result;
}

function ShowHideTimeZone(timepicker) {

    if (timepicker == undefined || timepicker == null)
        return;

    var timeZoneContainer = $(timepicker).parents(".timer-container").find(".est-container").first();
    var datetime = $(timepicker).val().replace("_", "").replace("/", "");
    if (datetime != "") {
        $(timeZoneContainer).show();
    }
    else {

        $(timeZoneContainer).hide();
    }
}

function changeAbbrivation(abbr) {
    $(".timer-container > .est-container").find("span").html(abbr);
}

function isValidDate(dateString) {
    // First check for the pattern
    if (!/^\d{1,2}\/\d{1,2}\/\d{4}$/.test(dateString))
        return false;

    // Parse the date parts to integers
    var parts = dateString.split("/");
    var day = parseInt(parts[1], 10);
    var month = parseInt(parts[0], 10);
    var year = parseInt(parts[2], 10);

    // Check the ranges of month and year
    if (year < 1000 || year > 3000 || month == 0 || month > 12)
        return false;

    var monthLength = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];

    // Adjust for leap years
    if (year % 400 == 0 || (year % 100 != 0 && year % 4 == 0))
        monthLength[1] = 29;

    // Check the range of the day
    return day > 0 && day <= monthLength[month - 1];
}
function isValidTime(value) {  
    // regular expression to match required time format
   return /^([01]?[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$/.test(value)        
}

function isValidDOB(dateString) {
    // First check for the pattern
    if (isValidDate(dateString)) {
        var date = new Date(dateString);
        var currentDate = new Date($("#hdnSystemDateTime").val());
        var duration = moment.duration(moment(currentDate).diff(new Date(date)));

        if (duration.asDays() < 0) {
            return false;
        }
        else {
            return true;
        }
    }

    return false;
}

function adjustCaseHeaderColWidth() {
    var width = $.trim($("#case_edit_header_col5").outerWidth()).toInt();
    if (width > 140 && width < 200) {
        $("#case_edit_header_col5 .col-xl-6").addClass("mw-100"); //
        $("#case_edit_header_col5 .row").removeClass("mt-xl-3");
    }
    else {
        $("#case_edit_header_col5 .col-xl-6").removeClass("mw-100");
        $("#case_edit_header_col5 .row").addClass("mt-xl-3");
    }
}

function ScheduleCheckLoad() {
    $("#hdnDisableLoader").val('1');
    if ($('#is_super_admin').val() == "SuperAdmin") {
        $.ajax({
            type: "GET",
            url: "/Schedule/CheckSchedulePublishFlag",
            success: function (response) {
                if (response == true) {
                    $('#ScheduleMId').addClass('Un-publish-sch');
                }
                else {
                    $('#ScheduleMId').removeClass('Un-publish-sch');
                }
            }
        });
    }
}
function checkUserLoggedInStatus() {
    $.ajax({
        cache: false,
        type: "GET",
        url: "/Account/CheckLogoutUser",
        success: function (response) {
            try {
                if (response.Status == true) {
                    window.location.href = "/Account/Signout"
                }
            }
            catch (e) {

            }
        }
    });
}

function playMsgNotification() {
    try {
        var x = document.getElementById('new_msg_notification');
        x.play();
    }
    catch (err) { console.log(err); }
}

function playBlastNotification() {
    try {
        var x = document.getElementById('new_blast_notification');
        x.play();
    }
    catch (err) { console.log(err); }
}

