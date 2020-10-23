
var popupHub = null;
$(document).ready(function () {
    // Declare a proxy to reference the hub.
    popupHub = $.connection.physicianCasePopupHub;
    logToConsole('page loaded ' + new Date().toString() + "\n");



    // message handler
    popupHub.client.showPhysicianCasePopup = function (id) {
        var url = "/Case/_CasePopupAlertPhysician";
        $.get(url, { id: id }, function (response) {
            $("#divModelPopUp").empty().html(response);
            $("#divModelPopUp").modal("show");
            timeoutId = setTimeout(function () { rejectCase(); }, 120000);

            // Event Registration for Popup
            $("#casePopupAlert").find("#btnReject,#btnClose").off("click").click(function () {
                rejectCase();
            });

            $("#btnAccept").off("click").click(function () {
                var cas_key = $("#casePopupAlert").find("#cas_key").val();
                $("#casePopupAlert").find("#cas_key").val("")
                window.clearTimeout(timeoutId); // clearing the timeout so the reject is not called.
                $.post('/Case/AcceptCase', { id: cas_key }, function (response) {
                    $("#divModelPopUp").modal("hide");
                    popupHub.server.closePopupForCurrentUser();
                    if (response.success) {
                        $("#validationSummary").empty().showBSSuccessAlert("Success: ", response.message);
                        //$("#drp_physican_status").val(response.phs_key);
                        refreshCurrentPhyStatus();
                        //changeStatus();
                        loadPageAsync('/Case/Index');
                    }
                    else {
                        if (response.showInfoPopup)
                            $("#validationSummary").empty().showBSInfoAlert("", response.message);
                        else
                            $("#validationSummary").empty().showBSDangerAlert("", response.message);
                    }
                })
            });
        });
    }

    popupHub.client.showPhysicianNewCasePopup = function (id) {
        showPhysicianNewCasePopup_def(id, true);
    }

    popupHub.client.showNavigatorCasePopup = function (id) {
        var url = "/Case/_CasePopupAlertNavigator";
        $.get(url, { id: id }, function (response) {
            $("#divModelPopUp").empty().html(response);
            $("#divModelPopUp").modal("show");
        });
    }

    popupHub.client.showNavigatorRejectCasePopupWithNoQueue = function (id) {
        showNavigatorRejectCasePopupWithNoQueue_def(id);
    }

    showNavigatorAcceptCasePopupWithNoQueue = function (caseObj) {
        showNavigatorAcceptCasePopupWithNoQueue_def(caseObj);
    }
    popupHub.client.showPhysicianStatusSnoozePopup = function (id, setCookie) { showPhysicianStatusSnoozePopup_def(id, setCookie);}

    popupHub.client.syncCaseInfoFromAdmin = function (caseObjStr) {
        syncCaseInfoFromAdmin_def(caseObjStr);
    } 

    popupHub.client.syncPhysicianStatusTime = function (resetTime) {    
        refreshCurrentPhyStatus(resetTime);
    }

    popupHub.client.syncCaseInfo = function (cas_key, cas_phy_key, cas_cst_key, cas_response_time_physician) {
        var hasCaseFormOpen = $("#createForm").find("#cas_key").val() == cas_key;
        if (hasCaseFormOpen) {
            $('#cas_cst_key').data('kendoDropDownList').value(cas_cst_key);
            $('#cas_phy_key').data('kendoDropDownList').value(cas_phy_key);
            $('#cas_response_time_physician').val(cas_response_time_physician);
        }
    }

    popupHub.client.closeNavigatorCasePopupWithNoQueue = function () {
        closeNavigatorCasePopupWithNoQueue_def();
    }

    popupHub.client.closeCasePopup = function () {
        $("#casePopupAlert").find("#cas_key").val("");
        $("#divModelPopUp").modal("hide");
    }
    //

    // method added not related to popup changes. however it is used when the remote login feature is used
    // so we are forcing the user to reload the page after remote login. 
    // adding it here because we have already track of connect users with application here. 

    popupHub.client.reloadPageForUser = function (browserAgent) {
        reloadPageForUser_def(browserAgent);

    }
    function rejectCase() {
        window.clearTimeout(timeoutId); // clearing the timeout so the reject is not called.
        var cas_key = $("#casePopupAlert").find("#cas_key").val();
        if (cas_key != "" && cas_key != undefined) {
            popupHub.server.rejectCase(cas_key);
            $("#casePopupAlert").find("#cas_key").val("");
            $("#divModelPopUp").modal("hide");
        }
    }




    // Start Hub
    $.connection.hub.start().done(function () {
        snoozePoupReloadCode();
        logToConsole('connection started' + new Date().toString() + "\n");
    });

    $.connection.hub.disconnected(function () {
        setTimeout(function () {
            try {
                $.connection.hub.start();
            }
            catch (err) {
                logToConsole(err);
            }
        }, 60000); // Restart connection after 60 seconds.
    });




    $.connection.hub.stateChanged(function (change) {
        if (change.newState === $.signalR.connectionState.connected) {

            logToConsole('connected' + new Date().toString() + "\n");
            connectToServer();
        }
    });

    function connectToServer() {
        if (!(popupHub.connection.state == $.signalR.connectionState.connected)) {
            popupHub.server.connect();
            if (connectToServer.timeout != undefined)
                window.clearTimeout(connectToServer.timeout);
        }
        connectToServer.timeout = window.setTimeout(function () {
            connectToServer();
        }, 10000)
    }

});


