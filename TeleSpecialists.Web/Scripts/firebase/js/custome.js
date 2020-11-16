var StatusArray = ['Open', 'snooze', 'Accept', 'Reject', 'closeNavigatorCase', 'refreshPhyStatus', 'syncCaseInfo', 'InternalBlast', 'ExternalBlast', 'acceptBlast'];
function snoozePoupReloadCode_edit() {
    showPhysicianStatusSnoozePopup_def_edit(3, true);
}
showPhysicianStatusSnoozePopup_def_edit = function (id, setCookie) {
    var url = "/Physician/StatusSnoozePopup";
    $.get(url, { phs_key: id }, function (response) {
        $("#divModelLessPopup").empty().html(response.data);
        // $("#divModelLessPopup").modal("show");
        showKendoPopup();
        //debugger;
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
function SendStrokeToPhysician(id, setCookie) {
    StartReadStrokeStatus();
    var url = "/Physician/ShowNewCasePopup?rnd=" + Math.floor(Math.random() * 10000) + 1;
    $.get(url, { id: id }, function (response) {
        $("#divCaseAssignPopup").empty().html(response);
        $("#divCaseAssignPopup").modal("show");

        playNewCaseNotification();

        bindEventsForshowPhysicianNewCasePopup_def();
        //showKendoPopup();
        if (setCookie || setCookie === "true") {
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
                try {
                    document.getElementById('new_case_notification').pause();
                }
                catch (err) {
                }
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
                try {
                    document.getElementById('new_case_notification').pause();
                }
                catch (err) {
                }
                deleteCookie("NewCasePopup");
                window.clearTimeout(newCasePopuptimeoutId);
                newCasePopuptimeoutId = null;
            }, autoCloseTime);
        }
    });
}
// stroke read status
var readInterval;
function StartReadStrokeStatus() {
    readInterval =  setInterval(function () {
        let strokeAlertStatus = localStorage.getItem("strokePopStatus");//$('#lbl_show_strokeAlert').val();
        console.log('status:' + strokeAlertStatus);
        if (strokeAlertStatus === 'true')
            hideStrokeAlertPopUp();
    }, 800);
}
function hideStrokeAlertPopUp() {
    try {
        setTimeout(function () {
            localStorage.setItem("strokePopStatus", "false");
            clearInterval(readInterval);
        }, 1500);
        console.log('divCaseAssignPopup going to be hide');
        $("#divCaseAssignPopup").modal("hide");
        document.getElementById('new_case_notification').pause();
    }
    catch (err) {
        console.log(err);
    }
}
// read status end
function hidePopAcceptReject(id) {
    try {
        document.getElementById('new_case_notification').pause();
    }
    catch (err) {
    }
    $("#divCaseAssignPopup").modal("hide");
    stopBlastInterval(id);
}
function SendStrokeInternalBlast(id, setCookie) {
    var url = "/Physician/ShowNewCasePopupInternalBlast?rnd=" + Math.floor(Math.random() * 10000) + 1;
    $.get(url, { id: id }, function (response) {
        $("#divCaseAssignPopup").empty().html(response);
        $("#divCaseAssignPopup").modal("show");

        //playNewCaseNotification();

        bindEventsForshowPhysicianNewCasePopup_def_InternalBlast();
        //showKendoPopup();
        if (setCookie || setCookie === "true") {
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
                try {
                    document.getElementById('new_case_notification').pause();
                }
                catch (err) {
                }
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
                try {
                    document.getElementById('new_case_notification').pause();
                }
                catch (err) {
                }
                deleteCookie("NewCasePopup");
                window.clearTimeout(newCasePopuptimeoutId);
                newCasePopuptimeoutId = null;
            }, autoCloseTime);
        }
    });
}
//98811
function ShowBlastStrokeAlert(id, setCookie, caseObjStr, blastType, strokeStamp) {
    var loggedUserid = $('#hdnUser').val().toString(); //'95f11773-4161-4fa6-8e6d-9c381101c2d9'; //
    console.log('logged in user id is : ' + loggedUserid);

    var status;
    var caseObj = JSON.parse(caseObjStr);
    for (var i = 0; i < caseObj.length; i++) {
        var record = caseObj[i];
        var result = caseObj[i].substring(1, caseObj[i].length - 1);
        var isfound = result.search(loggedUserid);
       
        status = isfound;
        //userArr = result.split(',');
    }
    if (status === -1) {
        console.log('Physician not found! stroke cant be send');
    }
    else {
        $('#cas_key_blast').val(id);
        startBlastInterval(id, blastType, strokeStamp);
    }
        
}


function CallFromFirebaseSW() {
    let loggedUserid = $('#hdnUser').val();
    console.log('you came from SW ; ', loggedUserid);
}
    
