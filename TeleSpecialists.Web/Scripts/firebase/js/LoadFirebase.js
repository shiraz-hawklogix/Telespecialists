
function SaveFB(name, email, teleid, img) {
    //logoutFirebase();
    if (!firebase.apps.length) {
        firebase.initializeApp(config);
    }

    var password = email;
    var tele_id = teleid;
    var username = name;
    
    firebase.auth().signInWithEmailAndPassword(email, password).then(function (value) {
        firebase.auth().onAuthStateChanged(function (user) {
            if (user) {
                var userId = user.uid;
                alert(userId);
                //SenderId = userId;
               // SaveInDB(userId);
              //  _SignInStatus();
                var ref = firebase.database().ref("TeleUsers");
                ref.child(userId).once('value', function (snapshot) {
                    if (snapshot.val() !== null) {
                        $("#userInfo").html('');
                        let _senderName = snapshot.val().name;
                        let _senderPhoto = snapshot.val().image;
                        let _teleid = snapshot.val().teleid;
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
        });

    }).catch(function (error) {
        toast(error.message, 7000);
    });
}