


//importScripts('https://www.gstatic.com/firebasejs/7.9.1/firebase-app.js');
//importScripts('https://www.gstatic.com/firebasejs/7.9.1/firebase-messaging.js');
//importScripts('/FireBaseChat/js/init.js');
//const messaging = firebase.messaging();

//<<<<<<<<<<*********** husnain code start *************>>>>>>>>>>>

//importScripts('https://www.gstatic.com/firebasejs/7.9.1/firebase-app.js');
//importScripts('https://www.gstatic.com/firebasejs/7.9.1/firebase-messaging.js');

// local  downloaded links
importScripts('/Scripts/firebase/firebasejs/firebase-app.js');
importScripts('/Scripts/firebase/firebasejs/firebase-messaging.js');


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
// msgs with telecare
const firebaseConfig = {
    apiKey: "AIzaSyDHC8RJ-uUoWIo3tYa9zzhVhvTxv5NuZF0",
    authDomain: "telecare-3724e.firebaseapp.com",
    databaseURL: "https://telecare-3724e.firebaseio.com",
    projectId: "telecare-3724e",
    storageBucket: "telecare-3724e.appspot.com",
    messagingSenderId: "922280291570",
    appId: "1:922280291570:web:6ce723cfaeeed0f7c8b46b",
    measurementId: "G-8TWCZJP852"
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
    let caseObjStr = payload['objectData'];

    if (caseId) {
        var listener = new BroadcastChannel('listener');
        listener.postMessage(payload);
    }
    // Customize notification here
    const notificationTitle = 'New Stroke Alert';
    const notificationOptions = {
        body: 'You Have New Stroke Alert.',
        icon: 'https://d31029zd06w0t6.cloudfront.net/wp-content/uploads/sites/63/2019/05/Surviving-a-Stroke.jpg'
    };

    return self.registration.showNotification(notificationTitle,
        notificationOptions);

});

//<<<<<<<<<<*********** husnain code end *************>>>>>>>>>>>
