﻿
var getToken;

var SenderId;
var _senderName;
var _senderPhoto;
var _receiverId;
var _receiverName;
var _receiverPhoto;
var _teleid;
var _serverTime;
var teleUsersArr = [];
var isGroup;
var teleUsersForGrpArr = [];
var groupUsers = [];
var _userType;
var _GroupType;
var _isOnline;
var _allGroup = [];
var _userChatList = [];
var unreadMsg = [];
var unreadMessage = 0;
var totalNotifications = 0;
var _msgType;
var publicGrpArr = [];
var groupID;


//var messaging;
$(document).ready(function () {
    PrintToken(Get_Token());
});
async function Get_Token() {
    try {
        const messaging = firebase.messaging();
        await messaging.requestPermission();
        //messaging.usePublicVapidKey("BOvSRNGbyCBPiAc8eq9MGz-cPhzBOA4oxkOyHu6p-XW2cT4lWiqBbM7HVU6K9pRINCX-Nu0NlDYOAt-_3D3L7nA");//old telenotificatios db
       // messaging.usePublicVapidKey('BFaRUoKGxUnhw1jL3q-jRuq3X5WwVPB7roHZQKxTUpAfE48sN7G3GWJGGAIIFJHeIuE-DHzqXyeCVe7eG-S1ulQ'); //new telenotificatios db

        //messaging.usePublicVapidKey('BK9GsbmLr2ohFs7VaIZbzvy67i-3FRtaBeKeAVwEiiuOvk5cRsZOoNKoxUMAQTf_wSSLAumO9c5cb9-KFYj_U4o'); //telecare uat dbs

        messaging.usePublicVapidKey('BCt_gF974whazZ4CfseUT5psM9SlWrO2uR12PtqEYOBzRWrnOzU00nNNVb9RA0hDFh7NDvA89vGqxeM4GVOHf-w'); //telecare production dbs

        const token = await messaging.getToken();
        //alert(token);
        
        //console.log('token is here:', token);
        return token;
    } catch (error) {
        console.log(error);
    }
}

function PrintToken(token) {
    console.log(token);
    //console.log('Get Value from promise : ' + Promise.resolve(token));
    //console.log('Get Value from promise : ' + token.then(function (e) { return e.resolve(); }));
    var promise = new Promise(function (resolve, reject) {
        resolve(token);
    });
    promise.then(function (e) {
        getToken = e;
        console.log('Success, token : ' + e);
        CreateToken();
        //console.log('Get value from variable : ' + get_Token);
       // ForBackGroundWorker();
    }).catch(function (e) {
        console.error('husnain look this error : ' + e);
    });

}

function CreateToken() {
    //var token = "kdsfs4-4l5skjdf7-450989";
    console.log('save token in var is :' + getToken);
    if (getToken) {
        $.ajax({
            cache: false,
            async: true,
            type: "POST",
            url: '/Case/CreateTokens?phy_token_key=' + getToken,
            success: function (data) {               
                console.log('Token Saved Successfully!');
                loginUsernameFB(data.fre_firebase_email, data.fre_firebase_email, 1, data.fre_firstname, data.fre_profileimg);
                //include(data);
            },
            error: function () {
                console.log('Token not Saved!');
            }
        });
    }
   
}

function include(data) {
    var head = document.getElementsByTagName('head')[0];
    //var js = document.createElement("script");
    //js.type = "text/javascript";
    //js.src = "https://www.gstatic.com/firebasejs/3.6.9/firebase.js";
    //head.appendChild(js);

    //var _js = document.createElement("script");
    //_js.type = "text/javascript";
    //_js.src = "https://www.gstatic.com/firebasejs/5.9.1/firebase-auth.js";
    ////_js.src = "/Scripts/firebase/js/firebase_auth.js";
    //head.appendChild(_js);
    //CallView();
    loginUsernameFB(data.fre_firebase_email, data.fre_firebase_email, 1, data.fre_firstname, data.fre_profileimg);
}
function CallView() {
    var url = '/home/_firebase';
    $.get(url, function (data) {
        $('#divfirebase').html(data);
    });
}



function SendMsg(body) {
    var message = {
        data: {
            score: '850',
            time: '2:45'
        },
        token: 'cZPD_xVO2n7Mye_0rEmrgG:APA91bFswYupnFHSuHcix_QQjipC2Krbe4Je9PJ8Gz1WNBgM9AJQ84PNUhbXQyXapq7NS-jZ-loiop9WTFbFetpA2uzPko4UHX6iM6zpnIIYWTIY2brFAfGbXIdOoI0u9qxqB01JcZcy'
    };
    var registrationTokens = [
        'cZPD_xVO2n7Mye_0rEmrgG:APA91bFswYupnFHSuHcix_QQjipC2Krbe4Je9PJ8Gz1WNBgM9AJQ84PNUhbXQyXapq7NS-jZ-loiop9WTFbFetpA2uzPko4UHX6iM6zpnIIYWTIY2brFAfGbXIdOoI0u9qxqB01JcZcy',
        'ekYlVAzUIacznMhzxbfxzK:APA91bGFujdNdl5IM8WVerYsa-Smc7OMueEBtywGylgd62SoLtJqyUAl8sVNH-r8tFFVRK8OFWAOsq2YglRTCa9WwUBgWCTtTPXfNX9Bku6_9m1zkt5gYqAfY0Y0frTiyFMtEvaEuwkF'
    ];
    var topic = 'testing ttesting';
    

    // Create a request variable and assign a new XMLHttpRequest object to it.
    //var request = new XMLHttpRequest()
    //// Open a new connection, using the GET request on the URL endpoint
    //request.open('POST', 'https://fcm.googleapis.com/fcm/send', true)
    //request.onload = function () {
    //    // Begin accessing JSON data here
    //}
    //// Send request
    //request.send();

    //xhttp.setRequestHeader("Content-type", "application/json");
    //xhttp.setRequestHeader("Authorization", 'key = AAAAob7gCuE:APA91bFaLo65Xsw1SxE9D6C-OAzo5d0sV3RqEY6E8bnfYseH6xj_0LK0yqfrBCzziPnlzFo9FTQeeB796ie4-zWyV7NsVB8sP0AxWpVf6VqmQR_MWZLiLuD3RrUZY3F9LWUwv09UkQtb');
    //    $.ajax({
    //        headers: {
    //            "key": "key = AAAAob7gCuE:APA91bFaLo65Xsw1SxE9D6C-OAzo5d0sV3RqEY6E8bnfYseH6xj_0LK0yqfrBCzziPnlzFo9FTQeeB796ie4-zWyV7NsVB8sP0AxWpVf6VqmQR_MWZLiLuD3RrUZY3F9LWUwv09UkQtb",
    //            "Accept": "application/json",//depends on your api
    //            "Content-type": "application/json"//"application/x-www-form-urlencoded"//depends on your api
    //        }, url: "https://fcm.googleapis.com/fcm/send",
    //        success: function (response) {
    //            var r = JSON.parse(response);
    //            console.log(r.base);
    //        }
    //    });
    //}
}



function GrpCreate(name, grptype, msg, physician, navArr) {
    //alert(name + ',' + grptype + ',' + msg + ',' + physician, navArr);
    //alert('grp called;');
    try {
        if (!firebase.apps.length) {
            firebase.initializeApp(config);
        }
        console.log('nva arr is  : ', navArr);
        SenderId = 'lvB46mLF5qbQuIIAm5eqHxDdMBv1';//'0Ph7pKpC80hb15mgLFM5X4xXvzI2';//physician;//'thusmIlApVOyx9M8zrGe9CgsKTC3';//'lvB46mLF5qbQuIIAm5eqHxDdMBv1';
        _senderName = name;//'Muhammad Masud';
         _senderPhoto = '/Content/images/M.png';
        var refGrp = firebase.database().ref("Groups");
        refGrp.orderByChild("grpFor").equalTo(physician).once("value", snapshot => {
            console.log(snapshot);
            if (snapshot.exists()) {
                ExistingGroup(name, grptype, msg, physician, navArr, SenderId, _senderName, _senderPhoto);
            }
            else {
                CreatNewGrp(name, grptype, msg, physician, navArr);
            }
        });
    } catch (error) {
        console.error(error);
    }
}

function CreatNewGrp(name, grptype, msg, physician, navArr) {
    var getkey = firebase.database().ref().child("Groups").push().key;
    console.log('id is :   ' + getkey);
    if (getkey)
        groupID = getkey;
    console.log('yup , ' + groupID);
    firebase.database().ref('Groups').child(getkey).set({
        groupName: name,
        createdBy: SenderId,
        id: SenderId,
        teleid: 1,
        type: 'Admin',
        grpFor: physician,
        userName: _senderName,
        image: '/Content/images/group.png',//_senderPhoto,
        dateTime: firebase.database.ServerValue.TIMESTAMP,
        grpType: grptype
    });
    firebase.database().ref('Groups/' + getkey + '/users/').child(SenderId).set({
        id: SenderId,
        groupName: name,
        teleid: 1,
        type: 'Admin',
        userName: _senderName,
        image: _senderPhoto,
        dateTime: firebase.database.ServerValue.TIMESTAMP
    });

    //GetGroupByKey(getkey);
    isGroup = true;
    _receiverId = getkey;
    _receiverName = name;
    _receiverPhoto = '/Content/images/group.png';

    //let msg = msg//'Dr Masud Stroke Sent To you';
    let shortmsg = msg.substring(0, 10) + '...';

    var _type = '';
    if (isGroup)
        _type = 'Public';
    else
        _type = 'Private';

    var ref = firebase.database().ref("TeleUsers/" + SenderId + '/Connections');
    //child(_receiverId).set(
    ref.child(_receiverId).update({
        lastOnline: firebase.database.ServerValue.TIMESTAMP,
        name: _receiverName,
        image: _receiverPhoto,
        lastMessage: shortmsg,
        type: _type,
        msgNode: SenderId + '-' + _receiverId
    }).then(function () {
        if (_type === 'Private')
            return ref.child(SenderId).once("value");
        else
            return ref.child(_receiverId).once("value");
    }).then(function (snapshot) {
        //debugger;
        var data = snapshot.val();
        var time = snapshot.val().lastOnline;
        //console.log('my local time is : ' + time);
        var reff = firebase.database().ref("TeleUsers/" + SenderId + '/Connections');
        reff.child(_receiverId).update({
            lastOnline: time * -1
        });

        // AfterModify();
    });
    if (isGroup) {
        //LoadGrpUsers(shortmsg);
        var refConn = firebase.database().ref("TeleUsers/" + SenderId + '/ConnectionRules');
        refConn.child(_receiverId).update({
            msgNode: _receiverId
        });
    }
    // send auto generated msg to group
    let grpid = getkey;
    firebase.database().ref('userMessages/' + grpid).push({
        isRead: false,
        grpId: grpid,
        senderId: SenderId,
        read: false,
        senderName: _senderName,
        senderPhoto: _senderPhoto,
        dateTime: firebase.database.ServerValue.TIMESTAMP,
        teleid: 1,
        message: msg,
        type: 'Public',
        msgType: 'text'
    });

    /// add users in firebase grp
    for (var i = 0; i < navArr.length; i++) {
        let _userid = navArr[i].firbaseuid;
        let _name = navArr[i].name;
        let _image = navArr[i].ImgPath;
        let grpId = getkey;
        let nodeid = _userid + '-' + grpId;
        let grpname = name;
        //userid, name, image, grpid, nodeid, grpname
        firebase.database().ref('Groups/' + grpId + '/users').child(_userid).set({
            id: _userid,
            teleid: 1,
            groupId: grpId,
            groupName: grpname,
            type: 'user',
            userName: _name,
            image: _image,
            dateTime: firebase.database.ServerValue.TIMESTAMP
        });
        firebase.database().ref("TeleUsers/" + _userid + "/Connections").child(grpId).set({
            lastOnline: firebase.database.ServerValue.TIMESTAMP,
            name: grpname,
            image: _receiverPhoto,
            lastMessage: '',
            type: 'Public'
        });

        firebase.database().ref("TeleUsers/" + _userid + "/ConnectionRules").child(grpId).set({
            msgNode: grpId
        });
    }

    // users add code end
}
function ExistingGroup(name, grptype, msg, physician, navArr, SenderId, _senderName, _senderPhoto) {
    var refGrp = firebase.database().ref("Groups");
    refGrp.orderByChild('grpFor').equalTo(physician).once('child_added', function (snapshot) {
        grpCreateStatus = false;
        var grpid = snapshot.key;//'-MDUZ8SNjw3E6m7bBWjs';
        console.log('global grp id is : ', grpid);
        firebase.database().ref('userMessages/' + grpid).push({
            isRead: false,
            grpId: grpid,
            senderId: SenderId,
            read: false,
            senderName: _senderName,
            senderPhoto: _senderPhoto,
            dateTime: firebase.database.ServerValue.TIMESTAMP,
            teleid: 1,
            message: msg,
            type: 'Public',
            msgType: 'text'
        });
    });
}


function AddUserInGrp(physician, userid) {

}

function AcceptCase(msg, userId) {
    console.log('msg is  : ' + msg + ' ,  ' + ' userid : ' + userId);
    console.log('senderid, name, image : ' + SenderId + ' , ' + _senderName + ' , ' + _senderPhoto);
    if (!firebase.apps.length) {
        firebase.initializeApp(config);
    }
    var ref = firebase.database().ref("Groups");
    ref.orderByChild("grpFor").equalTo(userId).on("child_added", function (snapshot) {
        var grpid = snapshot.key;
        console.log('global grp id is : ' + grpid);
        firebase.database().ref('userMessages/' + grpid).push({
            isRead: false,
            grpId: grpid,
            senderId: SenderId,
            read: false,
            senderName: _senderName,
            senderPhoto: _senderPhoto,
            dateTime: firebase.database.ServerValue.TIMESTAMP,
            teleid: 1,
            message: msg,
            type: 'Public',
            msgType: 'text'
        });
    });

}

function CheckGrpExist(userId) {
    if (!firebase.apps.length) {
        firebase.initializeApp(config);
    }
    let ref = firebase.database().ref("Groups");
    ref.orderByChild("grpFor").equalTo(userId).once("value", snapshot => {
        if (snapshot.exists()) {
            const userData = snapshot.val();
            console.log("exists!", userData);
            return true;
        }
        else {
            return false;
        }
    });

    //ref.orderByChild("grpFor").equalTo(userId).on("child_added", function (snapshot) {
    //    console.log('sanapshot value is : ', snapshot);
    //    console.log('snapshot is  : ', snapshot);
    //    if (snapshot.exists()) {
    //        console.log("exists!");
    //        return true;
    //    }
    //    else {
    //        console.log("not exists!");
    //        return false;
    //    }
    //});
}

function callGrp(status) {
    var promise = new Promise(function (resolve, reject) {
        resolve(status);
    });
    promise.then(function (e) {
        console.log('Success, status : ' + e);
        return e;
    }).catch(function (e) {
        console.error('husnain look this error : ' + e);
    });
}

function GetGroupByKey(key) {
    var ref = firebase.database().ref("Groups/" + key);
    ref.on('value', function (data) {
        console.log('data val :' ,data.val());
        _receiverId = data.key;
        _receiverName = data.val().groupName;
        _receiverPhoto = data.val().image;
        isGroup = true;
    });
    var txt = '';
    UserConnectionsForCall(txt);
}
function UserConnectionsForCall(shortmsg) {
    var _type = '';
    if (isGroup)
        _type = 'Public';
    else
        _type = 'Private';

    var ref = firebase.database().ref("TeleUsers/" + SenderId + '/Connections');
    //child(_receiverId).set(
    ref.child(_receiverId).update({
        lastOnline: firebase.database.ServerValue.TIMESTAMP,
        name: _receiverName,
        image: _receiverPhoto,
        lastMessage: shortmsg,
        type: _type,
        msgNode: SenderId + '-' + _receiverId
    }).then(function () {
        if (_type === 'Private')
            return ref.child(SenderId).once("value");
        else
            return ref.child(_receiverId).once("value");
    }).then(function (snapshot) {
        //debugger;
        var data = snapshot.val();
        var time = snapshot.val().lastOnline;
        //console.log('my local time is : ' + time);
        var reff = firebase.database().ref("TeleUsers/" + SenderId + '/Connections');
        reff.child(_receiverId).update({
            lastOnline: time * -1
        });

        // AfterModify();
    });
    if (isGroup) {
        LoadGrpUsers(shortmsg);
        let refConn = firebase.database().ref("TeleUsers/" + SenderId + '/ConnectionRules');
        refConn.child(_receiverId).update({
            msgNode: _receiverId
        });
    }

    if (isGroup === false) {
        let refConn = firebase.database().ref("TeleUsers/" + _receiverId + '/Connections');
        refConn.child(SenderId).update({
            lastOnline: firebase.database.ServerValue.TIMESTAMP,
            name: _senderName,
            image: _senderPhoto,
            lastMessage: shortmsg,
            type: _type
        }).then(function () {
            return refConn.child(SenderId).once("value");
        }).then(function (snapshot) {
            // debugger;
            var data = snapshot.val();
            var time = snapshot.val().lastOnline;
            var unread = snapshot.val().unread;
            if (unread)
                unread += 1;
            else
                unread = 1;
            //console.log('my local time is : ' + time);
            var reff = firebase.database().ref("TeleUsers/" + _receiverId + '/Connections');
            reff.child(SenderId).update({
                lastOnline: time * -1,
                unread: unread
            });
            // AfterModify();
        });



        var id = SenderId + "-" + _receiverId;
        var _id = _receiverId + "-" + SenderId;
        // debugger;
        var isExist = "not";

        var getRule = firebase.database().ref("TeleUsers/" + SenderId + '/ConnectionRules');
        getRule.child(id).on('value', function (snapshot) {
            if (snapshot.val() !== null) {
                isExist = 'found';
                // console.log('value is : ' + isExist + ' ,  ' + snapshot.key);
            }
            else {
                getRule.child(_id).on('value', function (data) {
                    if (data.val() !== null) {
                        isExist = '_found';
                        //  console.log('value is : ' + isExist + ' ,  ' + data.key);
                    }
                    else {
                        isExist = 'noFound';
                    }
                });
            }
        });

        /*
        if (isExist === 'noFound') {
            var _getRule = firebase.database().ref("TeleUsers/" + SenderId + '/ConnectionRules');
            _getRule.child(_id).on('value', function (snapshot) {
                isExist = '_found';
                console.log('value is : ' + isExist + ' ,  ' + snapshot.key);
            });
        }
        */

        if (isExist === 'noFound') {
            var refConRule = firebase.database().ref("TeleUsers/" + SenderId + '/ConnectionRules');
            refConRule.child(id).update({
                msgNode: id
            });

            var _refConRule = firebase.database().ref("TeleUsers/" + _receiverId + '/ConnectionRules');
            _refConRule.child(id).update({
                msgNode: id
            });
        }



    }
}
function LoadGrpUsers(shortmsg) {
    var _Users = [];
    var grpId = _receiverId;
    var ref = firebase.database().ref("Groups/" + grpId + "/users");
    ref.on("child_added", function (snapshot) {
        //_Users.push({ id: snapshot.key });
        var id = snapshot.key;
        if (id !== SenderId) {
            var refCon = firebase.database().ref("TeleUsers/" + id + '/Connections');
            refCon.child(grpId).update({
                lastOnline: firebase.database.ServerValue.TIMESTAMP,
                lastMessage: shortmsg,
                type: 'Public'
            }).then(function () {
                return refCon.child(grpId).once("value");
            }).then(function (snapshot) {
                // debugger;
                var data = snapshot.val();
                var time = snapshot.val().lastOnline;
                var unread = snapshot.val().unread;
                if (unread)
                    unread += 1;
                else
                    unread = 1;
                //console.log('my local time is : ' + time);
                var reff = firebase.database().ref("TeleUsers/" + id + '/Connections');
                reff.child(grpId).update({
                    lastOnline: time * -1,
                    unread: unread
                });

            });
        }
    });
}

// husnain code for firebase

function loginUsernameFB(email, password, tele_id, username, img) {
   // alert('in tyhis is :'  + email);
    if (!firebase.apps.length) {
        firebase.initializeApp(config);
    }
    firebase.auth().signInWithEmailAndPassword(email, password).then(function (value) {
        firebase.auth().onAuthStateChanged(function (user) {
            if (user) {
                var userId = user.uid;
                console.log('logged in user : ' + userId);
                SenderId = userId;
                SaveInDB(userId);
                _SignInStatus();
                var ref = firebase.database().ref("TeleUsers");
                ref.child(userId).once('value', function (snapshot) {
                    if (snapshot.val() !== null) {
                        $("#userInfo").html('');
                         _senderName = snapshot.val().name;
                         _senderPhoto = snapshot.val().image;
                        //let _teleid = snapshot.val().teleid;
                        var user_info = 'on';//"<div class='row'><img src='" + _senderPhoto + "' style='height:20px;width:20px' /> <span> " + _senderName + " </span>&nbsp;<span class='label label-rouded label-danger' id='lblnotify'></span></div>";
                        $("#lblFirebase").html(user_info);
                    }
                    else {
                        // alert('record not found');
                        firebase.database().ref('TeleUsers').child(userId).set({
                            id: userId,
                            name: username,
                            teleid: tele_id,
                            image: img
                        });
                        location.reload();
                    }

                });

            }
            else {
                $("#lblFirebase").html('off');
            }
        });

    }).catch(function (error) {
        toast(error.message, 7000);
    });


}
function SaveInDB(userid) {
    //if (userid) {
    //    $.ajax({
    //        type: 'POST',
    //        url: '/firebaseChat/SaveId',
    //        data: { id: userid },
    //        success: function (e) {
    //        },
    //        Error: function (e) {
    //        }
    //    });
    //}
}
function _SignInStatus(userid) {
    var ref = firebase.database().ref("TeleUsers");
    ref.on("child_added", function (snapshot) {
        ref.child(snapshot.key).child('Connections').on('child_added', function (data) {
            // console.log(data.key);
            if (data.key === userid) {
                var _ref = firebase.database().ref("TeleUsers/" + snapshot.key + "/Connections");
                _ref.child(userid).update({
                    Online: true,
                    lastOnline: firebase.database.ServerValue.TIMESTAMP
                });
            }
        });
    });
}
// husnain code end