/*!
 * Five9 ADT iframe communication library
 * version: $f9build-version
 *
 * Copyright (c)2016 Five9, Inc.
 */
(function(factory) {
  if ( typeof define === "function" && define.amd ) {
    define(factory);
  } else if (typeof module === "object" && typeof module.exports === "object") {
    module.exports = factory();
  } else {
    window.Five9 = window.Five9 || {};
    window.Five9.IframeApi = factory();
  }
})(function() {
  var WindowPort = function () {
    this._targetWindow = window.top;
    this._targetOrigin = '*';
    this._sourceWindow = window;
  };

  WindowPort.prototype.postMessage = function (data) {
    this._targetWindow.postMessage(data, this._targetOrigin);
  };

  WindowPort.prototype.onMessage = function (fn) {
    this._sourceWindow.addEventListener('message', fn, false);

    // The very first message ('connect') comes to window.top
    // then adapter starts send message to the current window object
    if (this._sourceWindow !== window.top) {
      window.top.addEventListener('message', fn, false);
    }
  };

  WindowPort.prototype.updateTarget = function (event) {
    if (event.source && event.source !== this._targetWindow) {
      this._targetWindow = event.source;
      this._targetOrigin = event.origin;
    }
  };

  var BridgeScript = {
    _pagePort: new WindowPort(),
    _clientName: 'IFrameApiClient',
    _apiImpls: {}
  };

  BridgeScript.toPage = function (msg) {
    console.log("BridgeScript.toPage", msg);
    var message = {
      message: msg
    };
    BridgeScript._pagePort.postMessage(message);
  };

  BridgeScript.defineApi = function (apiName, methods, events) {
    var eventTriggers = {};
    if (events) {
      events.forEach(function(eventName) {
        eventTriggers[eventName] = function(data) {
          BridgeScript.triggerEvent(apiName, eventName, data);
        };
      });
    }

    BridgeScript._apiImpls[apiName] = methods;

    return eventTriggers;
  };

  BridgeScript.triggerEvent = function (apiName, eventName, payload) {
    var event = {
      event: {
        objectId: apiName,
        name: eventName,
        data: payload
      }
    };
    BridgeScript.toPage(event);
  };

  BridgeScript._pagePort.onMessage(function (e) {
    var data = e.data;
    if (!data || data.sender !== 'Page') {
      return;
    }

    console.log("BridgeScript.onMessage: ", data);

    BridgeScript._pagePort.updateTarget(e);
    var message = data.message;

    if (message === 'connect') {
      BridgeScript.toPage('connected:');
      return;
    }

    var request = message.request;
    if (!request) {
      return;
    }

    var apiSuccessCb = function (response) {
      var msg = {
        id: message.id,
        response: response
      };
      BridgeScript.toPage(msg);
    };

    var apiFailCb = function (e) {
      var msg = {
        id: message.id,
        error: { what: e }
      };
      BridgeScript.toPage(msg);
    };

    var callImplMethod = function(apiName, methodName, payload, successCb, failCb) {
      if (BridgeScript._apiImpls.hasOwnProperty(apiName)) {
        var impl = BridgeScript._apiImpls[apiName];
        if (impl.hasOwnProperty(methodName)) {
          impl[methodName].call(impl, payload, successCb, failCb);
        } else {
          failCb('The ' + methodName + 'API is not implemented for ' + apiName);
        }
      } else {
        failCb('There is no implementation provided for ' + apiName);
      }
    };

    if (request.objectId === "IFrameApiBridge") {
      callImplMethod(request.attrName, 'initialize', request.args,
        function() {
          apiSuccessCb(request.attrName);
        },
        apiFailCb);
    } else {
      callImplMethod(request.objectId, request.attrName, request.args, apiSuccessCb, apiFailCb);
    }
  });

  return {
    registerApi: function (methods) {
      BridgeScript.defineApi('api', methods);
    },

    registerEvents: function (events) {
      return BridgeScript.defineApi('events', {
        initialize: function (params, successCb) {
          successCb();
        }
      }, events);
    }
  };
});