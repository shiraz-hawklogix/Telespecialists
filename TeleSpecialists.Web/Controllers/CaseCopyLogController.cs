using System;
using System.Web.Mvc;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;

namespace TeleSpecialists.Controllers
{
    public class CaseCopyLogController : BaseController
    {
        private readonly CaseCopyLogService _caseCopyLogService;
        public CaseCopyLogController()
        {
            _caseCopyLogService = new CaseCopyLogService();
        }
        public ActionResult Add(case_copy_log model)
        {
            try
            {
                model.cpy_copied_text = model.cpy_copied_text;
                model.cpy_created_by = loggedInUser.Id;
                model.cpy_created_by_name = loggedInUser.FullName;
                model.cpy_created_date_est = DateTime.Now.ToEST();
                model.cpy_user_agent = Request.UserAgent;
                _caseCopyLogService.Create(model);
                return Json(new { success = true });
            }
            catch(Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false });
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
                    _caseCopyLogService?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}