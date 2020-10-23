
$(document).ready(function () {
    SubscribeToEvents();
});
//For Identifying Agent TFN or ACD Call.
var strCallQueueType = null;
var autoWorkflowCallLength = 0;
var aniNumber = "";
var five9Params = {};
var callAccepted = 0;

function DialNumber(number) {
    ClickToDialEventHandler(number);
}
function setAutowokflowCallLength(callLength) {
    autoWorkflowCallLength = callLength;
}

function logoutAgent() {
    Five9.ExtensionLib.logoutAgent('0');
}

function changeAgentState() {
    Five9.ExtensionLib.setAgentState(true);
}

function logoutAgentFromSC() {
    Five9.ExtensionLib.setAgentState(false);
    logoutAgent();
}

function HangUpCall() {
    Five9.ExtensionLib.dispositionCall('0');
    localStorage["InCall"] = false;
}

function SubscribeToEvents() {
    Five9.ExtensionLib.setListener(extensionListener);
}

function SaveAccountHistory(message, comment, callmetadata, contactId) {


}

function SearchAccount(phoneNumber) {

}

function CurrentAccountID() {

}

function ClickToDialEventHandler(num) {

    var number = num;
    number = number.replace(/[^+0-9]/g, '');
    var campaign = 'OB - Manual Call';

    if (!!number) {

        var crmC2DInfo = {
            clickToDialNumber: number,
            screenpopC2DSearch: false,
            // Five9 campaign name
            preselectedCampaignName: campaign,
            crmObject: {
                // TODO include CRM-specific contact details
                id: '',
                name: 'Individual',
                type: 'Contact',
                label: 'Contact',
                isWho: true,
                isWhat: false
            }
        };
        Five9.ExtensionLib.click2dial(crmC2DInfo);
        logToConsole('click2dial cmd sent');
    } else {
        logToConsole('Phone Number required!');
    }
}

/**
* MyCallsToday call count
*/
var callCount = 0;

var extensionListener = {

    bringAppToFront: function (params) {
        setLastEvent('ExtensionLib.bringAppToFront');
        /*
         The bringAppToFront method should attempt to foreground the application.
        */
    },

    search: function (params) {
        setLastEvent('ExtensionLib.search' + JSON.stringify(params));
        var searchResponse = {
            crmObjects: [
                {
                    id: '',
                    type: 'Contact',
                    label: 'Contact',
                    name: 'Customer',
                    isWho: true,
                    isWhat: false
                }
            ],
            screenPopObject: {
                id: '',
                type: 'Contact',
                label: 'Contact',
                name: 'Customer',
                isWho: true,
                isWhat: false
            }
        };
        return searchResponse;
    },

    saveCallLog: function (params) {
        setLastEvent('ExtensionLib.saveCallLog' + JSON.stringify(params));
        if (params.callType.toLowerCase() == "inbound") {
            UpdateCallLog(params);
        }
    },

    screenPop: function (params) {

        setLastEvent('ExtensionLib.screenPop' + JSON.stringify(params));

        /*
         The screenPop method should display the specific CRM record to the agent.
         This is the same screenPopObject that was returned in the search method.
         */

        return 'ok';
    },

    getTodayCallsCount: function (params) {
        setLastEvent('ExtensionLib.getTodayCallsCount');
        return callCount;
    },

    openMyCallsToday: function (params) {
        setLastEvent('ExtensionLib.openMyCallsToday');

        /*
         Open ‘My Calls Today’ report in CRM system
         */
    },

    enableClickToDial: function (params) {
        setLastEvent('ExtensionLib.enableClickToDial');
        //$('#clicktodial').prop('disabled', false);
    },

    disableClickToDial: function (params) {
        setLastEvent('ExtensionLib.disableClickToDial');
        //$('#clicktodial').prop('disabled', true);
    },

    adapterStarted: function (params) {
        setLastEvent('ExtensionLib.adapterStarted');
    },

    agentStateChanged: function (params) {
        if ((params.loginState == 'WORKING' || params.currentStateTime > 1) && localStorage['PhoneBarLoggedIn'] != "true") {
            localStorage['PhoneBarLoggedIn'] = true;

        }
        else if (params.loginState == 'LOGOUT') {
            logToConsole("LOGOUT");
            localStorage['PhoneBarLoggedIn'] = false;

        }

        setLastEvent('ExtensionLib.agentStateChanged: ' + JSON.stringify(params));
    },

    callsChanged: function (params) {
        setLastEvent('ExtensionLib.callsChanged: ' + JSON.stringify(params));


        switch (params.state) {

            case 'OFFERED':
                {

                    /*when call comes in*/
                    if (params.callType.toLowerCase() == "inbound") {
                        callAccepted = 0;
                        setLastEvent('ExtensionLib.calloffered: ' + JSON.stringify(params));
                        CreateCallLog(params);//Create initial log entry
                    }

                    break;
                }
            case 'TALKING':
                {
                    /*when call is accepted*/
                    if (params.callType.toLowerCase() == "inbound") {

                        var ani = params.ani;
                        var dnis = params.dnis;
                        var callId = params.variables[42];
                        var campaignId = params.campaignId;
                        var timeStamp = params.startTimestamp;
                        var caseType = params.ivrTransferModule;

                        var url = "/case/create?ani=" + ani + "&dnis=" + dnis + "&call_id=" + callId + "&campaign=" + campaignId + "&start_timestamp=" + timeStamp + "&case_type=" + caseType;

                        var target = "divContentArea"
                        if (callAccepted == 0) {
                            callAccepted = 1;
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
                    }
                    break;
                }
        }
    },

    stationStateChanged: function (params) {
        setLastEvent('ExtensionLib.stationStateChanged: ' + JSON.stringify(params));
    }
};

//function PopCreateCase(params) {
//    var ani = params.ani;
//    var dnis = params.dnis;
//    var callId = params.variables[42];
//    var campaignId = params.campaignId;
//    var timeStamp = params.startTimestamp;

//    var req = {};
//    req.ani = ani;
//    req.dnis = dnis;
//    req.call_id = callId;
//    req.campaign = campaignId;
//    req.start_timestamp = timeStamp;

//    var url = "/case/create?";

//    var target = "divContentArea"

//    if ($('#createForm').length > 0) {
//        $("#IsAutoSave").val("1");
//        $("#createForm").submit();
//    }

//    $.get(url + $.param(req), function (response) {

//        $("#" + target).empty().html(response + $("#hdnJQueryValidations").val());
//        onReadyCallBack();
//    });
//}

function CreateCallLog(params) {


    var url = "/case/CreateCallLog";

    var data = {};
    data.ANI = params.ani;
    data.DNIS = params.dnis;
    data.TimeStamp = params.startTimestamp;
    data.CallId = params.variables[42];
    data.CampaignId = params.campaignId;
    data.Customer = params.customer;

    $.ajax({
        type: "POST",
        url: url,
        dataType: "json",
        data: JSON.stringify(data),
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            logToConsole(data);
        },
        error: function (error) {
            logToConsole(error);
        }
    });
}

function UpdateCallLog(params) {


    var url = "/case/UpdateCallLog";

    $.ajax({
        type: "POST",
        url: url,
        dataType: "json",
        data: JSON.stringify(params),
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            logToConsole(data);
        },
        error: function (error) {
            logToConsole(error);
        }
    });
}

function CaseScreenPop(param) {

}

var callStartTime;
var callEndTime;
var isCallconnected = false;
function resetCallStartTime() {
    callStartTime = null;
}
function getCallTime() {

}

function setLastEvent(msg) {
    logToConsole(msg);
}

function setAgentState(state) {

}

function getFormattedTime(milliseconds) {
    var result = "";
    var totalSeconds = Math.floor(milliseconds / 1000);
    if (totalSeconds < 0)
        totalSeconds = 0;

    var seconds = totalSeconds % 60;
    var minutes = Math.floor(totalSeconds / 60) % 60;
    var hours = Math.floor(totalSeconds / 3600);

    if (seconds < 10)
        seconds = "0" + seconds;

    if (minutes < 10)
        minutes = "0" + minutes;

    if (hours <= 0)
        return minutes + ":" + seconds;
    else
        return hours + ":" + minutes + ":" + seconds;
}

function getFormattedDate(dateString) {
    var result = "";

    var date = new Date(dateString);

    var day = date.getDate();
    var month = date.getMonth();
    var year = date.getFullYear();

    if (day < 10)
        day = "0" + day;

    if (month < 10)
        month = "0" + month;

    return month + "/" + day + "/" + year;
}
