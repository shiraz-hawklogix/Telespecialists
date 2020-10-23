using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;

namespace TeleSpecialists.BLL.Process.MDStaff
{
    public class MDStaffBase : BaseService
    {
        private string _token;
        private List<ucl_data> _listStates;
        private List<ucl_data> _listFacilityType;

        protected const string _serviceName = "MD Staff Import Service";
        protected EventLogger _logger;
        protected application_setting _appSetting;

        public MDStaffBase()
        {
            _logger = new EventLogger();
        }

        public List<ucl_data> AllStates
        {
            get
            {
                if (_listStates == null)
                    _listStates = new UCLService().GetUclData(UclTypes.State).ToList();

                return _listStates;
            }
        }

        public List<ucl_data> AllFacilityTypes
        {
            get
            {
                if (_listFacilityType == null)
                    _listFacilityType = _unitOfWork.UCL_UCDRepository.Query().ToList();

                return _listFacilityType;
            }
        }

        private async Task InitRequest()
        {
            if (this._appSetting == null)
                this._appSetting = _unitOfWork.AppSettingRepository.Query().FirstOrDefault();

            if (string.IsNullOrEmpty(this._token))
                await RefreshToken();

            if (_listStates == null)
                _listStates = _listStates = new UCLService().GetUclData(UclTypes.State).ToList();
        }

        public async Task<string> Get(string url)
        {
            await InitRequest();

            // replace place holders
            url = url.Replace("[base_url]", this._appSetting.aps_md_base_url);
            url = url.Replace("[instance]", this._appSetting.aps_md_instance);

            // create request
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Headers.Add("Authorization", "bearer " + _token);

            using (var response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private async Task<string> Post(string url, byte[] dataBytes)
        {
            // create request
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "Post";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = dataBytes.Length;

            using (var stream = await request.GetRequestStreamAsync())
            {
                await stream.WriteAsync(dataBytes, 0, dataBytes.Length);
            }

            using (var response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        protected void ResetToken()
        {
            this._token = "";
        }

        public async Task RefreshToken()
        {
            if (!string.IsNullOrEmpty(this._token)) return;

            string url = this._appSetting.aps_md_token_url; // "https://api.mdstaff.com/webapi/api/tokens";

            // data to post for auth
            var postData = "username=" + this._appSetting.aps_md_login + "&password=" + this._appSetting.aps_md_password + "&instance=" + this._appSetting.aps_md_instance + "&facilityid=" + this._appSetting.aps_md_facility + "&grant_type=password";

            // convert to bytes
            var dataBytes = Encoding.UTF8.GetBytes(postData);

            string resultJson = await Post(url, dataBytes);

            if (!string.IsNullOrEmpty(resultJson))
            {
                dynamic jsonObject = JsonConvert.DeserializeObject(resultJson);

                if (jsonObject != null)
                    if (!string.IsNullOrEmpty(Convert.ToString(jsonObject.access_token)))
                        this._token = Convert.ToString(jsonObject.access_token);
            }
        }

        public async Task AddLogEntry(string type, string request_id, string message, string created_by, string provider)
        {
            if (string.IsNullOrEmpty(provider)) provider = Guid.Empty.ToString();

            await _unitOfWork.ExecuteSqlCommandAsync("exec data_import_log_insert @dil_type, @dil_request_id, @dil_provider, @dil_message, @dil_created_by ",
                                                                                    new SqlParameter("@dil_type", type),
                                                                                    new SqlParameter("@dil_request_id", request_id),
                                                                                    new SqlParameter("@dil_provider", provider),
                                                                                    new SqlParameter("@dil_message", message),
                                                                                    new SqlParameter("@dil_created_by", created_by)
                                                                                    );

            // log with sms#if true
            _logger.LogEventWithMonitor(type, message, _serviceName);

            string tempMessage = string.Format("{0} - {1}", type, message);
            if (!tempMessage.Contains(provider)) tempMessage += " - Provider (" + provider + ")";
            Console.WriteLine(tempMessage);
        }
    }
}
