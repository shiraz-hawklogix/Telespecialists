define('modules/crm.api', [
    'ui.api.v1',
    'jquery',
    'underscore',
    'models/server/loginState',
    'presentation.models/pres.model.events'
],
    function (UiApi, $, _, LoginState, Events) {
        var CRMApi = {

            CustomEvents: {
                CRM_CMD_HANDLER_REQUEST: 'crm:api:cmdHandlerRequest'
            },

            initialize: function () {
                /**
                 * Initialize CRM engine
                 */
                var configObject = {
                    providerName: 'TeleCare',
                    myCallsTodayEnabled: false,
                    connectorExecutorType: UiApi.ConnectorExecutorType.EMBEDDED,
                    floatingModeType: UiApi.FloatingModeType.DISABLED,
                    guideLink: 'http://webapps.five9.com/assets/files/for_customers/' +
                        'documentation/vcc-applications/agent/agent-desktop-plus-guide.pdf'
                };
                UiApi.config(configObject);

                /**
                 * Initialize CRM method handlers
                 */
                CRMApi.customApi = UiApi.Crm.registerApi(
                    {
                        /**
                         * Standard methods
                         */
                        bringAppToFront: {
                            methodImpl: 'iframe',
                            executorType: UiApi.Crm.ExecutorType.LOCAL
                        },
                        search: {
                            methodImpl: 'iframe',
                            executorType: UiApi.Crm.ExecutorType.REMOTE
                        },
                        saveCallLog: {
                            methodImpl: 'iframe',
                            executorType: UiApi.Crm.ExecutorType.REMOTE
                        },
                        screenPop: {
                            methodImpl: 'iframe',
                            executorType: UiApi.Crm.ExecutorType.REMOTE
                        },
                        getTodayCallsCount: {
                            methodImpl: 'iframe',
                            executorType: UiApi.Crm.ExecutorType.REMOTE
                        },
                        openMyCallsToday: {
                            methodImpl: 'iframe',
                            executorType: UiApi.Crm.ExecutorType.REMOTE
                        },
                        enableClickToDial: {
                            methodImpl: 'iframe',
                            executorType: UiApi.Crm.ExecutorType.LOCAL
                        },
                        disableClickToDial: {
                            methodImpl: 'iframe',
                            executorType: UiApi.Crm.ExecutorType.LOCAL
                        },

                        /*
                         * Command handler responses
                         */
                        cmdHandlerResponse: {
                            methodImpl: 'iframe',
                            executorType: UiApi.Crm.ExecutorType.LOCAL
                        },

                        /**
                         *  Custom methods
                         */
                        adapterStarted: {
                            methodImpl: 'iframe',
                            executorType: UiApi.Crm.ExecutorType.LOCAL
                        },
                        activeSkillsChanged: {
                            methodImpl: 'iframe',
                            executorType: UiApi.Crm.ExecutorType.LOCAL
                        },
                        agentStateChanged: {
                            methodImpl: 'iframe',
                            executorType: UiApi.Crm.ExecutorType.LOCAL
                        },
                        callsChanged: {
                            methodImpl: 'iframe',
                            executorType: UiApi.Crm.ExecutorType.LOCAL
                        },
                        stationStateChanged: {
                            methodImpl: 'iframe',
                            executorType: UiApi.Crm.ExecutorType.LOCAL
                        }
                    });

                /**
                 * Initialize CRM event handlers
                 */
                var events = {};
                /**
                 *  Standard CRM events
                 */
                events[UiApi.Crm.Events.CRM_OBJECT_VISITED] = {
                    source: 'iframe:objectVisited'
                };
                events[UiApi.Crm.Events.CRM_CLICK_2_DIAL] = {
                    source: 'iframe:click2dial'
                };
                events[UiApi.Crm.Events.CRM_SUGGESTED_NUMBERS] = {
                    source: 'iframe:suggestedNumbers'
                };

                /*
                 * Command handler requests
                 */
                events[CRMApi.CustomEvents.CRM_CMD_HANDLER_REQUEST] = {
                    source: 'iframe:cmdHandlerRequest'
                };

                UiApi.Crm.registerEvents(events);

                UiApi.vent.on(Events.WfMainOnModelLoad, this.onWfMainOnModelLoad, this);
                UiApi.vent.on(Events.WfMainOnModelUnload, this.onWfMainOnModelUnload, this);
                UiApi.vent.on(Events.Logout, this.onLogout, this);

                UiApi.vent.on(CRMApi.CustomEvents.CRM_CMD_HANDLER_REQUEST, this.cmdHandlerRequest, this);
            },

            onWfMainOnModelLoad: function () {
                UiApi.Logger.info('crm.api', 'onWfMainOnModelLoad');

                UiApi.Root.Agent(UiApi.Context.AgentId).LoginState().on('change:state', this.onLoginState, this);
                UiApi.Root.Agent(UiApi.Context.AgentId).Presence().on('change:currentState', this.onPresence, this);
                // TODO active skill event doesn't trigger
                UiApi.Root.Agent(UiApi.Context.AgentId).ActiveSkills().on('change', this.onActiveSkills, this);
                UiApi.Root.Agent(UiApi.Context.AgentId).Calls().on('add change', this.onCalls, this);

                setTimeout(_.bind(this.addAgentStationListener, this), 100);

                CRMApi.customApi.adapterStarted();
            },

            onWfMainOnModelUnload: function () {
                UiApi.Logger.info('crm.api', 'onWfMainOnModelUnload');

                UiApi.Root.Agent(UiApi.Context.AgentId).LoginState().off('change:state', this.onLoginState, this);
                UiApi.Root.Agent(UiApi.Context.AgentId).Presence().off('change:currentState', this.onPresence, this);
                UiApi.Root.Agent(UiApi.Context.AgentId).ActiveSkills().off('change', this.onActiveSkills, this);
                UiApi.Root.Agent(UiApi.Context.AgentId).Calls().off('add change', this.onCalls, this);
                if (UiApi.Context.AgentStation) {
                    UiApi.Context.AgentStation.off('stateChanged', this.onAgentStation, this);
                    this.agentStationListening = false
                }
            },

            cmdHandlerRequest: function (data) {
                UiApi.Logger.info('crm.api', 'cmdHandlerRequest', data);

                if (this.hasOwnProperty(data.cmd)) {
                    this[data.cmd](data.data)
                        .done(function (response) {
                            var success = { id: data.id, cmd: data.cmd, response: response };
                            CRMApi.customApi.cmdHandlerResponse(success);
                        })
                        .fail(function (error) {
                            var failure = { id: data.id, cmd: data.cmd, error: error };
                            CRMApi.customApi.cmdHandlerResponse(failure);
                        });
                } else {
                    var errormsg = 'No implemenation for:' + data.cmd;
                    var failure = { id: data.id, cmd: data.cmd, error: errormsg };
                    CRMApi.customApi.cmdHandlerResponse(failure);
                }
            },

            onLogout: function (model) {
                UiApi.Logger.info('crm.api', 'onLogout');

                var data = {
                    agentId: UiApi.Context.AgentId,
                    userName: UiApi.Root.Agent(UiApi.Context.AgentId).get('userName'),
                    loginState: 'LOGOUT'
                };
                CRMApi.customApi.agentStateChanged(data);
            },

            onLoginState: function (model) {
                var state = model.get('state');
                UiApi.Logger.info('crm.api', 'onLoginState', 'state', state);

                UiApi.Root.Agent(UiApi.Context.AgentId).fetch().done(_.bind(function () {
                    var presence = UiApi.Root.Agent(UiApi.Context.AgentId).Presence();
                    var data = {
                        agentId: UiApi.Context.AgentId,
                        userName: UiApi.Root.Agent(UiApi.Context.AgentId).get('userName'),
                        loginState: state,
                        currentState: presence.get('currentState'),
                        currentStateTime: presence.get('currentStateTime')
                    };
                    CRMApi.customApi.agentStateChanged(data);
                }, this));
            },

            onPresence: function (model) {
                UiApi.Logger.info('crm.api', 'onPresence',
                    'readyChannels', model.get('currentState').readyChannels);

                var data = {
                    agentId: UiApi.Context.AgentId,
                    userName: UiApi.Root.Agent(UiApi.Context.AgentId).get('userName'),
                    currentState: model.get('currentState'),
                    currentStateTime: model.get('currentStateTime')
                };
                CRMApi.customApi.agentStateChanged(data);
            },

            agentStationListening: false,

            addAgentStationListener: function () {
                if (UiApi.Root.Agent(UiApi.Context.AgentId).LoginState().get('state') === 'WORKING' && !this.agentStationListening) {
                    if (UiApi.Context.AgentStation) {
                        UiApi.Context.AgentStation.on('stateChanged', this.onAgentStation, this);
                        UiApi.Logger.info('crm.api', 'addAgentStationListener', 'listener added.');
                        this.agentStationListening = true;
                    } else {
                        UiApi.Logger.warn('crm.api', 'addAgentStationListener', 'AgentStation is undefined.');
                    }
                }

                if (!this.agentStationListening) {
                    setTimeout(_.bind(this.addAgentStationListener, this), 1000);
                }
            },

            onAgentStation: function (state) {
                UiApi.Logger.info('crm.api', 'onAgentStation', state);

                var data = {
                    agentId: UiApi.Context.AgentId,
                    userName: UiApi.Root.Agent(UiApi.Context.AgentId).get('userName'),
                    state: state
                };
                CRMApi.customApi.stationStateChanged(data);
            },

            onActiveSkills: function (model) {
                UiApi.Logger.info('crm.api', 'onActiveSkills', model.attributes);
                CRMApi.customApi.activeSkillsChanged(model.attributes);
            },

            onCalls: function (model) {
                UiApi.Logger.info('crm.api', 'onCalls', 'state', model.get('state'), 'id', model.get('id'));

                var data = model.attributes;

                CRMApi.customApi.callsChanged(data);
            },

            /*
             * Command handler implementation methods
             */

            ping: function () {
                UiApi.Logger.info('crm.api', 'ping');
                var deferred = $.Deferred();
                return deferred.resolve('pong').promise();
            },

            getDispositions: function () {
                UiApi.Logger.info('crm.api', 'getDispositions');
                var deferred = $.Deferred();

                try {
                    var activeTask = UiApi.ComputedModels.activeTasksModel().getActiveTask(UiApi.ActiveTaskType.Call);
                    if (!!activeTask) {
                        var callModel = UiApi.Root.Agent(UiApi.Context.AgentId).Calls().get(activeTask.id);

                        if (!!callModel) {
                            var campaignId = callModel.get('campaignId');
                            UiApi.Root.Tenant(Five9.Context.TenantId).Campaigns().fetch()
                                .done(function () {
                                    UiApi.Root.Tenant(Five9.Context.TenantId).Campaigns().get(campaignId).DispositionInfo().fetch()
                                        .done(function (dispositionInfo) {
                                            var data = [];
                                            _.each(dispositionInfo, function (disposition, id) {
                                                var filters = disposition.channelTypes['CALL'].filters;
                                                _.each(filters, function (type, id) {
                                                    if (type === 'VOICE_DISPOSE_CALL_OR_VOICEMAIL') {
                                                        UiApi.Logger.info('crm.api', 'disposition', disposition.id, disposition.displayName);
                                                        data.push({ id: disposition.id, displayName: disposition.displayName });
                                                    }
                                                });

                                            });
                                            deferred.resolve(data);
                                        });
                                });
                        } else {
                            deferred.reject('No active call.');
                        }
                    } else {
                        deferred.reject('No active call.');
                    }
                } catch (e) {
                    deferred.reject(e.message);
                }

                return deferred.promise();
            },

            getSkills: function () {
                UiApi.Logger.info('crm.api', 'getSkills');
                var deferred = $.Deferred();

                try {
                    UiApi.Root.Agent(UiApi.Context.AgentId).Skills().fetch()
                        .done(function (skills) {
                            deferred.resolve(skills);
                        });
                } catch (e) {
                    deferred.reject(e.message);
                }

                return deferred.promise();
            },

            getActiveSkills: function () {
                UiApi.Logger.info('crm.api', 'getActiveSkills');
                var deferred = $.Deferred();

                try {
                    UiApi.Root.Agent(UiApi.Context.AgentId).ActiveSkills().fetch()
                        .done(function (activeSkills) {
                            deferred.resolve(activeSkills);
                        });
                } catch (e) {
                    deferred.reject(e.message);
                }

                return deferred.promise();
            },

            activateSkills: function (data) {
                UiApi.Logger.info('crm.api', 'activateSkills', data);
                var deferred = $.Deferred();

                if (typeof data.skillIds === 'undefined') {
                    return deferred.reject('skillIds required.').promise();
                }

                try {
                    UiApi.Root.Agent(UiApi.Context.AgentId).ActiveSkills().fetch()
                        .done(function () {
                            var newSkills = [];
                            data.skillIds.forEach(function (id) {
                                newSkills.push({ id: id });
                            });
                            var activeSkills = UiApi.Root.Agent(UiApi.Context.AgentId).ActiveSkills();
                            activeSkills.reset(newSkills);
                            activeSkills.save()
                                .done(function (response) {
                                    deferred.resolve(response);
                                })
                                .fail(function (error) {
                                    deferred.reject(error.responseJSON.five9ExceptionDetail.message);
                                });
                        });
                } catch (e) {
                    deferred.reject(e.message);
                }

                return deferred.promise();
            },

            getNotReadyReasonCodes: function () {
                UiApi.Logger.info('crm.api', 'getNotReadyReasonCodes');
                var deferred = $.Deferred();

                try {
                    UiApi.Root.Tenant(UiApi.Context.TenantId).NotReadyReasonCodes().fetch()
                        .done(function (reasonCodes) {
                            deferred.resolve(reasonCodes);
                        });
                } catch (e) {
                    deferred.reject(e.message);
                }

                return deferred.promise();
            },

            getLogoutReasonCodes: function () {
                UiApi.Logger.info('crm.api', 'getLogoutReasonCodes');
                var deferred = $.Deferred();

                try {
                    UiApi.Root.Tenant(UiApi.Context.TenantId).LogoutReasonCodes().fetch()
                        .done(function (reasonCodes) {
                            deferred.resolve(reasonCodes);
                        });
                } catch (e) {
                    deferred.reject(e.message);
                }

                return deferred.promise();
            },

            dispositionCall: function (data) {
                UiApi.Logger.info('crm.api', 'dispositionCall', data);
                var deferred = $.Deferred();

                if (typeof data.dispositionId === 'undefined') {
                    return deferred.reject('dispositionId required.').promise();
                }

                try {
                    var activeTask = UiApi.ComputedModels.activeTasksModel().getActiveTask(UiApi.ActiveTaskType.Call);
                    if (!!activeTask) {
                        UiApi.Root.Agent(UiApi.Context.AgentId).Calls().get(activeTask.id).setDisposition(data.dispositionId);
                    }
                } catch (e) {
                    deferred.reject(e.message);
                }

                return deferred.promise();
            },

            getAgentState: function () {
                UiApi.Logger.info('crm.api', 'getAgentState', data);
                var deferred = $.Deferred();

                try {
                    var model = UiApi.Root.Agent(UiApi.Context.AgentId).Presence();

                    var data = {
                        agentId: UiApi.Context.AgentId,
                        userName: UiApi.Root.Agent(UiApi.Context.AgentId).get('userName'),
                        currentState: model.get('currentState'),
                        currentStateTime: model.get('currentStateTime')
                    };

                    deferred.resolve(data);
                } catch (e) {
                    deferred.reject(e.message);
                }

                return deferred.promise();
            },

            setAgentState: function (data) {
                UiApi.Logger.info('crm.api', 'setAgentState', data);
                var deferred = $.Deferred();

                var presence = UiApi.Root.Agent(UiApi.Context.AgentId).Presence();

                if (typeof data.ready === 'undefined') {
                    return deferred.reject('ready parameter required.').promise();
                }

                if (!presence.isReadyForCall() && data.ready) {
                    presence.setReadyChannels(['CALL', 'VOICE_MAIL']);

                } else if (presence.isReadyForCall() && !data.ready) {
                    presence.setNotReady(data.reasonCode);
                }

                return deferred.promise();
            },

            logoutAgent: function (data) {
                UiApi.Logger.info('crm.api', 'logoutAgent', data);
                var deferred = $.Deferred();

                if (typeof data.logoutReasonId === 'undefined') {
                    deferred.reject('logoutReasonId required.').promise();
                }

                try {
                    UiApi.vent.trigger(Events.StopSession, data.logoutReasonId);
                } catch (e) {
                    deferred.reject(e.message);
                }

                return deferred.promise();
            },

            sendDtmf: function (data) {
                UiApi.Logger.info('crm.api', 'sendDtmf', data);
                var deferred = $.Deferred();

                if (typeof data.digit === 'undefined') {
                    deferred.reject('digit required.').promise();
                }

                try {
                    if (UiApi.Context.AgentStation && UiApi.Context.AgentStation.isSoftphone()) {
                        UiApi.Context.Softphone().sendDtmf(data.digit);
                        deferred.resolve();
                    } else {
                        deferred.reject('sendDtmf not supported.');
                    }
                } catch (e) {
                    deferred.reject(e.message);
                }

                return deferred.promise();
            }
        };

        return CRMApi;
    });

define('workflow/init', ['ui.api.v1', 'modules/crm.api'],
    function (UiApi, CrmApi) {
        return {
            initialize: function () {
                //Place your library initialization code here
                UiApi.Logger.debug('CrmApi:workflow:initialize');

                // Initialize the CRM Shim
                CrmApi.initialize();
            },

            onModelLoad: function () {
                //Place your server model subscription code here
                UiApi.Logger.debug('CrmApi:workflow:onModelLoad');
            },

            onModelUnload: function () {
                //Place your cleanup code here
                UiApi.Logger.debug('CrmApi:workflow:onModelUnload');
            }
        };
    });

define('3rdparty.bundle', [
    'ui.api.v1',
    'handlebars',
    'workflow/init'

    //presentations models

    //components

],
    function (UiApi, Handlebars, Init
    ) {

        UiApi.config({});



        require.config({
            map: {
                '*': {
                }
            }
        });


        Init.initialize();
        UiApi.vent.on(UiApi.PresModelEvents.WfMainOnModelLoad, function () {
            Init.onModelLoad();
        });
        UiApi.vent.on(UiApi.PresModelEvents.WfMainOnModelUnload, function () {
            Init.onModelUnload();
        });
    });
