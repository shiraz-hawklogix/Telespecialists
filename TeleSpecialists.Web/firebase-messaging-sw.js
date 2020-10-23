//importScripts('https://www.gstatic.com/firebasejs/7.9.1/firebase-app.js');
//importScripts('https://www.gstatic.com/firebasejs/7.9.1/firebase-messaging.js');
//importScripts('/FireBaseChat/js/init.js');
//const messaging = firebase.messaging();

//<<<<<<<<<<*********** husnain code start *************>>>>>>>>>>>
importScripts('https://www.gstatic.com/firebasejs/7.9.1/firebase-app.js');
importScripts('https://www.gstatic.com/firebasejs/7.9.1/firebase-messaging.js');
// tele code with telenotification db
/*
var firebaseConfig = {
    apiKey: "AIzaSyBn7zsblu7K4SdNc48pcGXkGaUWLW7rWIo",
    authDomain: "telenotification-a48ca.firebaseapp.com",
    databaseURL: "https://telenotification-a48ca.firebaseio.com",
    projectId: "telenotification-a48ca",
    storageBucket: "telenotification-a48ca.appspot.com",
    messagingSenderId: "694692088545",
    appId: "1:694692088545:web:b488ac266dc3875ab945a0",
    measurementId: "G-SBF832LGH6"
};
*/
// msgs with telecare uat
//const firebaseConfig = {
//    apiKey: "AIzaSyDHC8RJ-uUoWIo3tYa9zzhVhvTxv5NuZF0",
//    authDomain: "telecare-3724e.firebaseapp.com",
//    databaseURL: "https://telecare-3724e.firebaseio.com",
//    projectId: "telecare-3724e",
//    storageBucket: "telecare-3724e.appspot.com",
//    messagingSenderId: "922280291570",
//    appId: "1:922280291570:web:6ce723cfaeeed0f7c8b46b",
//    measurementId: "G-8TWCZJP852"
//};


// msgs with telecare production

const firebaseConfig = {
    apiKey: "AIzaSyD78t_u3xK5eFQZyD95NL3-XGubH6z4XCo",
    authDomain: "telecare-c852b.firebaseapp.com",
    databaseURL: "https://telecare-c852b.firebaseio.com",
    projectId: "telecare-c852b",
    storageBucket: "telecare-c852b.appspot.com",
    messagingSenderId: "945324237265",
    appId: "1:945324237265:web:4ea74b5e9f81f1789f40f0",
    measurementId: "G-5WF4NMKNWZ"
};

// Initialize Firebase
firebase.initializeApp(firebaseConfig);
// Retrieve an instance of Firebase Messaging so that it can handle background
// messages.
//const messaging = firebase.messaging();




firebase.messaging().setBackgroundMessageHandler(function (payload) {
    console.log('husnain you Received background message ', payload);
    let arr_data = payload.data;
    let caseId = arr_data['caseId'];
    let caseType = arr_data['caseType'];
    console.log('case type is : ' + caseType);

    if (caseId) {
        var listener = new BroadcastChannel('listener');
        listener.postMessage(payload);
    }

    if (caseType !== 'TwoFactorAuth') {
        console.log('sent stroke StrokeAlertMessage');
        const notificationTitle = 'New Stroke Alert';
        const notificationOptions = {
            body: 'You Have New Stroke Alert.',
            icon: 'https://media.graytvinc.com/images/690*387/Stroke+MGN+graphic.JPG'
        };

        return self.registration.showNotification(notificationTitle,
            notificationOptions);
    }
});

//<<<<<<<<<<*********** husnain code end *************>>>>>>>>>>>

/**
 * Here is is the code snippet to initialize Firebase Messaging in the Service
 * Worker when your app is not hosted on Firebase Hosting.
 // [START initialize_firebase_in_sw]
 // Give the service worker access to Firebase Messaging.
 // Note that you can only use Firebase Messaging here, other Firebase libraries
 // are not available in the service worker.
 importScripts('https://www.gstatic.com/firebasejs/7.9.1/firebase-app.js');
 importScripts('https://www.gstatic.com/firebasejs/7.9.1/firebase-messaging.js');
 // Initialize the Firebase app in the service worker by passing in the
 // messagingSenderId.
 firebase.initializeApp({
   'messagingSenderId': 'YOUR-SENDER-ID'
 });
 // Retrieve an instance of Firebase Messaging so that it can handle background
 // messages.
 const messaging = firebase.messaging();
 // [END initialize_firebase_in_sw]
 **/


// If you would like to customize notifications that are received in the
// background (Web app is closed or not in browser focus) then you should
// implement this optional method.
// [START background_handler]
//messaging.setBackgroundMessageHandler(function (payload) {
//    console.log('[firebase-messaging-sw.js] Received background message ', payload);
//    // Customize notification here
//    const notificationTitle = 'Background Message Title';
//    const notificationOptions = {
//        body: 'Background Message body.',
//        icon: '/firebase-logo.png'
//    };

//    //return self.registration.showNotification(notificationTitle,
//    //    notificationOptions);
//});
