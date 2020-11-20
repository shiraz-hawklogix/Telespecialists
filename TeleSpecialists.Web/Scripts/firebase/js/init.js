// config wetting of telenotification db
//var firebaseConfig = {
//    apiKey: "AIzaSyBn7zsblu7K4SdNc48pcGXkGaUWLW7rWIo",
//    authDomain: "telenotification-a48ca.firebaseapp.com",
//    databaseURL: "https://telenotification-a48ca.firebaseio.com",
//    projectId: "telenotification-a48ca",
//    storageBucket: "telenotification-a48ca.appspot.com",
//    messagingSenderId: "694692088545",
//    appId: "1:694692088545:web:b488ac266dc3875ab945a0",
//    measurementId: "G-SBF832LGH6"
//};
// config wetting of teleCare db

var blastInterval; 
var checkBlastArr = [];
var animated = false;
var listener = new BroadcastChannel('listener');
listener.onmessage = function (e) {
    //console.log('Got message from service worker', e);
    let loggedUser = $('#hdnUser').val();
    var _payload = e.data.data;
    let physician_key = _payload['phy_key'];
    let caseId = _payload['caseId'];
    let caseType = _payload['caseType'];
    console.log('Got message from service worker id is' + caseId);
    console.log('Got message from service worker id is' + caseType);
    let jsonData = _payload['jsonData'];
    let action = _payload['action'];
    let objectData = _payload['objectData'];
    let strokeStamp = _payload['strokeStamp'];
    console.log('case type is : ' + caseType);
    if (caseType === StatusArray[0]) {
        if (loggedUser === physician_key)
            SendStrokeToPhysician(caseId, true);
    }
    else if (caseType === StatusArray[1])
        snoozePoupReloadCode_edit();
    else if (caseType === StatusArray[2]) {
        hidePopAcceptReject(caseId);
        if (loggedUser === physician_key)
            showNavigatorAcceptCasePopupWithNoQueue_def(jsonData);
    }
    else if (caseType === StatusArray[3]) {
        hidePopAcceptReject(caseId);
        if (loggedUser === physician_key)
            showNavigatorRejectCasePopupWithNoQueue_def(caseId, action);
    }
    else if (caseType === StatusArray[4])
        closeNavigatorCasePopupWithNoQueue_def();
    else if (caseType === StatusArray[5])
        refreshCurrentPhyStatus();
    else if (caseType === StatusArray[6])
        syncCaseInfoFromAdmin_def(objectData);
    else if (caseType === StatusArray[7])
        ShowBlastStrokeAlert(caseId, true, objectData, 'INTERNAL BLAST', strokeStamp);
    else if (caseType === StatusArray[8])
        ShowBlastStrokeAlert(caseId, true, objectData, 'EXTERNAL BLAST', strokeStamp);
    else if (caseType === StatusArray[9])
        stopBlastInterval(caseId);
};

var isCaseFound = $('#lblCaseId').val();
if (isCaseFound)
    alert(isCaseFound);


firebase.messaging().onMessage(function (payload) {
   // debugger;
    console.log("Message received. ", payload);
    let loggedUser = $('#hdnUser').val();
    // get record from data node 
    $('#imgForBlast').hide();
    let arr_data = payload.data;
    let physician_key = arr_data['phy_key'];
    let caseId = arr_data['caseId'];
    let caseType = arr_data['caseType'];
    let jsonData = arr_data['jsonData'];
    let action = arr_data['action'];
    let objectData = arr_data['objectData'];
    let strokeStamp = arr_data['strokeStamp'];

    console.log('case type is : ' + caseType);
    if (caseType === StatusArray[0]) {
        if (loggedUser === physician_key)
            SendStrokeToPhysician(caseId, true);
    }
    else if (caseType === StatusArray[1])
        snoozePoupReloadCode_edit();
    else if (caseType === StatusArray[2]) {
        hidePopAcceptReject(caseId);
        if (loggedUser === physician_key)
            showNavigatorAcceptCasePopupWithNoQueue_def(jsonData);
    }
    else if (caseType === StatusArray[3]) {
        hidePopAcceptReject(caseId);
        if (loggedUser === physician_key)
            showNavigatorRejectCasePopupWithNoQueue_def(caseId, action);
    }
    else if (caseType === StatusArray[4])
        closeNavigatorCasePopupWithNoQueue_def();
    else if (caseType === StatusArray[5])
        refreshCurrentPhyStatus();
    else if (caseType === StatusArray[6])
        syncCaseInfoFromAdmin_def(objectData);
    else if (caseType === StatusArray[7])
        ShowBlastStrokeAlert(caseId, true, objectData, 'INTERNAL BLAST', strokeStamp);
    else if (caseType === StatusArray[8])
        ShowBlastStrokeAlert(caseId, true, objectData, 'EXTERNAL BLAST', strokeStamp);
    else if (caseType === StatusArray[9])
        stopBlastInterval(caseId);
    
});

function appendMessage(payload) {
    const messagesElement = document.querySelector('#messages');
    const dataHeaderELement = document.createElement('h5');
    const dataElement = document.createElement('pre');
    dataElement.style = 'overflow-x:hidden;';
    dataHeaderELement.textContent = 'Received message:';
    dataElement.textContent = JSON.stringify(payload, null, 2);
    messagesElement.appendChild(dataHeaderELement);
    messagesElement.appendChild(dataElement);
}

if ('serviceWorker' in navigator) {
    window.addEventListener('load', function () {
        navigator.serviceWorker.register('/firebase-messaging-sw.js').then(function (registration) {
            //Registration was successful
            console.log('ServiceWorker registration successful with scope: ', registration.scope);
        }, function (err) {
            //registration failed :(
            console.log('ServiceWorker registration failed: ', err);
        });
    });
}


////// blast icon code

function AnimateImg() {
    $('#imgForBlast').toggle(500);
}

function CheckFirebaseUser() {
    var userStatus = $('#lblforfirebase').val();
    if (userStatus === 'get') {
        alert('found get');
        CallView();
    }
}

$('#imgForBlast').click(function () {
    stopBlastInterval();
    $('#imgForBlast').hide();
    var id = $('#cas_key_blast').val();
    SendStrokeInternalBlast(id, true);
});

function startBlastInterval(id, blastType, strokeStamp) {
    var arraycontainBlast = (checkBlastArr.indexOf(id) > -1);
    console.log('arraycontainBlast result : ' + arraycontainBlast);
    if (!arraycontainBlast) {
        checkBlastArr.push(id);
        var divBlast = "<a href='javascript:void(0);' id='" + id + "'   onclick='GetBlastDetail(" + id + ");' data-strokeStamp='" + strokeStamp+"' > <span class='ml6 font_12px'><span class='text-wrapper'> <span class='letters' id='lblBlast" + id + "'>" + blastType + "</span></span> </span> </a>";
        $('#divInternalExternal').append(divBlast);
        if (!animated)
            AnimateJS();
        let muteStatus = localStorage.getItem('muteStatus');
        if (muteStatus === 'false')
            playBlastNotification();
        else
            console.log('tune muted');
        
        localStorage.setItem("activeBlastIds", checkBlastArr);
        localStorage.setItem("activeBlasts", $('#divInternalExternal').html());
    }
}
function stopBlastInterval(id) {
    console.log('blast going to be hide :', id);
    $('#lblBlast' + id).html('');
    $('#' + id).remove();
    localStorage.setItem("activeBlasts", $('#divInternalExternal').html());
}

$('.btnBlast').click(function () {
    stopBlastInterval();
    var id = $('#cas_key_blast').val();
    SendStrokeInternalBlast(id, true);
});

function GetBlastDetail(e) {
   // var _strokeStamp = //$(this).attr('data-strokeStamp');
    console.log('open stamp: ', e);
    //stopBlastInterval(e);
    SendStrokeInternalBlast(e, true);
}

/*function AnimateJS() {
    animated = true;
    var textWrapper = document.querySelector('.ml6 .letters');
    textWrapper.innerHTML = textWrapper.textContent.replace(/\S/g, "<span class='letter'>$&</span>");

    anime.timeline({ loop: true })
        .add({
            targets: '.ml6 .letter',
            translateY: ["1.1em", 0],
            translateZ: 0,
            duration: 500,
            delay: (el, i) => 50 * i
        }).add({
            targets: '.ml6',
            opacity: 0,
            duration: 500,
            easing: "easeOutExpo",
            delay: 500
        });
}*/

$(document).ready(function () {
    var _arrVal = localStorage.getItem('activeBlastIds');
    if (_arrVal)
        checkBlastArr = _arrVal.slice();
    console.log('checkBlastArr:', checkBlastArr);
});
