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
var listener = new BroadcastChannel('listener');
listener.onmessage = function (e) {
    //console.log('Got message from service worker', e);
    var _payload = e.data.data;
    let caseId = _payload['caseId'];
    let caseType = _payload['caseType'];
    console.log('Got message from service worker id is' + caseId);
    console.log('Got message from service worker id is' + caseType);
    let jsonData = _payload['jsonData'];
    let action = _payload['action'];
    let objectData = _payload['objectData'];
    console.log('case type is : ' + caseType);
    if (caseType === StatusArray[0])
        SendStrokeToPhysician(caseId, true);
    else if (caseType === StatusArray[1])
        snoozePoupReloadCode_edit();
    else if (caseType === StatusArray[2])
        showNavigatorAcceptCasePopupWithNoQueue_def(jsonData);
    else if (caseType === StatusArray[3])
        showNavigatorRejectCasePopupWithNoQueue_def(caseId, action);
    else if (caseType === StatusArray[4])
        closeNavigatorCasePopupWithNoQueue_def();
    else if (caseType === StatusArray[5])
        refreshCurrentPhyStatus();
    else if (caseType === StatusArray[6])
        syncCaseInfoFromAdmin_def(objectData);
    else if (caseType === StatusArray[7])
        ShowBlastStrokeAlert(caseId, true, objectData, 'INTERNAL BLAST');
    else if (caseType === StatusArray[8])
        ShowBlastStrokeAlert(caseId, true, objectData, 'EXTERNAL BLAST');
    else if (caseType === StatusArray[9])
        LogoutOtherSystems(objectData); // we have to call function for twofactor
};

var isCaseFound = $('#lblCaseId').val();
if (isCaseFound)
    alert(isCaseFound);


firebase.messaging().onMessage(function (payload) {
   // debugger;
    console.log("Message received. ", payload);
    // get record from data node 
    $('#imgForBlast').hide();
    let arr_data = payload.data;
    let caseId = arr_data['caseId'];
    let caseType = arr_data['caseType'];
    let jsonData = arr_data['jsonData'];
    let action = arr_data['action'];
    let objectData = arr_data['objectData'];
    console.log('case type is : ' + caseType);
    if (caseType === StatusArray[0])
        SendStrokeToPhysician(caseId, true);
    else if (caseType === StatusArray[1])
        snoozePoupReloadCode_edit();
    else if (caseType === StatusArray[2]) {
        
        hidePopAcceptReject();
        showNavigatorAcceptCasePopupWithNoQueue_def(jsonData);
    }
    else if (caseType === StatusArray[3]) {
        hidePopAcceptReject();
        showNavigatorRejectCasePopupWithNoQueue_def(caseId, action);
    }
    else if (caseType === StatusArray[4])
        closeNavigatorCasePopupWithNoQueue_def();
    else if (caseType === StatusArray[5])
        refreshCurrentPhyStatus();
    else if (caseType === StatusArray[6])
        syncCaseInfoFromAdmin_def(objectData);
    else if (caseType === StatusArray[7])
        ShowBlastStrokeAlert(caseId, true, objectData, 'INTERNAL BLAST');
    else if (caseType === StatusArray[8])
        ShowBlastStrokeAlert(caseId, true, objectData, 'EXTERNAL BLAST');
    else if (caseType === StatusArray[9])
        LogoutOtherSystems(objectData); // we have to call function for twofactor
    
    
    //SendStrokeInternalBlast(caseId, true);
    

    // get Record from notification  node by husnain
    //let arr_nf = payload.notification;
    //let title = arr_nf['title'];
    //let body = arr_nf['body'];
    //let img = arr_nf['image'];
    //let div = "<div><h2>" + title + "</h2><strong>" + body + "</strong><br><img style='height:10%;width:20%' src='" + img + "'/></div>";
    //$("#divData").html('');
    //$("#divData").append(div);
    //appendMessage(payload);
    //console.log('found notification title is : ' + title);
    //for (let key in payload.notification) {
    //    console.log('NF value is : ' + key);
    //}
    //let body = arr_nf['body'];
    //let img = arr_nf['image'];
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

function startBlastInterval(blastType) {
    //$('#imgForBlast').show();
    //blastInterval = setInterval(AnimateImg, 500);
    $('#lblBlast').html(blastType);
    $('#btnBlast').show();
}
function stopBlastInterval() {
    //clearInterval(blastInterval);
    //$('#imgForBlast').hide();
    //blastInterval = null;
    $('#lblBlast').html('');
    $('#btnBlast').hide();
}

$('#btnBlast').click(function () {
    stopBlastInterval();
    var id = $('#cas_key_blast').val();
    SendStrokeInternalBlast(id, true);
});

