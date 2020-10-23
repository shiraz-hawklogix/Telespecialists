using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Web.WebSockets;
using Microsoft.AspNet.Identity;
using System.Configuration;
using TeleSpecialists.BLL;
using TeleSpecialists.BLL.Helpers;

namespace TeleSpecialists.Web.Hubs
{

    public class WebSocketEventHandler : WebSocketHandler
    {
        public static WebSocketCollection ConnectedClients = new WebSocketCollection();
        public string UserId { get; set; }
        public string ServerUrl { get; set; }

        private WebScrapper scrapper;

        public WebSocketCollection _chatClients
        {
            get
            {
                var allUsers = WebSocketEventHandler.ConnectedClients.Select(x => ((WebSocketEventHandler)x));
                var UsersToRemoveList = allUsers.Where(m => !string.IsNullOrEmpty(m.ServerUrl)).ToList();

                UsersToRemoveList.ForEach(m =>
                {
                    lock (ConnectedClients)
                    {
                        ConnectedClients.Remove(m);
                    }
                });

                string strServerList = ConfigurationManager.AppSettings["ServerUrlList"];
                if (!string.IsNullOrEmpty(strServerList))
                {
                    var ServerList = strServerList.Split(',').ToList();
                    foreach (var server in ServerList)
                    {
                        try
                        {
                            var url = server + "/RpcHandler/GetOnlineUsers?SignalRAuthKey=" + Settings.SignalRAuthKey;
                            var response = scrapper.GetData(url, "GET", "");
                            if (!string.IsNullOrEmpty(response))
                            {
                                var onlineUsers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(response);
                                onlineUsers.ForEach(m =>
                                {
                                    lock (ConnectedClients)
                                    {
                                        ConnectedClients.Add(new WebSocketEventHandler { UserId = m, WebSocketContext = null, ServerUrl = server });
                                    }
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                        }
                    }
                }
                return ConnectedClients;
            }
        }

        public WebSocketEventHandler()
        {
            UserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            scrapper = new WebScrapper();

        }

        public override void OnOpen()
        {
            lock (ConnectedClients)
            {
                ConnectedClients.Add(this);
            }

            base.OnOpen();
        }

        public override void OnClose()
        {
            base.OnClose();
        }


        public void CallJSMethod(List<string> recievers, SocketResponseModel model)
        {

            recievers = recievers.Distinct().ToList();

            var allUsers = _chatClients.Select(x => ((WebSocketEventHandler)x)).ToList();
            var users = allUsers.Where(m => string.IsNullOrEmpty(m.ServerUrl))
                                .Where(x => recievers.Contains(x.UserId)).ToList();
            users.ForEach(client =>
            {
                var data = Functions.EncodeTo64UTF8(Newtonsoft.Json.JsonConvert.SerializeObject(model));
                client.Send(data);
            });

            var otherServerUsers = allUsers.Where(m => recievers.Contains(m.UserId))
                                           .Where(m => !string.IsNullOrEmpty(m.ServerUrl))
                                           .Select(m => new { m.ServerUrl })
                                           .Distinct()
                                           .ToList();

            try
            {
                otherServerUsers.ForEach(s =>
                {

                    var sendRequest = new SocketSendRequestModel { Recievers = recievers, ResponseModel = model };
                    var postData = Newtonsoft.Json.JsonConvert.SerializeObject(sendRequest);
                    var url = s.ServerUrl + "/RpcHandler/CallJsMethod?SignalRAuthKey=" + Settings.SignalRAuthKey;
                    var response = scrapper.GetData(url, "POST", postData, ContentTypes.Json);
                });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        public void CallJSMethod(SocketResponseModel model)
        {
            var _UserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            var allUsers = _chatClients.Select(x => ((WebSocketEventHandler)x)).Where(m => m.UserId != _UserId).ToList();
            var receivers = allUsers.Select(m => m.UserId).Distinct().ToList();
            allUsers.ForEach(client =>
            {
                var data = Functions.EncodeTo64UTF8(Newtonsoft.Json.JsonConvert.SerializeObject(model));
                client.Send(data);
            });

            var otherServerUsers = allUsers
                                           .Where(m => !string.IsNullOrEmpty(m.ServerUrl))
                                           .Select(m => new { m.ServerUrl })
                                           .Distinct()
                                           .ToList();

            try
            {
                otherServerUsers.ForEach(s =>
                {

                    var sendRequest = new SocketSendRequestModel { Recievers = receivers, ResponseModel = model };
                    var postData = Newtonsoft.Json.JsonConvert.SerializeObject(sendRequest);
                    var url = s.ServerUrl + "/RpcHandler/CallJsMethod?SignalRAuthKey=" + Settings.SignalRAuthKey;
                    var response = scrapper.GetData(url, "POST", postData, ContentTypes.Json);
                });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        public void CallJSMethod(string reciever, SocketResponseModel model)
        {
            var list = new List<string>() { reciever }.Distinct().ToList();
            CallJSMethod(list, model);
        }



        //public override void OnMessage(string message)
        //{
        //    var users = _chatClients.Where(x => ((WebSocketEventHandler)x).UserId == UserId).ToList();
        //    users.ForEach(client => {
        //        client.Send(message);
        //    });
        //    //base.OnMessage(message);
        //}
    }


    public class SocketSendRequestModel
    {
        public List<string> Recievers { get; set; }
        public SocketResponseModel ResponseModel { get; set; }
    }

    public class SocketResponseModel
    {
        public string MethodName { get; set; }
        public List<object> Data { get; set; }
    }




    //public class SocketResponseAcceptModel
    //{
    //    public string MethodName { get; set; }
    //    public List<object> Data { get; set; }
    //}

}