using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Extensions;

namespace TeleSpecialists.BLL.Helpers
{
    public enum ContentTypes
    {
        [Description("application/x-www-form-urlencoded")]
        UrlEncoded,
        [Description("application/json; charset=utf-8")]
        Json
    }
    public class WebScrapper
    {
        public string baseUrl { get; set; }
        public string ProxyHost { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUserName { get; set; }
        public string ProxyPassword { get; set; }
        public string Exception { get; set; }
        public System.Net.HttpStatusCode ResponseStatusCode { get; set; }
        public Dictionary<string, string> UserAgentsDictionary { get; set; }
        private int Count;
        public WebScrapper()
        {
            Count = 0;
        }
    

        public string GetInitialCookies(string _URL, string _Verb, string _WebRequestPostData, out object cc)
        {
            if (_WebRequestPostData == null) _WebRequestPostData = "";
            string page;
            this.Exception = "";
            CookieContainer _cc = null;
            HttpWebRequest request = null;
            try
            {
                Uri uri = new Uri(_URL);
                request = (HttpWebRequest)WebRequest.Create(uri);

                string postsourcedata = _WebRequestPostData;

                // start added codes
                request.KeepAlive = true;
                request.ProtocolVersion = HttpVersion.Version10;

                request.Method = _Verb;
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postsourcedata.Length;
                request.Proxy = GetProxy();
                request.UseDefaultCredentials = true;
                request.CookieContainer = new CookieContainer();

                request.AllowAutoRedirect = true;
                request.MaximumAutomaticRedirections = 50;
                request.Timeout = (int)new TimeSpan(60, 60, 60).TotalMilliseconds;
                request.UserAgent = GetRandomUserAgents();
                if (_URL.ToLower().Contains("https"))
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; // TLS 1.2
                }
                if (!string.IsNullOrEmpty(postsourcedata) && _Verb == "POST")
                {
                    Stream writeStream = request.GetRequestStream();
                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(postsourcedata);
                    writeStream.Write(bytes, 0, bytes.Length);
                    writeStream.Close();
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                ResponseStatusCode = response.StatusCode;
                Stream responseStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8);

                page = readStream.ReadToEnd();

                _cc = request.CookieContainer;
            }
            catch (Exception ee)
            {
                ResponseStatusCode = HttpStatusCode.InternalServerError;
                this.Exception = ee.Message;
             
                page = "Failure  " + ee.ToString();
            }

            cc = _cc;

            return page;

        }

      

        public string GetData(string _URL, string _Verb, string _WebRequestPostData, ref object cc, ContentTypes ContentType = ContentTypes.UrlEncoded)
        {

            string page;
            this.Exception = "";
            HttpWebRequest request = null;
            try
            {
                HttpWebResponse response = GetResponse(_URL, _Verb, _WebRequestPostData, ref cc, ref request, ContentType);
                ResponseStatusCode = response.StatusCode;
                Stream responseStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8);

                page = readStream.ReadToEnd();

            }
            catch (Exception ee)
            {
                ResponseStatusCode = HttpStatusCode.InternalServerError;                
                this.Exception = ee.Message;
                page = "Failure " + ee.ToString();
            }

            return page;


        }

        public string GetData(string _URL, string _Verb, string _WebRequestPostData, ContentTypes ContentType = ContentTypes.UrlEncoded)
        {

            string page;
            this.Exception = "";
            object cc = null;
            HttpWebRequest request = null;
            try
            {
                HttpWebResponse response = GetResponse(_URL, _Verb, _WebRequestPostData, ref cc, ref request, ContentType);
                ResponseStatusCode = response.StatusCode;
                Stream responseStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8);

                page = readStream.ReadToEnd();

            }
            catch (Exception ee)
            {
                ResponseStatusCode = HttpStatusCode.InternalServerError;
                this.Exception = ee.Message;
                page = "Failure " + ee.ToString();
            }

            return page;


        }

        #region Private Members


        private HttpWebResponse GetResponse(string _URL, string _Verb, string _WebRequestPostData, ref object cc, ref HttpWebRequest request, ContentTypes ContentType = ContentTypes.UrlEncoded, int TimeOutMinutes = 60)
        {
            if (_WebRequestPostData == null) _WebRequestPostData = "";


            Uri uri = new Uri(_URL);
            request = (HttpWebRequest)WebRequest.Create(uri);

            if (_URL.ToLower().Contains("https"))
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; // TLS 1.2
            }

            string postsourcedata = _WebRequestPostData;

            // start added codes
            request.KeepAlive = true;
            request.ProtocolVersion = HttpVersion.Version10;

            request.Method = _Verb;
            request.ContentType = ContentType.ToDescription();
            request.ContentLength = postsourcedata.Length;

            request.Proxy = GetProxy();
            request.UseDefaultCredentials = true;

            if (cc != null)
                setCookies(ref request, cc);
            else
                request.CookieContainer = new CookieContainer();
            request.AllowAutoRedirect = true;
            request.MaximumAutomaticRedirections = 50;

            request.Timeout = (int)new TimeSpan(60, TimeOutMinutes, 60).TotalMilliseconds;
            request.UserAgent = GetRandomUserAgents();
            
            if (!string.IsNullOrEmpty(postsourcedata) && _Verb == "POST")
            {
                Stream writeStream = request.GetRequestStream();
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(postsourcedata);
                writeStream.Write(bytes, 0, bytes.Length);
                writeStream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response;
        }
        private void setCookies(ref HttpWebRequest request, object cc)
        {
            CookieCollection cookies = ((CookieContainer)cc).GetCookies(new Uri(baseUrl));
            if (cookies.Count > 0)
            {
                request.CookieContainer = (CookieContainer)cc;

                for (int i = 0; i < cookies.Count; i++)
                {
                    Cookie c = cookies[i];
                    request.Headers.Add(c.Name, c.Value);
                }
            }
            else
            {
                request.CookieContainer = new CookieContainer();
            }
        }

        private string GetRandomUserAgents()
        {

            string strdefault = "Mozilla / 5.0(Windows NT 6.3; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 66.0.3359.170 Safari / 537.36 OPR / 53.0.2907.110"; ;

            if (UserAgentsDictionary != null)
            {
                if (Count < UserAgentsDictionary.Count())
                {

                    strdefault = UserAgentsDictionary.ElementAt(Count).Value;
                    Count++;
                }
                else
                {
                    Count = 0;
                    strdefault = UserAgentsDictionary.ElementAt(Count).Value;
                    Count++;
                }
            }

            return strdefault;
        }

        private System.Net.WebProxy GetProxy()
        {

            if (this.ProxyHost != "" && this.ProxyPort > 0)
            {

                System.Net.WebProxy proxy = new System.Net.WebProxy(this.ProxyHost, this.ProxyPort);
                proxy.BypassProxyOnLocal = false;

                //proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

                if (!string.IsNullOrEmpty(ProxyUserName) && !string.IsNullOrEmpty(ProxyPassword))
                {
                    ICredentials cred = new NetworkCredential(ProxyUserName, ProxyPassword);
                    proxy.Credentials = cred;
                }


                return proxy;
            }
            else
            {
                return System.Net.WebProxy.GetDefaultProxy();
            }


        }
        #endregion
    }
}
