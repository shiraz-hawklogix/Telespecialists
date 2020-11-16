var timeoutId = null;
var snoozetimeoutId = null;
var newCasePopuptimeoutId = null;

showPhysicianStatusSnoozePopup_def = function (id, setCookie) {
    var url = "/Physician/StatusSnoozePopup?rnd=" + Math.floor(Math.random() * 10000) + 1;
    $.get(url, { phs_key: id }, function (response) {
        $("#divModelLessPopup").empty().html(response.data);
        // $("#divModelLessPopup").modal("show");
        showKendoPopup();
        var isSetCookie = true;
        try {
            isSetCookie = setCookie;
        }
        catch (e) {
            isSetCookie = true;
        }

        if (isSetCookie) {
            var dt = new Date();
            dt.setMinutes(dt.getMinutes() + 2);
            var snoozeData = {
                status: id,
                user: response.userId,
                expiryDate: dt
            };
            createCookie("SnoozePopup", JSON.stringify(snoozeData), dt);
            snoozetimeoutId = window.setTimeout(function () {
                $("#divModelLessPopup").data("kendoWindow").close();
                deleteCookie("SnoozePopup");
                window.clearTimeout(snoozetimeoutId);
                snoozetimeoutId = null;
            }, 120000);
        }
        else {
            var data = JSON.parse(getCookie("SnoozePopup"));

            var autoCloseTime = new Date(data.expiryDate) - new Date();
            snoozetimeoutId = window.setTimeout(function () {
                $("#divModelLessPopup").data("kendoWindow").close();
                deleteCookie("SnoozePopup");
                window.clearTimeout(snoozetimeoutId);
                snoozetimeoutId = null;
            }, autoCloseTime);
        }
    });
}

reloadPageForUser_def = function (browserAgent) {
    var currentBrowser = window.navigator.userAgent.toLowerCase();
    if (currentBrowser == browserAgent) {
        window.setTimeout(function () {
            window.location.href = '/Home/Index';
        }, 4000);
    }
}

showPhysicianNewCasePopup_def = function (id, setCookie) {
    var url = "/Physician/ShowNewCasePopup?rnd=" + Math.floor(Math.random() * 10000) + 1;
    $.get(url, { id: id }, function (response) {
        $("#divCaseAssignPopup").empty().html(response);
        $("#divCaseAssignPopup").modal("show");

        playNewCaseNotification();

        bindEventsForshowPhysicianNewCasePopup_def();
        //showKendoPopup();
        if (setCookie || setCookie == "true") {
            var dt = new Date();
            dt.setMinutes(dt.getMinutes() + 2);
            var popupData = {
                // status: id,
                user: response.userId,
                expiryDate: dt
            };
            createCookie("NewCasePopup", JSON.stringify(popupData), dt);
            newCasePopuptimeoutId = window.setTimeout(function () {
                $("#divCaseAssignPopup").modal("hide");
                document.getElementById('new_case_notification').pause();
                deleteCookie("NewCasePopup");
                window.clearTimeout(newCasePopuptimeoutId);
                newCasePopuptimeoutId = null;
                rejectCaseWithNoQueue(true);
            }, 120000);
        }
        else {
            var data = JSON.parse(getCookie("NewCasePopup"));

            var autoCloseTime = new Date(data.expiryDate) - new Date();
            newCasePopuptimeoutId = window.setTimeout(function () {
                $("#divCaseAssignPopup").modal("hide");
                document.getElementById('new_case_notification').pause();
                deleteCookie("NewCasePopup");
                window.clearTimeout(newCasePopuptimeoutId);
                newCasePopuptimeoutId = null;
            }, autoCloseTime);
        }
    });
}

syncCaseInfoFromAdmin_def = function (caseObjStr) {
    var caseObj = JSON.parse(caseObjStr);

    var hasCaseFormOpen = $("#createForm").find("#cas_key").val() == caseObj.cas_key;
    if (hasCaseFormOpen) {
        if ($('#cas_cst_key').data('kendoDropDownList') != undefined)
            $('#cas_cst_key').data('kendoDropDownList').value(caseObj.cas_cst_key);

        $('#cas_response_first_atempt').val(caseObj.cas_response_first_atempt);
        if (caseObj.IsLoginDelayRequired) {
            $("#cas_metric_notes").attr("data-is-required", "true");
            $("#cas_metric_notes_label").addClass("text-danger");
        }
        else {
            $("#cas_metric_notes").removeAttr("data-is-required");
            $("#cas_metric_notes_label").removeClass("text-danger");
        }

        if ($('#cas_response_first_atempt').data('kendoTimePicker') != undefined)
            $('#cas_response_first_atempt').data('kendoTimePicker').readonly(caseObj.cas_phy_has_technical_issue);

    }

}

showNavigatorAcceptCasePopupWithNoQueue_def = function (caseObjStr) {
    var caseObj = JSON.parse(caseObjStr);

    var hasCaseFormOpen = $("#createForm").find("#cas_key").val() == caseObj.cas_key;
    if (hasCaseFormOpen) {
        var caseStatusDropDown = $('#cas_cst_key').data('kendoDropDownList');
        if (caseStatusDropDown != undefined)
            caseStatusDropDown.value(caseObj.cas_cst_key);

        phyDropDown = $('#cas_phy_key').data('kendoDropDownList');
        if (phyDropDown != undefined)
            phyDropDown.value(caseObj.cas_phy_key);

        $('#cas_response_time_physician').val(caseObj.cas_response_time_physician);
        $("#assignmentTime").html("<strong> Waiting to Accept to Accepted: </strong>" + caseObj.FromWaitingToAcceptToAcceptTime);
        $("#btnSaveSend").disable();
    }

    $("#divCaseAssignPopup").empty().showAlertPopUp("Confirm", "Case #" + caseObj.cas_case_number + " has been accepted", function () { $("#divCaseAssignPopup").modal("hide"); });
}

function rejectCaseWithNoQueue(hasExpired) {
    window.clearTimeout(newCasePopuptimeoutId); // clearing the timeout so the reject is not called.
    var cas_key = $.trim($("#newcasePopupAlert").find("#cas_key").val()).toInt();
    if (cas_key == 0)
        return;

    document.getElementById('new_case_notification').pause();

    $.get('/Case/RejectCaseWithNoQueue', { id: cas_key, hasExpired: hasExpired }, function (response) {
        $("#newcasePopupAlert").find("#cas_key").val("");
        $("#divCaseAssignPopup").modal("hide");
    });
}

function closeNavigatorCasePopupWithNoQueue_def() {

    window.clearTimeout(newCasePopuptimeoutId); // clearing the timeout so the reject is not called.    
   
    document.getElementById('new_case_notification').pause();
    $("#newcasePopupAlert").find("#cas_key").val("");
    $("#divCaseAssignPopup").modal("hide");
}

showNavigatorRejectCasePopupWithNoQueue_def = function (id, action) {    
    var url = "/Case/_CasePopupAlertNavigatorForManualAssign";
    $.post(url, { id: id, action: action }, function (response) {
        window.setTimeout(function () {
            $("#divCaseAssignPopup").empty().html(response);
            $("#divCaseAssignPopup").modal("show");


            var hasCaseFormOpen = $("#createForm").find("#cas_key").val() == id;
            if (hasCaseFormOpen) {
                var physicianDropDown = $('#cas_phy_key').data('kendoDropDownList');
                if (physicianDropDown != null)
                    physicianDropDown.value("");

                var statusDropDown = $('#cas_cst_key').data('kendoDropDownList');
                if (statusDropDown != undefined)
                    statusDropDown.value(caseStatusEnum.Open);
                $("#assignmentTime").html("<strong>Waiting to Accept to Accepted: </strong> 00:00:00");
                $("#btnSaveSend").disable();
            }
        }, 5000);

    });
}


function bindEventsForshowPhysicianNewCasePopup_def() {
    // Event Registration for Popup
    $("#newcasePopupAlert").find("#btnRejectNewCasePopup,#btnClose").off("click").click(function () {
        rejectCaseWithNoQueue(false);
    });

    $("#btnAcceptNewCasePopup").off("click").click(function () {
        var cas_key = $("#newcasePopupAlert").find("#cas_key").val();
        document.getElementById('new_case_notification').pause();
        $("#newcasePopupAlert").find("#cas_key").val("")
        window.clearTimeout(timeoutId); // clearing the timeout so the reject is not called.
        $.post('/Case/AcceptCaseWithNoQueue', { id: cas_key }, function (response) {
            $("#divCaseAssignPopup").modal("hide");
            if (response.success) {
                $("#validationSummary").empty().showBSSuccessAlert("Success: ", response.message);
                //$("#drp_physican_status").val(response.phs_key);
                refreshCurrentPhyStatus();
                //changeStatus();
               // loadPageAsync('/Case/Index');
            }
            else {
                if (response.showInfoPopup)
                    $("#validationSummary").empty().showBSInfoAlert("", response.message);
                else
                    $("#validationSummary").empty().showBSDangerAlert("", response.message);
            }
        })
    });
}

function snoozePoupReloadCode() {
    if ($("#divModelLessPopup").data("reload") == "1") {
        var snoozeJson = getCookie("SnoozePopup");
        if (snoozeJson != "") {
            window.setTimeout(function () {
                var data = JSON.parse(snoozeJson);

                if (data.user == $("#hdnUser").val())
                    showPhysicianStatusSnoozePopup_def(data.status, false);
            }, 1000);
        }

        $("#divModelLessPopup").data("reload", "0");
    }
}

function showKendoPopup() {
    var myWindow = $("#divModelLessPopup");

    myWindow.kendoWindow({
        width: "710",
        title: "Snooze",
        visible: false,
        actions: [
            "Minimize",
        ]
    }).data("kendoWindow").center().open();
}

function playNewCaseNotification() {
    document.getElementById('new_case_notification').muted = false;
    document.getElementById('new_case_notification').volume = 1;
    document.getElementById('new_case_notification').play();
}

