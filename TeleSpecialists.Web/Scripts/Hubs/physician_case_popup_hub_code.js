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
        bindEventsForshowPhysicianNewCasePopup_def();
        playNewCaseNotification();
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
    //let reason = 'Physician is busy right now!';
    //SetRejectionWithReason(cas_key, reason);
    // commented to check dispatch code
    $.get('/Case/RejectCaseWithNoQueue', { id: cas_key, hasExpired: hasExpired }, function (response) {
        $("#newcasePopupAlert").find("#cas_key").val("");
        $("#divCaseAssignPopup").modal("hide");
        let reason = 'Physician is busy right now!';
        SetRejectionWithReason(cas_key, reason);
    });
}
// added by husnain

function SetRejectionWithReason(cas_key, reason) {
    $.ajax({
        type: 'POST',
        url: '/dispatch/RejectCase',
        data: { casKey: cas_key, caseRejectionType: reason },
        success: function (e) { },
        Error: function (e) { }
    });
}


function closeNavigatorCasePopupWithNoQueue_def() {

    window.clearTimeout(newCasePopuptimeoutId); // clearing the timeout so the reject is not called.    
    try {
        document.getElementById('new_case_notification').pause();
    }
    catch(err){ }
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
    $('#btnAcceptNewCasePopup').prop('disabled', false);
    // Event Registration for Popup
    $("#newcasePopupAlert").find("#btnRejectNewCasePopup,#btnClose").off("click").click(function () {
        localStorage.setItem("strokePopStatus", "true");
        let phy_name = $("#cas_phy_name").val();
        let cas_number = $("#cas_number").val();
        let facility_name = $('#cas_fac_name').val();
        let msg = 'Dr ' + phy_name + ' has been Rejected the Case # ' + cas_number + ', ' + facility_name;
        let physician = $("#cas_phy").val();
        AcceptCase(msg, physician);
        rejectCaseWithNoQueue(false);
    });

    $("#btnAcceptNewCasePopup").off("click").click(function () {
        localStorage.setItem("strokePopStatus", "true");
        $("#hdnDisableLoader").val('0');
        $('#btnAcceptNewCasePopup').prop('disabled', true);
        $("#divCaseAssignPopup").modal("hide");
        let phy_name = $("#cas_phy_name").val();
        let cas_number = $("#cas_number").val();
        let facility_name = $('#cas_fac_name').val();
        let msg = 'Stroke Alert from ' + facility_name + ' has been Accepted by ' + phy_name + ', Case# ' + cas_number;
        let physician = $("#cas_phy").val();
        AcceptCase(msg, physician);

        var cas_key = $("#newcasePopupAlert").find("#cas_key").val();
        document.getElementById('new_case_notification').pause();
        $("#newcasePopupAlert").find("#cas_key").val("");
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
    try {
        document.getElementById('new_case_notification').muted = false;
        document.getElementById('new_case_notification').volume = 1;
        document.getElementById('new_case_notification').play();
    }
    catch (err) { console.log(err); }
}


function bindEventsForshowPhysicianNewCasePopup_def_InternalBlast() {
    // Event Registration for Popup
    $('#btnAcceptNewCasePopup').prop('disabled', false);
    $("#btnAcceptBlast").off("click").click(function () {
        $("#hdnDisableLoader").val('0');
        $('#btnAcceptNewCasePopup').prop('disabled', true);
        //$("#divCaseAssignPopup").modal("hide");
        let phy_name = $("#cas_phy_name").val();
        let cas_number = $("#cas_number").val();
        let facility_name = $('#cas_fac_name').val();
        let accptedMsg = 'Stroke Alert from ' + facility_name + ' has been Accepted by ' + phy_name + ', Case# ' + cas_number;
        let physician = $("#cas_phy").val();
        var cas_key = $("#newcasePopupAlertBlast").find("#cas_key").val();
        let autoBlastStamp = $('#blastStamp').html();
        let grpName = phy_name + ' ' + 'SA';
       
        stopBlastInterval(cas_key);
       // alert(cas_key);
        $.ajax({
            type: 'POST',
            url: '/case/IsCaseRead',
            data: { id: cas_key },
            success: function (e) {
                if (e) {
                    console.log('read status is : ' + e);
                    $("#divCaseAssignPopup").modal("hide");
                    $("#validationSummary").empty().showBSInfoAlert("", "Case has been already assigned to another physician");
                }
                else {
                    // latest code for create auto stamp for blast
                    var _navigatorsArr = []; // load navigators in this array
                    $.ajax({
                        type: 'POST',
                        url: '/firebaseChat/GetUser',
                        data: { id: physician },
                        success: function (e) {
                            autoBlastStamp = autoBlastStamp.replaceAll("##NewLine##", "<br/>");
                            console.log('auto msg is : ' + autoBlastStamp);
                            _navigatorsArr.push({ user_id: e.fre_userId, email: e.fre_email, name: e.fre_firstname, firbaseuid: e.fre_firebase_uid, ImgPath: e.fre_profileimg });
                            _CreateGroupNewHub(grpName, 'Private', autoBlastStamp, physician, _navigatorsArr);
                            setTimeout(function () { AcceptCase(accptedMsg, physician); }, 3000);
                        },
                        Error: function (e) {
                        }
                    });
                    // code end
                    //AcceptCase(msg, physician); //  old code to create response stamp
                    
                    try {
                        document.getElementById('new_case_notification').pause();
                    }
                    catch (err) { console.log(err); }
                    $("#newcasePopupAlertBlast").find("#cas_key").val("");
                    window.clearTimeout(timeoutId); // clearing the timeout so the reject is not called.
                    $.post('/Case/AcceptCaseWithNoQueueInternalBlast', { id: cas_key }, function (response) {
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
                    });
                    
                }
            },
            Error: function (e) {
            }
        });
       
    });
}
function _CreateGroupNewHub(name, grptype, msg, physician, navArr) {
    if (physician) {
        GrpCreate(name, grptype, msg, physician, navArr);
    }
    else {
        CreatNewGrp(name, grptype, msg, physician, navArr);
    }
}