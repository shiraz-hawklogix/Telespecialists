using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;

namespace TeleSpecialists.BLL.ViewModels
{
    public class user_fcm_notification
    {

        private readonly TokenService _tokenservice;
        public user_fcm_notification()
        {
            _tokenservice = new TokenService();
        }

        public string phy_key { get; set; }
        public string msg_title { get; set; }
        public string msg_body { get; set; }
        public string notify_img { get; set; }
        public List<string> phy_tokens { get; set; }


        public object SendToken(string phy_key,string caseType)
        {
            List<token> GetDetail = new List<token>();
            if (caseType == "TwoFactorAuth")
            {
                GetDetail = _tokenservice.GetAllLoggedInUserToken(phy_key);
            }
            else
            {
                GetDetail = _tokenservice.GetAll(phy_key);
            }


            user_fcm_notification user_Fcm_Notification = new user_fcm_notification();
            if (GetDetail.Count > 0)
            {
                var first = GetDetail.FirstOrDefault();
                var tokens = GetDetail.Select(m => m.tok_phy_token).ToList();
                if(caseType != "TwoFactorAuth")
                {
                    user_Fcm_Notification.msg_title = "Stroke Alert";
                    user_Fcm_Notification.msg_body = first.AspNetUser.FirstName + " " + first.AspNetUser.LastName + " You have New Stroke!";
                    user_Fcm_Notification.notify_img = "https://media.graytvinc.com/images/690*387/Stroke+MGN+graphic.JPG";
                } 
                user_Fcm_Notification.phy_key = first.tok_phy_key;
                user_Fcm_Notification.phy_tokens = tokens;
            }
            return user_Fcm_Notification;
            //(user_Fcm_Notification, JsonRequestBehavior.AllowGet);
        }
        public object SendToken(List<string> phy_ids)
        {
            List<Model.token> GetDetail = new List<Model.token>();
            foreach (var id in phy_ids)
            {
                var _GetDetail = _tokenservice.GetAll(id);
                GetDetail.AddRange(_GetDetail);
            }
            
            user_fcm_notification user_Fcm_Notification = new user_fcm_notification();
            if (GetDetail.Count > 0)
            {
                var first = GetDetail.FirstOrDefault();
                var tokens = GetDetail.Select(m => m.tok_phy_token).ToList();

                user_Fcm_Notification.phy_key = first.tok_phy_key;
                user_Fcm_Notification.msg_title = "Stroke Alert";
                user_Fcm_Notification.msg_body = "Interal Blast Stroke Alert Sent !";
                user_Fcm_Notification.notify_img = "https://media.graytvinc.com/images/690*387/Stroke+MGN+graphic.JPG";
                user_Fcm_Notification.phy_tokens = tokens;
            }
            return user_Fcm_Notification;
            //(user_Fcm_Notification, JsonRequestBehavior.AllowGet);
        }



        public bool SendNotification(string phy_key = "", int caseId = 0, string jsonData = "", string caseType = "Open", int action = 1, List<object> Data = null, List<string> phy_ids = null)
        {
            try
            {
                #region Husnain Work on Web Socket
                user_fcm_notification user_Fcm_Notification = new user_fcm_notification();
                // get token from db for testing purpose start
                if (phy_ids != null)
                    user_Fcm_Notification = (user_fcm_notification)SendToken(phy_ids);
                else
                    user_Fcm_Notification = (user_fcm_notification)SendToken(phy_key, caseType);
                WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";

                #region Code for telenotification db
                /*
                //serverKey - Key from Firebase cloud messaging server  
                tRequest.Headers.Add(string.Format("Authorization: key={0}", "AAAAob7gCuE:APA91bFaLo65Xsw1SxE9D6C-OAzo5d0sV3RqEY6E8bnfYseH6xj_0LK0yqfrBCzziPnlzFo9FTQeeB796ie4-zWyV7NsVB8sP0AxWpVf6VqmQR_MWZLiLuD3RrUZY3F9LWUwv09UkQtb"));
                //Sender Id - From firebase project setting  
                tRequest.Headers.Add(string.Format("Sender: id={0}", "BFaRUoKGxUnhw1jL3q-jRuq3X5WwVPB7roHZQKxTUpAfE48sN7G3GWJGGAIIFJHeIuE-DHzqXyeCVe7eG-S1ulQ"));
                */
                #endregion

                #region Code for telecare  db
                //serverKey - Key from Firebase cloud messaging server  
                tRequest.Headers.Add(string.Format("Authorization: key={0}", "AAAA1rwwbPI:APA91bGBQvXGabgDWOHGz8OEu9-yA7w3QhuXQoeAu9TEPGaEemYdoRXp_PRx1IkhYbGqwvb_xSf3LFk_ZErTSZd7HlehYUXeXxnROuL3Y22fspbWUWUwdVRrNtJHZ_dPL1ykSTLnTskS"));
                //Sender Id - From firebase project setting  
                tRequest.Headers.Add(string.Format("Sender: id={0}", "BK9GsbmLr2ohFs7VaIZbzvy67i-3FRtaBeKeAVwEiiuOvk5cRsZOoNKoxUMAQTf_wSSLAumO9c5cb9-KFYj_U4o"));
                #endregion

                #region for telecare production

                //serverKey - Key from Firebase cloud messaging server  
                //tRequest.Headers.Add(string.Format("Authorization: key={0}", "AAAA3Bm3IdE:APA91bEXYQXLck9ZHvar0W7_KnCLUrgn8oJoFs2A5pt4rHaOq3cN6RruzyOrNqPxHgH5DKQfnhwNLocPJJS5AmiUjvC4q0Hv2aHKqlzGkprQyg0ozaGCUgMHWpPi_F0ywywku3W3SfMw"));
                ////Sender Id - From firebase project setting  
                //tRequest.Headers.Add(string.Format("Sender: id={0}", "BCt_gF974whazZ4CfseUT5psM9SlWrO2uR12PtqEYOBzRWrnOzU00nNNVb9RA0hDFh7NDvA89vGqxeM4GVOHf-w"));
                #endregion

                tRequest.ContentType = "application/json";
                var payload = new
                {
                    //to = "eI-JlOt4adcnJ6ip9gucNn:APA91bHAtuuzGNTaSs3wJjjSARj38pnbTNyyL2XLzIWXMobTh4BKc2mEPniwLGNvKOWiVPpbS_I1tJCqbmRLMvrSikV6ZI6Thrrn7GvxAVxgNhvvUqs3cI47eqrEzgG7lt-XUwONEZkr",
                    registration_ids = user_Fcm_Notification.phy_tokens,
                    priority = "high",
                    content_available = true,
                    //notification = new
                    //{
                    //    body = user_Fcm_Notification.msg_body,
                    //    title = user_Fcm_Notification.msg_title,
                    //    click_action = "https://localhost:44304/Case",//"https://telecare.hawklogix.com/Case",
                    //    image = user_Fcm_Notification.notify_img,
                    //    badge = 1
                    //},
                    data = new
                    {
                        phy_key = phy_key,
                        caseId = caseId,
                        caseType = caseType,
                        jsonData = jsonData,
                        action = action,
                        objectData = Data
                    }

                };

                string postbody = JsonConvert.SerializeObject(payload).ToString();
                Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
                tRequest.ContentLength = byteArray.Length;
                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    using (WebResponse tResponse = tRequest.GetResponse())
                    {
                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {
                            if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                {
                                    String sResponseFromServer = tReader.ReadToEnd();
                                    //result.Response = sResponseFromServer;
                                }
                        }
                    }
                }
                #endregion
                return true;
            }
            catch
            {
                return false;
            }
        }

        public object GetobjectOfSendNotification(string phy_key = "", int caseId = 0, string jsonData = "", string caseType = "Open", int action = 1, List<object> Data = null, List<string> phy_ids = null, List<string> strokeDetail = null)
        {
            try
            {
                #region Husnain Work on Web Socket
                user_fcm_notification user_Fcm_Notification = new user_fcm_notification();
                // get token from db for testing purpose start
                if (phy_ids != null)
                    user_Fcm_Notification = (user_fcm_notification)SendToken(phy_ids);
                else
                    user_Fcm_Notification = (user_fcm_notification)SendToken(phy_key,caseType);

                var payload = new
                {
                    registration_ids = user_Fcm_Notification.phy_tokens,
                    priority = "high",
                    content_available = true,
                    data = new
                    {
                        phy_key = phy_key,
                        caseId = caseId,
                        caseType = caseType,
                        jsonData = jsonData,
                        action = action,
                        objectData = Data,
                        caseNumber = strokeDetail[0],
                        fac_name = strokeDetail[1],
                        fac_key = strokeDetail[2],
                        phy_name = strokeDetail[3]
                    }

                };

                return payload;
                #endregion
            }
            catch(Exception e)
            {
                return false;
            }
        }

    }
}
