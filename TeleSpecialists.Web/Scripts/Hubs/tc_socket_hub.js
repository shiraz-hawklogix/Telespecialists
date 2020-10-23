var ws;
$(document).ready(function () {
    window.setTimeout(function () { connectSocket(); }, 5000);
    function connectSocket() {

        ws = new WebSocket("wss://" + window.location.origin.replace("https://", "").replace("http://", "") + "/TCSocket/Get");

        ws.onopen = function () {
            console.log('connected');
            snoozePoupReloadCode();
        };
        ws.onmessage = function (evt) {
            var data = JSON.parse(atob(evt.data));
            if ($.trim(data.MethodName) != "") {

                var paramsData = [];
                if (data.Data != 'undefined' && data.Data != null) {
                    paramsData = data.Data;
                }
                try {

                    window[$.trim(data.MethodName)].apply(null, paramsData);



                }
                catch (e) {
                    console.log(e);
                }

            }

        };
        ws.onerror = function (evt) {
            console.log(evt.message);
        };
        ws.onclose = function () {
            console.log('Socket is closed. Reconnect will be attempted in 0.2 second.');
            window.setTimeout(function () { connectSocket(); }, 200);
        };
    }
})

