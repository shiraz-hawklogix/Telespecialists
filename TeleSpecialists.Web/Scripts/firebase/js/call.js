
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
//use for auto stamps of sending msgs
var autoSenderId;
var autoSenderName;
var autoSenderImage;
/////////////////////////////////


//var messaging;
$(document).ready(function () {
    var blastDiv = localStorage.getItem("activeBlasts");
    if (blastDiv)
        $('#divInternalExternal').html(blastDiv);
    PrintToken(Get_Token());
    GetMuteStatus();
});
async function Get_Token() {
    try {
        const messaging = firebase.messaging();
        await messaging.requestPermission();
        //messaging.usePublicVapidKey("BOvSRNGbyCBPiAc8eq9MGz-cPhzBOA4oxkOyHu6p-XW2cT4lWiqBbM7HVU6K9pRINCX-Nu0NlDYOAt-_3D3L7nA");//old telenotificatios db
        //messaging.usePublicVapidKey('BFaRUoKGxUnhw1jL3q-jRuq3X5WwVPB7roHZQKxTUpAfE48sN7G3GWJGGAIIFJHeIuE-DHzqXyeCVe7eG-S1ulQ'); //new telenotificatios db

        messaging.usePublicVapidKey('BK9GsbmLr2ohFs7VaIZbzvy67i-3FRtaBeKeAVwEiiuOvk5cRsZOoNKoxUMAQTf_wSSLAumO9c5cb9-KFYj_U4o'); //telecare dbs


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
            },
            error: function () {
                console.log('Token not Saved!');
            }
        });
    }
   
}

function GrpCreate(name, grptype, msg, physician, navArr) {
    //alert(name + ',' + grptype + ',' + msg + ',' + physician, navArr);
    //alert('grp called;');
    try {
        if (!firebase.apps.length) {
            firebase.initializeApp(config);
        }
        console.log('nva arr is  : ', navArr);
        autoSenderId = 'lvB46mLF5qbQuIIAm5eqHxDdMBv1';
        autoSenderName = 'Muhammad Masud Admin';
        autoSenderImage = '/Content/images/M.png';
        var refGrp = firebase.database().ref("Groups");
        refGrp.orderByChild("grpFor").equalTo(physician).once("value", snapshot => {
            console.log(snapshot);
            if (snapshot.exists()) {
                ExistingGroup(name, grptype, msg, physician, navArr, autoSenderId, autoSenderName, autoSenderImage);
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
    try {
        var getkey = firebase.database().ref().child("Groups").push().key;
        console.log('id is :   ' + getkey);
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
        if (getkey)
            groupID = getkey;
        console.log('yup , ' + groupID);
        firebase.database().ref('Groups').child(getkey).set({
            groupName: name,
            createdBy: autoSenderId,
            id: autoSenderId,
            teleid: 1,
            type: 'Admin',
            grpFor: physician,
            userName: autoSenderName,
            image: '/Content/images/group.png',//_senderPhoto,
            dateTime: firebase.database.ServerValue.TIMESTAMP,
            grpType: grptype
        });
        firebase.database().ref('Groups/' + getkey + '/users/').child(autoSenderId).set({
            id: autoSenderId,
            groupName: name,
            teleid: 1,
            type: 'Admin',
            userName: autoSenderName,
            image: autoSenderImage,
            dateTime: firebase.database.ServerValue.TIMESTAMP
        });
        // add sender in grp
        firebase.database().ref('Groups/' + getkey + '/users/').child(SenderId).set({
            id: SenderId,
            groupName: name,
            teleid: 1,
            type: 'Admin',
            userName: _senderName,
            image: _senderPhoto,
            dateTime: firebase.database.ServerValue.TIMESTAMP
        });
        firebase.database().ref("TeleUsers/" + SenderId + "/Connections").child(groupID).set({
            lastOnline: firebase.database.ServerValue.TIMESTAMP,
            lastMsgDateTime: firebase.database.ServerValue.TIMESTAMP,
            name: name,
            image: _receiverPhoto,
            lastMessage: shortmsg,
            type: 'Public',
            grpid: groupID
        }).then(function () {
            return firebase.database().ref("TeleUsers/" + SenderId + "/Connections").child(_receiverId).once("value");
        }).then(function (snapshot) {
            var _time = snapshot.val().lastOnline;
            var reffSender = firebase.database().ref("TeleUsers/" + SenderId + '/Connections');
            reffSender.child(_receiverId).update({
                lastOnline: _time * -1
            });
        });
        firebase.database().ref("TeleUsers/" + SenderId + "/ConnectionRules").child(groupID).set({
            msgNode: groupID
        });
       
        var ref = firebase.database().ref("TeleUsers/" + autoSenderId + '/Connections');
        //child(_receiverId).set(
        ref.child(_receiverId).update({
            lastOnline: firebase.database.ServerValue.TIMESTAMP,
            lastMsgDateTime: firebase.database.ServerValue.TIMESTAMP,
            name: _receiverName,
            image: _receiverPhoto,
            lastMessage: shortmsg,
            type: _type,
            msgNode: autoSenderId + '-' + _receiverId,
            grpid: groupID
        });
        //.then(function () {
        //    return ref.child(_receiverId).once("value");
        //}).then(function (snapshot) {
        //    var time = snapshot.val().lastOnline;
        //    //console.log('my local time is : ' + time);
        //    var reff = firebase.database().ref("TeleUsers/" + autoSenderId + '/Connections');
        //    reff.child(_receiverId).update({
        //        lastOnline: time * -1
        //    });

        //    // AfterModify();
        //});
        if (isGroup) {
            //LoadGrpUsers(shortmsg);
            var refConn = firebase.database().ref("TeleUsers/" + autoSenderId + '/ConnectionRules');
            refConn.child(_receiverId).update({
                msgNode: _receiverId
            });
        }
        
        var encryptMsg = CryptoJS.AES.encrypt(msg, '123');
        // send auto generated msg to group
        let grpid = getkey;
        firebase.database().ref('userMessages/' + grpid).push({
            isRead: false,
            grpId: grpid,
            senderId: autoSenderId,
            read: false,
            senderName: autoSenderName,
            senderPhoto: autoSenderImage,
            dateTime: firebase.database.ServerValue.TIMESTAMP,
            teleid: 1,
            message: encryptMsg.toString(), //msg,
            type: 'Public',
            msgType: 'text'
        });
        // open it after testing
        addNavigators(name, grptype, msg, physician, navArr, getkey, shortmsg);
        UserConnectionFB(groupID, shortmsg, name, _receiverPhoto);
    }
    catch (error) {
        console.error('found error in create grp: ',error);
    }
    
    // users add code end
}
function ExistingGroup(name, grptype, msg, physician, navArr, _autosenderid, _autoname, _autophoto) {
    var encryptMsg = CryptoJS.AES.encrypt(msg, '123');
    var refGrp = firebase.database().ref("Groups");
    refGrp.orderByChild('grpFor').equalTo(physician).once('child_added', function (snapshot) {
        grpCreateStatus = false;
        var grpid = snapshot.key;//'-MDUZ8SNjw3E6m7bBWjs';
        console.log('global grp id is : ', grpid);
        firebase.database().ref('userMessages/' + grpid).push({
            isRead: false,
            grpId: grpid,
            senderId: _autosenderid,
            read: false,
            senderName: _autoname,
            senderPhoto: _autophoto,
            dateTime: firebase.database.ServerValue.TIMESTAMP,
            teleid: 1,
            message: encryptMsg.toString(),//msg,
            type: 'Public',
            msgType: 'text'
        });
        var shortmsg = msg.substring(0, 10) + '...';
        UserConnectionFB(grpid, shortmsg, name, _autophoto);
    });
}

function addNavigators(name, grptype, msg, physician, navArr, getkey, shortmsg) {
    /// add users in firebase grp
    console.log('inisede navArr loop array : ' , navArr);
    for (var i = 0; i < navArr.length; i++) {
        let _userid = navArr[i].firbaseuid;
        let _name = navArr[i].name;
        let _image = navArr[i].ImgPath;
        let grpId = getkey;
        let nodeid = _userid + '-' + grpId;
        let grpname = name;
        if (_userid) {
            // add users in group
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
            // add group in user connections
            var ref = firebase.database().ref("TeleUsers/" + _userid + "/Connections");
            ref.child(grpId).set({
                lastOnline: firebase.database.ServerValue.TIMESTAMP,
                lastMsgDateTime: firebase.database.ServerValue.TIMESTAMP,
                name: grpname,
                image: _receiverPhoto,
                lastMessage: shortmsg,
                type: 'Public',
                grpid: grpId
            }).then(function () {
                return ref.child(grpId).once("value");
            }).then(function (snapshot) {
                var time = snapshot.val().lastOnline;
                var reff = firebase.database().ref("TeleUsers/" + _userid + '/Connections');
                reff.child(grpId).update({
                    lastOnline: time * -1
                });
            });
            // add group in connection Rules
            firebase.database().ref("TeleUsers/" + _userid + "/ConnectionRules").child(grpId).set({
                msgNode: grpId
            });
        }
    }
        // user added end code
}


function AcceptCase(msg, userId) {
    console.log('msg is  : ' + msg + ' ,  ' + ' userid : ' + userId);
    console.log('senderid, name, image : ' + SenderId + ' , ' + _senderName + ' , ' + _senderPhoto);
    var encryptMsg = CryptoJS.AES.encrypt(msg, '123');
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
            message: encryptMsg.toString(),
            type: 'Public',
            msgType: 'text'
        });
        var shortmsg = msg.substring(0, 10) + '...';
        UserConnectionFB(grpid, shortmsg);
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

function GetGroupByKey_modified(key) {
    var ref = firebase.database().ref("Groups/" + key);
    ref.on('value', function (data) {
        console.log('data val :' ,data.val());
        _receiverId = data.key;
        _receiverName = data.val().groupName;
        _receiverPhoto = data.val().image;
        isGroup = true;
    });
    var txt = '';
    //UserConnectionsForCall(txt);
}
// UserConnectionsForCall currently not in use
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
        lastMsgDateTime: firebase.database.ServerValue.TIMESTAMP,
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
            lastMsgDateTime: firebase.database.ServerValue.TIMESTAMP,
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
            }
            else {
                getRule.child(_id).on('value', function (data) {
                    if (data.val() !== null) {
                        isExist = '_found';
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
                lastMsgDateTime: firebase.database.ServerValue.TIMESTAMP,
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

function loginUsernameFB(email, password, tele_id, username, img, userSqlid, saveUser = 'user' ) {
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
                if (saveUser === 'custom')
                    SaveUserInDB(userId, userSqlid);
                else
                    SaveInDB(userId);
                SignInStatus(userId);

                UserUnreadMsgsChanged();
                UserUnreadMsgsModify();

                var ref = firebase.database().ref("TeleUsers");
                ref.child(userId).once('value', function (snapshot) {
                    if (snapshot.val() !== null) {
                        $("#userInfo").html('');
                         _senderName = snapshot.val().name;
                         _senderPhoto = snapshot.val().image;
                        //let _teleid = snapshot.val().teleid;
                        var user_info = 'on';
                        $("#lblFirebase").html(user_info);
                        // update user image in its connections use, can be used for existing db
                        UpdateUserInfo(userId, username, img);
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
        CreateWithPassword(email, password, tele_id, username, img, userSqlid);
    });


}
// create user if not found
function CreateWithPassword(email, password, tele_id, username, img, userSqlid) {
    firebase.auth().createUserWithEmailAndPassword(email, password)
        .then(function (user) {
            console.log('new user create in db :', user);
            console.log('all in one:',email, password, tele_id, username, img, userSqlid);
            loginUsernameFB(email, password, tele_id, username, img, userSqlid);
        })
        .catch(function (error) {
            console.error('this user still  not added in firebase look into error:', error);
        });
}
// code end

//userSqlid
function SaveInDB(userid) {
    if (userid) {
        $.ajax({
            type: 'POST',
            url: '/firebaseChat/SaveId',
            data: { id: userid },
            success: function (e) {
            },
            Error: function (e) {
            }
        });
    }
}
function SaveUserInDB(userid, userSqlid) {
    if (userid) {
        $.ajax({
            type: 'POST',
            url: '/firebaseChat/SaveCustomUserId',
            data: { firebaseId: userid, sqlid: userSqlid},
            success: function (e) {
            },
            Error: function (e) {
            }
        });
    }
}
function SignInStatus(userid) {
    var ref = firebase.database().ref("TeleUsers");
    ref.on("child_added", function (snapshot) {
        ref.child(snapshot.key).child('Connections').on('child_added', function (data) {
            if (data.key === userid) {
                var _ref = firebase.database().ref("TeleUsers/" + snapshot.key + "/Connections");
                _ref.child(userid).update({
                    Online: true,
                    lastOnline: firebase.database.ServerValue.TIMESTAMP                    
                }).then(function () {
                    return _ref.child(userid).once("value");
                }).then(function (childSnapshot) {
                    var time = childSnapshot.val().lastOnline;
                    _ref.child(userid).update({
                        lastOnline: time * -1
                    });
                });
            }
        });
    });
}
function UserUnreadMsgsChanged() {
    var getConnection = firebase.database().ref("TeleUsers/" + SenderId + "/Connections");
    getConnection.orderByChild('lastOnline').on('child_changed', function (friends) {
       // console.log('UserUnreadMsgsChanged called!');
        var unreadmsg = '';
        if (friends.val().unread !== 0 && friends.val().unread) {
            unreadmsg = friends.val().unread;
            document.getElementById("firebaseLink").style.color = "#FCFC00";
            let muteStatus = localStorage.getItem('muteStatus');
            if (muteStatus === 'false')
                playMsgNotification();
            else
                console.log('tune muted');
        }
        else {
            document.getElementById("firebaseLink").style.color = "#FFFFFF";
            //$('#firebaseLink').hide();
            //$('#firebaseLink').show();
        }

    });

   }
function UserUnreadMsgsModify() {
    var getConnection = firebase.database().ref("TeleUsers/" + SenderId + "/Connections");
    getConnection.orderByChild('lastOnline').on('child_added', function (friends) {
        //console.log('UserUnreadMsgsModify called!');
        var unreadmsg = '';
        if (friends.val().unread !== 0 && friends.val().unread) {
            unreadmsg = friends.val().unread;
            document.getElementById("firebaseLink").style.color = "#FCFC00";
            let muteStatus = localStorage.getItem('muteStatus');
            if (muteStatus === 'false')
                playMsgNotification();
            else
                console.log('tune muted');
        }
        else {
            document.getElementById("firebaseLink").style.color = "#FFFFFF";
            //$('#firebaseLink').hide();
            //$('#firebaseLink').show();
        }

    });

}
function UserConnectionFB(grpid, shortmsg, grpName, grpimg) {
    var ref = firebase.database().ref("Groups/" + grpid + "/users");
    ref.on('child_added', function (snapshot) {
        var userConn = firebase.database().ref("TeleUsers/" + snapshot.key + "/Connections");
        userConn.orderByChild('grpid').equalTo(grpid).once('value', function (userExist) {
            if (userExist.exists()) {
                console.log('record found in connections');
                userConn.child(grpid).update({
                    lastMessage: shortmsg,
                    lastOnline: firebase.database.ServerValue.TIMESTAMP,
                    lastMsgDateTime: firebase.database.ServerValue.TIMESTAMP
                }).then(function () {
                    return userConn.child(grpid).once("value");
                }).then(function (childSnapshot) {
                    var unread = childSnapshot.val().unread;
                    var time = childSnapshot.val().lastOnline;
                    if (unread)
                        unread += 1;
                    else
                        unread = 1;
                    userConn.child(grpid).update({
                        unread: unread,
                        lastOnline: time * -1
                    });
                });
            }
            else {
                console.log('record noooot found in connections');
                AddUserConnections(snapshot.key, grpid, shortmsg, grpName, grpimg);
            }
        });
        
    });

}
function AddUserConnections(userid, groupID, shortmsg, grpName, grpimg) {
    let ref = firebase.database().ref("TeleUsers/" + userid + "/Connections");
    ref.child(groupID).set({
        lastOnline: firebase.database.ServerValue.TIMESTAMP,
        lastMsgDateTime: firebase.database.ServerValue.TIMESTAMP,
        name: grpName,
        image: grpimg,
        lastMessage: shortmsg,
        type: 'Public',
        grpid: groupID,
        msgNode: userid + '-' + groupID
    });
}
// husnain code end
// logout function  call
$('#signout').click(function () {
    localStorage.clear();
    signOutAndLogout(true);
});

function signOutAndLogout(reload = false) {
    console.log('logged out user : ' + SenderId);
    if (SenderId) {
        SignOutStatus();
        log_out();
        //if (reload)
        //    window.location.href = "/Account/Signout";
    }
}
function SignOutStatus() {
    var ref = firebase.database().ref("TeleUsers");
    ref.on("child_added", function (snapshot) {
        ref.child(snapshot.key).child('Connections').on('child_added', function (data) {
            if (data.key === SenderId) {
                console.log('user going to be sign out');
                var _ref = firebase.database().ref("TeleUsers/" + snapshot.key + "/Connections");
                _ref.child(SenderId).update({
                    Online: false,
                    lastOnline: firebase.database.ServerValue.TIMESTAMP
                }).then(function () {
                    return _ref.child(SenderId).once("value");
                }).then(function (childSnapshot) {
                    var time = childSnapshot.val().lastOnline;
                    _ref.child(SenderId).update({
                        lastOnline: time * -1
                    });
                });
            }
        });
    });
}
function log_out() {
    firebase.auth().signOut().then(function () {

    }).catch(function (error) {
        // console.log("Error signing user out:", error);
    });
}
// logout function end

function GetMuteStatus() {
    $.ajax({
        type: 'POST',
        url: '/firebaseChat/GetMuteStatus',
        success: function (e) {
            localStorage.setItem('muteStatus', e);
            if (e) {
                $('#fa_bell').removeClass('fa-bell');
                $('#fa_bell').addClass('fa-bell-slash');
            }
        },
        Error: function (e) {
        }
    });
}
function UpdateUserInfo(userid, username, img) {
    firebase.database().ref('TeleUsers').child(userid).update({
        name: username,
        image: img
    });
    let ref = firebase.database().ref("TeleUsers/" + userid + "/Connections");
    ref.orderByChild('type').equalTo('Private').on('child_added', function (snapshot) {
        //console.log('private users in connections:' + snapshot.key);
        let refConn = firebase.database().ref("TeleUsers/" + snapshot.key + "/Connections");
        refConn.child(userid).update({
            image: img,
            name: username
        })
    })

}
