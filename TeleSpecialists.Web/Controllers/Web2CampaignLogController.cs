using System;
using System.Web.Mvc;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;

namespace TeleSpecialists.Controllers
{
    public class Web2CampaignLogController : BaseController
    {
        // GET: Web2CampaignLog
        private readonly Web2CampaignLogService _web2CampaignLogService;

        public Web2CampaignLogController()
        {
            _web2CampaignLogService = new Web2CampaignLogService();
        }

        [HttpPost]
        public ActionResult Add(web2campaign_log model)
        {
            try
            {
                var currentESTDate = DateTime.Now.ToEST();
                model.wcl_created_by = loggedInUser.Id;
                model.wcl_created_by_name = loggedInUser.FullName;
                model.wcl_created_date = currentESTDate;
                model.wcl_request_send_time = currentESTDate;
                model.wcl_user_agent = Request.UserAgent;
                _web2CampaignLogService.Create(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public ActionResult Update(web2campaign_log model)
        {
            var log = _web2CampaignLogService.GetByCaseKey(model.wcl_cas_key.Value);

            if (log != null)
            {
                log.wcl_browser_name = model.wcl_browser_name;
                log.wcl_error_code = model.wcl_error_code;
                log.wcl_error_description = model.wcl_error_description;
                if (model.wcl_error_code.ToLower().Contains("tc") == false)
                {
                    log.wcl_raw_result = Functions.DecodeFrom64(model.wcl_raw_result);
                    log.wcl_response_received_time = DateTime.Now.ToEST();
                }
                _web2CampaignLogService.Edit(log);
                return Json(new { success = true });
            }
            else
            {
                return Add(model);
            }
            
        }
        #region ----- Disposable -----
        private bool disposed = false; // to detect redundant calls
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // dispose-only, i.e. non-finalizable logic
                    _web2CampaignLogService?.Dispose();

                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
