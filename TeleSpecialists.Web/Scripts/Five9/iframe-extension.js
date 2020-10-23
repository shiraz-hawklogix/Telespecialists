/*
 * Five9 Extension library
 *
 * @requires jQuery and Bootstrap css
 *
 * Copyright (c)2016-17 Five9, Inc.
 */
(function (factory) {
    if (typeof define === "function" && define.amd) {
        define(factory);
    } else if (typeof module === "object" && typeof module.exports === "object") {
        module.exports = factory();
    } else {
        window.Five9 = window.Five9 || {};
        window.Five9.ExtensionLib = factory();
    }
})(function () {

    $(document).ready(function () {
        // register Five9 iframe API methods and events
        Five9.IframeApi.registerApi(IframeClient);
        IframeClient.eventTriggers = Five9.IframeApi.registerEvents([
            // standard events
            'click2dial',
            'callsChanged',
            'cmdHandlerRequest'
        ]);

        // register custom command handlers
        IframeClient.registerCmd([
            'ping',
            'dispositionCall',
            'getAgentState',
            'setAgentState',
            'logoutAgent',
            'getDispositions',
            'getSkills',
            'getActiveSkills',
            'activateSkills',
            'getNotReadyReasonCodes',
            'getLogoutReasonCodes',
            'sendDtmf'
        ]);
    });

    var IframeClient = {

        cmdApi: {},
        cmdQueue: {},
        cmdIndex: 0,

        /**
         * Registers custom command handler methods
         *
         * @param methods
         */
        registerCmd: function (methods) {
            if (methods) {
                methods.forEach(function (methodName) {
                    IframeClient.cmdApi[methodName] = function (data) {
                        return IframeClient.sendCmd(methodName, data);
                    };
                });
            }
        },

        /**
         * Sends custom command handler request via iframe client
         *
         * @param cmd
         * @param data
         * @returns {*|{then, fail}}
         */
        sendCmd: function (cmd, data) {
            console.log('IframeClient.sendCmd: ' + cmd + ' ' + JSON.stringify(data));
            var deferred = $.Deferred();
            var id = ++this.cmdIndex;
            var request = { id: id, cmd: cmd, data: data };

            this.cmdQueue[id] = deferred;
            IframeClient.eventTriggers.cmdHandlerRequest(request);

            return deferred.promise();
        },

        /**
         * Command handler response handling method
         *
         * @param params
         * @param successCb
         * @param failCb
         */
        cmdHandlerResponse: function (params, successCb, failCb) {
            console.log('IframeClient.cmdHandlerResponse: ' + JSON.stringify(params));
            if (!params) return;
            var id = params.id;
            if (!id || !this.cmdQueue.hasOwnProperty(id)) return;
            var deferred = this.cmdQueue[id];

            if (params.error) {
                deferred.reject(params.error);
            } else {
                deferred.resolve(params.response);
            }

            delete this.cmdQueue[id];
            successCb('ok');
        },

        initialize: function (params, successCb) {
            console.log('IframeClient.initialize()');
            successCb('ok');
        },

        /**
         * Standard methods
         */
        bringAppToFront: function (params, successCb, failCb) {
            console.log('IframeClient.bringAppToFront: ' + JSON.stringify(params));
            raiseEvent('bringAppToFront', params, successCb, failCb);
        },

        search: function (params, successCb, failCb) {
            console.log('IframeClient.search: ' + JSON.stringify(params));
            setLastEvent('ExtensionLib.saveCallLog' + JSON.stringify(params));            
            raiseEvent('search', params, successCb, failCb);
        },

        saveCallLog: function (params, successCb, failCb) {
            console.log('IframeClient.saveCallLog: ' + JSON.stringify(params));
            raiseEvent('saveCallLog', params, successCb, failCb);
        },

        screenPop: function (params, successCb, failCb) {
            console.log('IframeClient.screenPop: ' + JSON.stringify(params));
            raiseEvent('screenPop', params, successCb, failCb);
        },

        getTodayCallsCount: function (params, successCb, failCb) {
            console.log('IframeClient.getTodayCallsCount');
            raiseEvent('getTodayCallsCount', params, successCb, failCb);
        },

        openMyCallsToday: function (params, successCb, failCb) {
            console.log('IframeClient.openMyCallsToday');
            raiseEvent('openMyCallsToday', params, successCb, failCb);
        },

        enableClickToDial: function (params, successCb, failCb) {
            console.log('IframeClient.enableClickToDial');
            raiseEvent('enableClickToDial', params, successCb, failCb);
        },

        disableClickToDial: function (params, successCb, failCb) {
            console.log('IframeClient.disableClickToDial');
            raiseEvent('disableClickToDial', params, successCb, failCb);
        },

        /**
         *  Custom methods
         */
        adapterStarted: function (params, successCb, failCb) {
            console.log('IframeClient.adapterStarted: ' + JSON.stringify(params));
            raiseEvent('adapterStarted', params, successCb, failCb);
        },

        agentStateChanged: function (params, successCb, failCb) {
            console.log('IframeClient.agentStateChanged: ' + JSON.stringify(params));
            raiseEvent('agentStateChanged', params, successCb, failCb);
        },

        activeSkillsChanged: function (params, successCb, failCb) {
            console.log('IframeClient.activeSkillsChanged: ' + JSON.stringify(params));
            raiseEvent('activeSkillsChanged', params, successCb, failCb);
        },

        callsChanged: function (params, successCb, failCb) {

            console.log('IframeClient.callsChanged: ' + JSON.stringify(params));
            raiseEvent('callsChanged', params, successCb, failCb);
        },

        stationStateChanged: function (params, successCb, failCb) {
            console.log('IframeClient.stationStateChanged: ' + JSON.stringify(params));
            raiseEvent('stationStateChanged', params, successCb, failCb);
        }
    };

    var _listener;

    function raiseEvent(event, params, successCb, failCb) {
        try {
            if (_listener && _listener.hasOwnProperty(event)) {
                successCb(_listener[event].call(_listener, params));
            } else {
                //throw new Error(event + ' not implemented.');
                console.warn(event + ' not implemented.');
            }
        } catch (e) {
            console.warn('raiseEvent: ' + e);
            failCb(e);
        }
    }

    return {
        setListener: function (listener) {
            _listener = listener;
        },

        /**
         *  Standard methods
         */

        click2dial: function (crmC2DInfo) {

            IframeClient.eventTriggers.click2dial(crmC2DInfo);
        },

        objectVisited: function (crmObject) {
            IframeClient.eventTriggers.objectVisited(crmObject);
        },

        suggestedNumbers: function (suggestedNumbersInfo) {
            IframeClient.eventTriggers.suggestedNumbers(suggestedNumbersInfo);
        },

        /**
         *  Custom command methods
         */

        ping: function () {
            return IframeClient.cmdApi.ping();
        },

        dispositionCall: function (dispositionId) {
            return IframeClient.cmdApi.dispositionCall({ dispositionId: dispositionId });
        },

        getAgentState: function () {
            return IframeClient.cmdApi.getAgentState();
        },

        setAgentState: function (ready, reasonCode) {
            return IframeClient.cmdApi.setAgentState({ ready: ready, reasonCode: reasonCode || '0' });
        },

        logoutAgent: function (logoutReasonId) {
            return IframeClient.cmdApi.logoutAgent({ logoutReasonId: logoutReasonId });
        },

        getDispositions: function () {
            return IframeClient.cmdApi.getDispositions();
        },

        getSkills: function () {
            return IframeClient.cmdApi.getSkills();
        },

        getActiveSkills: function () {
            return IframeClient.cmdApi.getActiveSkills();
        },

        activateSkills: function (skillIds) {
            return IframeClient.cmdApi.activateSkills({ skillIds: skillIds });
        },

        getNotReadyReasonCodes: function () {
            return IframeClient.cmdApi.getNotReadyReasonCodes();
        },

        getLogoutReasonCodes: function () {
            return IframeClient.cmdApi.getLogoutReasonCodes();
        },

        sendDtmf: function (digit) {
            return IframeClient.cmdApi.sendDtmf({ digit: digit });
        }
    };
});


/*
 * Five9 ADT iframe communication library
 * version: $f9build-version
 *
 * Copyright (c)2016 Five9, Inc.
 */
(function (factory) {
    if (typeof define === "function" && define.amd) {
        define(factory);
    } else if (typeof module === "object" && typeof module.exports === "object") {
        module.exports = factory();
    } else {
        window.Five9 = window.Five9 || {};
        window.Five9.IframeApi = factory();
    }
})(function () {
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
            events.forEach(function (eventName) {
                eventTriggers[eventName] = function (data) {
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

        var callImplMethod = function (apiName, methodName, payload, successCb, failCb) {
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
                function () {
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

